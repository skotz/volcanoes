using System;
using TorchSharp;
using Volcano.Game;
using static TorchSharp.torch;

namespace Volcano.Neural
{
    public class GraphResNet : torch.nn.Module<Tensor, Tuple<Tensor, Tensor>>
    {
        // GCNII paper range is 0.1-0.2. Higher values let h0 (which has no neighborhood
        // information) dominate every layer and make depth nearly useless.
        private const float _alpha = 0.15f;

        // One adjacency per counter-clockwise neighbor slot (Constants.CounterClockwiseNeighbors index 0, 1, 2)
        internal const int relations = 3;

        private readonly int width;
        private Tensor adjacency;
        private TorchSharp.Modules.Sequential inputBlock;
        private TorchSharp.Modules.LayerNorm finalNorm;
        private TorchSharp.Modules.ModuleList<GCNIILayer> backBone;
        private TorchSharp.Modules.Sequential policyHead;
        private TorchSharp.Modules.Sequential valueHead;

        private class GCNIILayer : torch.nn.Module<Tensor, Tensor>
        {
            private readonly int width;
            private readonly float beta;
            private readonly float alpha;
            private readonly Func<Tensor> adjacency;
            private readonly TorchSharp.Modules.Linear selfWeight;
            private readonly TorchSharp.Modules.Linear relWeight;
            private readonly TorchSharp.Modules.Linear weight;
            private readonly TorchSharp.Modules.LayerNorm norm;

            public GCNIILayer(string name, int width, float beta, float alpha, Func<Tensor> adjacency) : base(name)
            {
                this.width = width;
                this.beta = beta;
                this.alpha = alpha;
                this.adjacency = adjacency;
                selfWeight = torch.nn.Linear(width, width, hasBias: false);

                // Equivalent to a separate weight matrix per relation, summed
                relWeight = torch.nn.Linear(width * relations, width, hasBias: false);
                weight = torch.nn.Linear(width, width, hasBias: false);
                norm = torch.nn.LayerNorm(new long[] { width });
                RegisterComponents();
            }

            public override Tensor forward(Tensor input)
            {
                using (var scope = torch.NewDisposeScope())
                {
                    var h = input.narrow(2, 0, width);
                    var h0 = input.narrow(2, width, width);

                    var hNorm = norm.forward(h);
                    var parts = new Tensor[relations];
                    for (var r = 0; r < relations; r++)
                    {
                        parts[r] = torch.matmul(adjacency().select(0, r), hNorm);
                    }

                    var ax = selfWeight.forward(hNorm) + relWeight.forward(torch.cat(parts, 2));

                    // Multiplied explicitly using unified float logic
                    var support = (ax * (1.0f - alpha)) + (h0 * alpha);

                    var linearOut = weight.forward(support);
                    var update = (support * (1.0f - beta)) + (linearOut * beta);

                    var activatedUpdate = torch.nn.functional.relu(update);
                    var nextH = h + activatedUpdate;

                    var result = torch.cat(new Tensor[] { nextH, h0 }, dim: 2);

                    return result.MoveToOuterDisposeScope();
                }
            }
        }

        public GraphResNet(int numLayers, int width, Device device) : base("GraphResNet")
        {
            this.width = width;
            adjacency = BuildTypedAdjacency().to(device);

            // LayerNorm keeps h and h0 at a consistent scale across the residual stack.
            inputBlock = torch.nn.Sequential(torch.nn.Linear(GraphEncoder.features, width),
                                             torch.nn.LayerNorm(new long[] { width }),
                                             torch.nn.ReLU());
            finalNorm = torch.nn.LayerNorm(new long[] { width });
            backBone = torch.nn.ModuleList<GCNIILayer>();

            for (int l = 1; l <= numLayers; l++)
            {
                // Explicitly cast beta calculation to float
                var beta = (float)Math.Log(0.5 / l + 1);
                backBone.append(new GCNIILayer("GCNII_" + l.ToString(), width, beta, _alpha, () => adjacency));
            }

            // Enhanced Policy Head Feature Squeeze Pipeline:
            // Forcing it down through an intentional channel bottleneck prevents
            // representation collapse right before the move logit mapping.
            // Input is each node's features concatenated with the global mean and max, so every
            // move logit can see whole-board context and threats anywhere on the sphere.
            policyHead = torch.nn.Sequential(
                torch.nn.Linear(width * 3, width),
                torch.nn.ReLU(),
                torch.nn.Linear(width, 1)
            );

            valueHead = torch.nn.Sequential(torch.nn.Linear(width * 2, 64),
                                            torch.nn.ReLU(),
                                            torch.nn.Linear(64, 1),
                                            torch.nn.Tanh());
            RegisterComponents();
            this.to(device);
        }

        /// <summary>
        /// [relations, 80, 80] where A_k[t, CounterClockwiseNeighbors[t][k]] = 1
        /// </summary>
        internal static Tensor BuildTypedAdjacency()
        {
            var size = GraphEncoder.nodes;
            var data = new float[relations * size * size];

            for (var k = 0; k < relations; k++)
            {
                for (var t = 0; t < size; t++)
                {
                    data[(k * size + t) * size + Constants.CounterClockwiseNeighbors[t][k]] = 1.0f;
                }
            }

            return torch.tensor(data, [relations, size, size]);
        }

        public override Tuple<Tensor, Tensor> forward(Tensor input)
        {
            // Enclosing the primary forward pass inside a clean DisposeScope
            // ensures intermediate activations don't leak memory during self-play.
            using (var scope = torch.NewDisposeScope())
            {
                var initial = inputBlock.forward(input);
                var state = cat(new[] { initial, initial }, 2);

                foreach (GCNIILayer layer in backBone)
                {
                    state = layer.forward(state);
                }

                var hidden = finalNorm.forward(state.narrow(2, 0, width));
                var mean = hidden.mean(new long[] { 1 }, keepdim: true);
                var max = hidden.amax(new long[] { 1 }, keepdim: true);

                // Step 1: Run through the newly fortified policy sequential blocks
                var policyRaw = policyHead.forward(cat(new[] { hidden, mean.expand_as(hidden), max.expand_as(hidden) }, 2));

                // Step 2: SAFE SQUEEZE. Drops ONLY the final singleton feature dimension.
                // This yields an absolute, un-scrambled mapping of shape (BatchSize, 80)
                var policy = policyRaw.squeeze(2);

                var pooled = cat(new[] { mean.squeeze(1), max.squeeze(1) }, 1);
                var value = valueHead.forward(pooled);

                // Move the outputs safely out of the local scope before it terminates
                return new Tuple<Tensor, Tensor>(policy.MoveToOuterDisposeScope(), value.MoveToOuterDisposeScope());
            }
        }
    }
}
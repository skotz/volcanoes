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

        private readonly int width;
        private readonly Tensor adjacency;
        private TorchSharp.Modules.Sequential inputBlock;
        private TorchSharp.Modules.LayerNorm finalNorm;
        private TorchSharp.Modules.Parameter positionEmbedding;
        private TorchSharp.Modules.ModuleList<GCNIILayer> backBone;
        private TorchSharp.Modules.Sequential policyHead;
        private TorchSharp.Modules.Sequential valueHead;

        private class GCNIILayer : torch.nn.Module<Tensor, Tensor>
        {
            private readonly int width;
            private readonly float beta;
            private readonly float alpha;
            private readonly Tensor adjacency;
            private readonly TorchSharp.Modules.Linear weight;
            private readonly TorchSharp.Modules.LayerNorm norm;

            public GCNIILayer(string name, int width, float beta, float alpha, Tensor adjacency) : base(name)
            {
                this.width = width;
                this.beta = beta;
                this.alpha = alpha;
                this.adjacency = adjacency;
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
                    var ax = torch.matmul(adjacency, hNorm);

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
            adjacency = BuildNormalizedAdjacency().to(device);

            // LayerNorm keeps h and h0 at a consistent scale across the residual stack.
            inputBlock = torch.nn.Sequential(torch.nn.Linear(GraphEncoder.features, width),
                                             torch.nn.LayerNorm(new long[] { width }),
                                             torch.nn.ReLU());
            finalNorm = torch.nn.LayerNorm(new long[] { width });

            // Learned per-node embedding, shape [1, nodes, width], broadcast over the batch.
            positionEmbedding = torch.nn.Parameter(torch.randn(new long[] { 1, GraphEncoder.nodes, width }) * 0.1);
            register_parameter(nameof(positionEmbedding), positionEmbedding);
            backBone = torch.nn.ModuleList<GCNIILayer>();

            for (int l = 1; l <= numLayers; l++)
            {
                // Explicitly cast beta calculation to float
                var beta = (float)Math.Log(0.5 / l + 1);
                backBone.add_module("GCNII_" + l.ToString(), new GCNIILayer("GCNII_" + l.ToString(), width, beta, _alpha, adjacency));
            }

            // Enhanced Policy Head Feature Squeeze Pipeline:
            // Forcing it down through an intentional channel bottleneck prevents
            // representation collapse right before the move logit mapping.
            // Input is each node's features concatenated with the global mean, so every
            // move logit can see whole-board context.
            policyHead = torch.nn.Sequential(
                torch.nn.Linear(width * 2, width),
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

        internal static Tensor BuildNormalizedAdjacency()
        {
            var size = GraphEncoder.nodes;
            var adjacent = new float[size * size];
            var degree = new float[size];

            for (var t = 0; t < size; t++)
            {
                adjacent[t * size + t] = 1.0f;
                foreach (var a in Constants.AdjacentIndexes[t])
                {
                    adjacent[t * size + a] = 1.0f;
                }
            }

            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    degree[i] += adjacent[i * size + j];
                }
            }

            var normalized = new float[size * size];
            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    normalized[i * size + j] = adjacent[i * size + j] / (float)Math.Sqrt(degree[i] * degree[j]);
                }
            }

            return torch.tensor(normalized, [size, size]);
        }

        public override Tuple<Tensor, Tensor> forward(Tensor input)
        {
            // Enclosing the primary forward pass inside a clean DisposeScope
            // ensures intermediate activations don't leak memory during self-play.
            using (var scope = torch.NewDisposeScope())
            {
                var initial = inputBlock.forward(input) + positionEmbedding;
                var state = cat(new[] { initial, initial }, 2);

                foreach (GCNIILayer layer in backBone)
                {
                    state = layer.forward(state);
                }

                var hidden = finalNorm.forward(state.narrow(2, 0, width));
                var globalContext = hidden.mean(new long[] { 1 }, keepdim: true).expand_as(hidden);

                // Step 1: Run through the newly fortified policy sequential blocks
                var policyRaw = policyHead.forward(cat(new[] { hidden, globalContext }, 2));

                // Step 2: SAFE SQUEEZE. Drops ONLY the final singleton feature dimension.
                // This yields an absolute, un-scrambled mapping of shape (BatchSize, 80)
                var policy = policyRaw.squeeze(2);

                var pooled = cat(new[] { hidden.mean(new long[] { 1 }), hidden.amax(new long[] { 1 }) }, 1);
                var value = valueHead.forward(pooled);

                // Move the outputs safely out of the local scope before it terminates
                return new Tuple<Tensor, Tensor>(policy.MoveToOuterDisposeScope(), value.MoveToOuterDisposeScope());
            }
        }
    }
}
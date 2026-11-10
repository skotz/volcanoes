using System;
using TorchSharp;
using Volcano.Game;
using static TorchSharp.torch;

namespace Volcano.Neural
{
    public class GraphResNet : torch.nn.Module<Tensor, Tuple<Tensor, Tensor>>
    {
        private const double _alpha = 0.1;

        private readonly int width;
        private readonly Tensor adjacency;
        private TorchSharp.Modules.Sequential inputBlock;
        private TorchSharp.Modules.ModuleList<GCNIILayer> backBone;
        private TorchSharp.Modules.Sequential policyHead;
        private TorchSharp.Modules.Sequential valueHead;

        /// <summary>
        /// GCNII layer. Input is [batch, node, 2 * width] where the first half is the current hidden state
        /// and the second half is the initial embedding, which is carried through unchanged.
        /// </summary>
        private class GCNIILayer : torch.nn.Module<Tensor, Tensor>
        {
            private readonly int width;
            private readonly double beta;
            private readonly Tensor adjacency;
            private readonly TorchSharp.Modules.Linear weight;
            private readonly TorchSharp.Modules.LayerNorm norm;

            public GCNIILayer(string name, int width, double beta, Tensor adjacency) : base(name)
            {
                this.width = width;
                this.beta = beta;
                this.adjacency = adjacency;
                weight = torch.nn.Linear(width, width, hasBias: false);
                norm = torch.nn.LayerNorm(new long[] { width });
                RegisterComponents();
            }

            public override Tensor forward(Tensor input)
            {
                var h = input.narrow(2, 0, width);
                var h0 = input.narrow(2, width, width);

                var support = matmul(adjacency, h) * (1 - _alpha) + h0 * _alpha;
                var output = support * (1 - beta) + weight.forward(support) * beta;
                output = torch.nn.functional.relu(norm.forward(output));

                return cat(new[] { output, h0 }, 2);
            }
        }

        public GraphResNet(int numLayers, int width, Device device) : base("GraphResNet")
        {
            this.width = width;
            adjacency = BuildNormalizedAdjacency().to(device);

            inputBlock = torch.nn.Sequential(torch.nn.Linear(GraphEncoder.features, width),
                                             torch.nn.ReLU());
            backBone = torch.nn.ModuleList<GCNIILayer>();

            for (int l = 1; l <= numLayers; l++)
            {
                var beta = Math.Log(0.5 / l + 1);
                backBone.add_module("GCNII_" + l.ToString(), new GCNIILayer("GCNII_" + l.ToString(), width, beta, adjacency));
            }

            policyHead = torch.nn.Sequential(torch.nn.Linear(width, 1));
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

        /// <summary>
        /// input [batch, 80, 10] -> policy logits [batch, 80], value [batch, 1]
        /// </summary>
        public override Tuple<Tensor, Tensor> forward(Tensor input)
        {
            var initial = inputBlock.forward(input);
            var state = cat(new[] { initial, initial }, 2);

            foreach (GCNIILayer layer in backBone)
            {
                state = layer.forward(state);
            }

            var hidden = state.narrow(2, 0, width);
            var policy = policyHead.forward(hidden).reshape(input.shape[0], GraphEncoder.nodes);

            var pooled = cat(new[] { hidden.mean(new long[] { 1 }), hidden.amax(new long[] { 1 }) }, 1);
            var value = valueHead.forward(pooled);

            return new Tuple<Tensor, Tensor>(policy, value);
        }
    }
}
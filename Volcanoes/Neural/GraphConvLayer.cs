using System;
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;

namespace Volcano.Neural
{
    public class GraphConvLayer : Module<Tensor, Tensor>
    {
        private readonly Tensor _adjacencyMatrix;
        private readonly long _inFeatures;
        private readonly long _outFeatures;

        public Parameter Weight { get; private set; }

        public Parameter Bias { get; private set; }

        public GraphConvLayer(long inFeatures, long outFeatures, Tensor adjacencyMatrix)
            : base("GraphConvLayer")
        {
            _inFeatures = inFeatures;
            _outFeatures = outFeatures;
            _adjacencyMatrix = adjacencyMatrix.clone();

            // 1. Initialize the tensors and wrap them as a Parameter
            var wTensor = torch.randn([inFeatures, outFeatures]) * Math.Sqrt(2.0 / inFeatures);
            Weight = nn.Parameter(wTensor);

            // 2. Register using the exact v0.107 syntax (lowercase matching PyTorch)
            register_parameter(nameof(Weight), Weight);

            // 3. Crucial step: Tells TorchSharp to bind all fields and registered elements
            RegisterComponents();
        }

        public override Tensor forward(Tensor x)
        {
            // x shape: [Batch, 80, 10]
            long batchSize = x.shape[0];

            // 1. Linear Transformation: Broadcasts perfectly across the batch
            // [Batch, 80, 10] x [10, OutFeatures] -> [Batch, 80, OutFeatures]
            var support = torch.matmul(x, Weight);

            // 2. Expand the Adjacency Matrix to match the batch size exactly
            // From: [80, 80] -> [1, 80, 80] -> [Batch, 80, 80]
            var adjExpanded = _adjacencyMatrix.unsqueeze(0).expand(new long[] { batchSize, 80, 80 });

            // 3. Batch Matrix Multiplication: Both tensors now have the same Batch dimension!
            // [Batch, 80, 80] x [Batch, 80, OutFeatures] -> [Batch, 80, OutFeatures]
            var output = torch.bmm(adjExpanded, support);

            return output;
        }
    }

#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.

    public static class skotz
    {
        public static class nn
        {
            public static GraphConvLayer ConvGraph(long inFeatures, long outFeatures, Tensor boardTopology)
            {
                return new GraphConvLayer(inFeatures, outFeatures, boardTopology);
            }
        }
    }

#pragma warning restore CS8981
}
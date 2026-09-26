using static TorchSharp.torch;
using static TorchSharp.torch.nn;

namespace Volcano.Neural
{
    public class GraphConvLayer : Module<Tensor, Tensor>
    {
        private readonly Module<Tensor, Tensor> linear;
        private readonly Tensor adjacencyMatrix;

        public GraphConvLayer(long inFeatures, long outFeatures, Tensor boardTopology)
            : base("SwappedGraphConvLayer")
        {
            // We use a 1D Convolution with kernel size 1 as our "Linear" transformation layer.
            // Why? Because nn.Linear changes the LAST dimension. Since our last dimension is
            // now Tiles, nn.Conv1d(kernelSize: 1) is the correct way to transform CHANNELS.
            linear = Conv1d(inFeatures, outFeatures, kernel_size: 1, bias: false);
            adjacencyMatrix = boardTopology.alias();
            RegisterComponents();
        }

        /// <param name="nodeFeatures">Expected shape: [BatchSize, InChannels, Tiles]</param>
        public override Tensor forward(Tensor nodeFeatures)
        {
            using (var scope = NewDisposeScope())
            {
                long numNodes = adjacencyMatrix.shape[0];

                // 1. Create Normalized Adjacency (A + I) / 4
                var identity = eye(numNodes, numNodes, adjacencyMatrix.dtype, adjacencyMatrix.device);
                var A_hat = adjacencyMatrix.add(identity);
                var A_norm = A_hat.div(4.0f);

                // 2. Aggregate features along the Tiles dimension using RIGHT multiplication
                // [Batch, Channels, Tiles] x [Tiles, Tiles] -> Keeps shape as [Batch, Channels, Tiles]
                var aggregatedFeatures = matmul(nodeFeatures, A_norm);

                // 3. Apply the channel transformation using the Conv1d layer
                var transformed = linear.forward(aggregatedFeatures);

                // 4. Apply activation function
                var activated = functional.relu(transformed);

                return activated.MoveToOuterDisposeScope();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                adjacencyMatrix.Dispose();
                linear.Dispose();
            }
            base.Dispose(disposing);
        }
    }

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
}
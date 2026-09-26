using static TorchSharp.torch;
using static TorchSharp.torch.nn;

namespace Volcano.Neural
{
    public class GraphConvLayer : Module<Tensor, Tensor>
    {
        private readonly Module<Tensor, Tensor> linear;
        private readonly Tensor adjacencyMatrix;

        public GraphConvLayer(long inFeatures, long outFeatures, Tensor boardTopology)
            : base("SequentialGraphConvLayer")
        {
            linear = Linear(inFeatures, outFeatures, hasBias: true);
            // Retain the matrix across operations
            adjacencyMatrix = boardTopology.alias();
            RegisterComponents();
        }

        public override Tensor forward(Tensor nodeFeatures)
        {
            using (var scope = NewDisposeScope())
            {
                long numNodes = adjacencyMatrix.shape[0];

                // Create identity matrix matching graph size
                var identity = eye(numNodes, numNodes, adjacencyMatrix.dtype, adjacencyMatrix.device);

                // Add self-loops (A + I)
                var A_hat = adjacencyMatrix.add(identity);

                // Normalization step (3 neighbors + 1 self-loop = regular degree of 4)
                var A_norm = A_hat.div(4.0f);

                // Aggregate neighbor attributes (Message Passing)
                var aggregatedFeatures = matmul(A_norm, nodeFeatures);

                // Trainable weight matrix mapping transformation
                var transformed = linear.forward(aggregatedFeatures);

                // Apply correct TorchSharp functional ReLU activation
                var activated = functional.relu(transformed);

                // Safely bubbles the target tensor out of the disposal scope
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
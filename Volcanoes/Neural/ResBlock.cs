using TorchSharp;
using static TorchSharp.torch;

namespace Volcano.Neural
{
    public class ResBlock : torch.nn.Module<Tensor, Tensor>
    {
        private GraphConvLayer conv1;
        private TorchSharp.Modules.BatchNorm2d bn1;
        private GraphConvLayer conv2;
        private TorchSharp.Modules.BatchNorm2d bn2;

        public ResBlock(int numHidden, string name, Tensor boardTopology) : base(name)
        {
            conv1 = skotz.nn.ConvGraph(numHidden, numHidden, boardTopology);
            bn1 = torch.nn.BatchNorm2d(numHidden);
            conv2 = skotz.nn.ConvGraph(numHidden, numHidden, boardTopology);
            bn2 = torch.nn.BatchNorm2d(numHidden);
            RegisterComponents();
        }

        public override torch.Tensor forward(torch.Tensor x)
        {
            torch.Tensor residual = x.clone();
            x = torch.nn.functional.relu(bn1.forward(conv1.forward(x)));
            x = bn2.forward(conv2.forward(x));
            x += residual;
            x = torch.nn.functional.relu(x);
            return x;
        }
    }
}
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;

namespace Volcano.Neural
{
    public class ResBlock : torch.nn.Module<Tensor, Tensor>
    {
        private CylindricalConv2d conv1;
        private BatchNorm2d bn1;
        private CylindricalConv2d conv2;
        private BatchNorm2d bn2;

        public ResBlock(int numHidden, string name) : base(name)
        {
            conv1 = skotz.nn.CylConv2d(numHidden, numHidden);
            bn1 = torch.nn.BatchNorm2d(numHidden);
            conv2 = skotz.nn.CylConv2d(numHidden, numHidden);
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
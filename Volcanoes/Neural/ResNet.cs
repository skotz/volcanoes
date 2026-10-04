using System;
using System.Text;
using TorchSharp;
using static TorchSharp.torch;

namespace Volcano.Neural
{
    public class ResNet : torch.nn.Module<Tensor, Tuple<Tensor, Tensor>>
    {
        private TorchSharp.Modules.Sequential startBlock;
        private TorchSharp.Modules.ModuleList<ResBlock> backBone;
        private StringBuilder sb = new StringBuilder();
        private TorchSharp.Modules.Sequential policyHead;
        private TorchSharp.Modules.Sequential valueHead;
        private Device device;

        public Device Device
        {
            get
            {
                return device;
            }
        }

        public ResNet(int numResBlocks, int numHidden, Device device) : base("ResNet")
        {
            this.device = device;
            startBlock = torch.nn.Sequential(skotz.nn.CylConv2d(Encoder.channels, numHidden),
                                             torch.nn.BatchNorm2d(numHidden),
                                             torch.nn.ReLU());
            backBone = torch.nn.ModuleList<ResBlock>();

            for (int i = 0; i < numResBlocks; i++)
            {
                sb.Append("ResBlock_");
                sb.Append(i.ToString());
                backBone.add_module(sb.ToString(), new ResBlock(numHidden, sb.ToString()));
            }

            policyHead = torch.nn.Sequential(skotz.nn.CylConv2d(numHidden, 256),
                                             torch.nn.BatchNorm2d(256),
                                             torch.nn.ReLU(),
                                             torch.nn.Flatten(),
                                             torch.nn.Linear(256 * 80, 80));
            valueHead = torch.nn.Sequential(skotz.nn.CylConv2d(numHidden, 36),
                                            torch.nn.BatchNorm2d(36),
                                            torch.nn.ReLU(),
                                            torch.nn.Flatten(),
                                            torch.nn.Linear(36 * 80, 1),
                                            torch.nn.Tanh());
            RegisterComponents();
            this.to(device);
        }

        public override Tuple<torch.Tensor, torch.Tensor> forward(torch.Tensor input)
        {
            torch.Tensor output = startBlock.forward(input);
            foreach (ResBlock block in backBone)
            {
                output = block.forward(output);
            }
            torch.Tensor policy = policyHead.forward(output);
            torch.Tensor value = valueHead.forward(output);
            return new Tuple<torch.Tensor, torch.Tensor>(policy, value);
        }
    }
}
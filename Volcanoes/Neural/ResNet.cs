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

        public ResNet(int numResBlocks, int numHidden, Device device, Tensor boardTopology) : base("ResNet")
        {
            this.device = device;
            startBlock = torch.nn.Sequential(skotz.nn.ConvGraph(Encoder.channels, numHidden, boardTopology),
                                             torch.nn.LayerNorm([numHidden]),
                                             torch.nn.ReLU());
            backBone = torch.nn.ModuleList<ResBlock>();

            for (int i = 0; i < numResBlocks; i++)
            {
                sb.Append("ResBlock_");
                sb.Append(i.ToString());
                backBone.add_module(sb.ToString(), new ResBlock(numHidden, sb.ToString(), boardTopology));
            }

            policyHead = torch.nn.Sequential(skotz.nn.ConvGraph(numHidden, 256, boardTopology),
                                             torch.nn.LayerNorm([256]),
                                             torch.nn.ReLU(),
                                             torch.nn.Flatten(),
                                             torch.nn.Linear(256 * 80, 80));
            valueHead = torch.nn.Sequential(skotz.nn.ConvGraph(numHidden, 36, boardTopology),
                                            torch.nn.LayerNorm([36]),
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
using TorchSharp;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;

namespace Volcano.Neural
{
    public class CylindricalConv2d : Module<Tensor, Tensor>
    {
        private readonly TorchSharp.Modules.Conv2d _conv;

        public CylindricalConv2d(long inChannels, long outChannels)
            : base("CylindricalConv2d")
        {
            // Initialize Conv2d with NO padding, because we manually handle both directions
            _conv = torch.nn.Conv2d(inChannels, outChannels, 3, 1, 0);

            RegisterComponents();
        }

        public override Tensor forward(Tensor input)
        {
            // 1. Pad LEFT and RIGHT circularly
            // Array format: [Left, Right, Top, Bottom] -> only horizontal gets values
            long[] horizontalPad = [1, 1, 0, 0];

            // 2. Pad TOP and BOTTOM with zeros (Constant mode)
            // Array format: [Left, Right, Top, Bottom] -> only vertical gets values
            long[] verticalPad = [0, 0, 1, 1];

            // Execute the pipeline sequentially
            using (var horizPadded = functional.pad(input, horizontalPad, PaddingModes.Circular))
            using (var fullyPadded = functional.pad(horizPadded, verticalPad, PaddingModes.Constant, 0.0))
            {
                return _conv.forward(fullyPadded);
            }
        }
    }
}
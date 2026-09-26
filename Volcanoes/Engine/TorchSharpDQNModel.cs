using System;
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;

namespace Volcano.Engine
{
    internal class TorchSharpDQNModel : Module<torch.Tensor, torch.Tensor>
    {
        private readonly Conv2d _conv1;
        private readonly BatchNorm2d _bn1;
        private readonly ResNetBlock[] _resnetBlocks;
        private readonly Conv2d _conv2;
        private readonly BatchNorm2d _bn2;
        private readonly Linear _output;

        private torch.Tensor _triangularMask;
        private readonly torch.Device _device;

        public TorchSharpDQNModel(torch.Device device = null) : base(nameof(TorchSharpDQNModel))
        {
            _device = device ?? torch.CPU;

            // First Conv2D: 13 input channels -> 128 output channels
            _conv1 = Conv2d(13, 128, kernelSize: 3, padding: 1);
            _conv1 = _conv1.to(_device);

            // BatchNorm after first conv
            _bn1 = BatchNorm2d(128);
            _bn1 = _bn1.to(_device);

            // 6 ResNet blocks (128 -> 128 channels)
            _resnetBlocks = new ResNetBlock[6];
            for (int i = 0; i < 6; i++)
            {
                _resnetBlocks[i] = new ResNetBlock(128, device: _device);
            }

            // Second Conv2D: 128 -> 128 channels
            _conv2 = Conv2d(128, 128, kernelSize: 3, padding: 1);
            _conv2 = _conv2.to(_device);

            // BatchNorm after second conv
            _bn2 = BatchNorm2d(128);
            _bn2 = _bn2.to(_device);

            // Flatten + Linear: (128 * 10 * 8) = 10240 -> 80
            _output = Linear(10240, 80);
            _output = _output.to(_device);

            // Register parameters
            RegisterComponents();

            // Create triangular weight mask for Conv2D
            InitializeTriangularMask();
        }

        private void RegisterComponents()
        {
            RegisterModule("conv1", _conv1);
            RegisterModule("bn1", _bn1);
            for (int i = 0; i < _resnetBlocks.Length; i++)
            {
                RegisterModule($"resnet{i}", _resnetBlocks[i]);
            }
            RegisterModule("conv2", _conv2);
            RegisterModule("bn2", _bn2);
            RegisterModule("output", _output);
        }

        private void InitializeTriangularMask()
        {
            // Create a mask for 3x3 kernel with triangular topology
            var maskData = new float[] {
                1, 1, 0,
                1, 1, 1,
                0, 1, 1
            };

            _triangularMask = tensor(maskData, dtype: float32).reshape(1, 1, 3, 3).to(_device);
        }

        public override torch.Tensor forward(torch.Tensor state)
        {
            // Input: batch of states, shape (batch_size, 1040)
            // Reshape to (batch_size, 13, 10, 8) - 80 tiles arranged as 10x8 spatial grid with 13 channels
            var batch_size = state.shape[0];
            var x = state.reshape(batch_size, 13, 10, 8);

            // First Conv2D with triangular mask
            x = ApplyMaskedConv2d(x, _conv1);
            x = _bn1.forward(x);
            x = relu(x);

            // 6 ResNet blocks
            foreach (var block in _resnetBlocks)
            {
                x = block.forward(x);
            }

            // Second Conv2D with triangular mask
            x = ApplyMaskedConv2d(x, _conv2);
            x = _bn2.forward(x);
            x = relu(x);

            // Flatten
            x = x.view(batch_size, -1L);

            // Output layer (10240 -> 80)
            x = _output.forward(x);

            return x;
        }

        private torch.Tensor ApplyMaskedConv2d(torch.Tensor input, Conv2d conv)
        {
            var originalWeight = conv.weight.clone();

            // Apply triangular mask to each output channel pair
            var maskedWeight = originalWeight.clone();
            var maskExpanded = _triangularMask.expand(originalWeight.shape[0], originalWeight.shape[1], 3, 3);

            maskedWeight = maskedWeight * maskExpanded;

            // Temporarily set weights
            conv.weight.copy_(maskedWeight);

            // Run convolution
            var output = conv.forward(input);

            // Restore original weights
            conv.weight.copy_(originalWeight);

            return output;
        }

        public void SetTrainingMode(bool training)
        {
            train(training);
        }

        public void CopyWeightsFrom(TorchSharpDQNModel source)
        {
            using (no_grad())
            {
                _conv1.weight.copy_(source._conv1.weight);
                if (_conv1.bias != null) _conv1.bias.copy_(source._conv1.bias);

                _bn1.weight.copy_(source._bn1.weight);
                _bn1.bias.copy_(source._bn1.bias);
                _bn1.running_mean.copy_(source._bn1.running_mean);
                _bn1.running_var.copy_(source._bn1.running_var);

                for (int i = 0; i < _resnetBlocks.Length; i++)
                {
                    _resnetBlocks[i].CopyWeightsFrom(source._resnetBlocks[i]);
                }

                _conv2.weight.copy_(source._conv2.weight);
                if (_conv2.bias != null) _conv2.bias.copy_(source._conv2.bias);

                _bn2.weight.copy_(source._bn2.weight);
                _bn2.bias.copy_(source._bn2.bias);
                _bn2.running_mean.copy_(source._bn2.running_mean);
                _bn2.running_var.copy_(source._bn2.running_var);

                _output.weight.copy_(source._output.weight);
                if (_output.bias != null) _output.bias.copy_(source._output.bias);
            }
        }
    }

    internal class ResNetBlock : Module<torch.Tensor, torch.Tensor>
    {
        private readonly Conv2d _conv1;
        private readonly BatchNorm2d _bn1;
        private readonly Conv2d _conv2;
        private readonly BatchNorm2d _bn2;
        private readonly torch.Device _device;
        private torch.Tensor _triangularMask;

        public ResNetBlock(int channels, torch.Device device = null) : base(nameof(ResNetBlock))
        {
            _device = device ?? torch.CPU;

            _conv1 = Conv2d(channels, channels, kernelSize: 3, padding: 1);
            _conv1 = _conv1.to(_device);
            _bn1 = BatchNorm2d(channels);
            _bn1 = _bn1.to(_device);

            _conv2 = Conv2d(channels, channels, kernelSize: 3, padding: 1);
            _conv2 = _conv2.to(_device);
            _bn2 = BatchNorm2d(channels);
            _bn2 = _bn2.to(_device);

            RegisterModule("conv1", _conv1);
            RegisterModule("bn1", _bn1);
            RegisterModule("conv2", _conv2);
            RegisterModule("bn2", _bn2);

            InitializeTriangularMask();
        }

        private void InitializeTriangularMask()
        {
            var maskData = new float[] {
                1, 1, 0,
                1, 1, 1,
                0, 1, 1
            };

            _triangularMask = tensor(maskData, dtype: float32).reshape(1, 1, 3, 3).to(_device);
        }

        public override torch.Tensor forward(torch.Tensor x)
        {
            var residual = x;

            var output = ApplyMaskedConv2d(x, _conv1);
            output = _bn1.forward(output);
            output = relu(output);

            output = ApplyMaskedConv2d(output, _conv2);
            output = _bn2.forward(output);

            output = output + residual;
            output = relu(output);

            return output;
        }

        private torch.Tensor ApplyMaskedConv2d(torch.Tensor input, Conv2d conv)
        {
            var originalWeight = conv.weight.clone();
            var maskedWeight = originalWeight.clone();
            var maskExpanded = _triangularMask.expand(originalWeight.shape[0], originalWeight.shape[1], 3, 3);

            maskedWeight = maskedWeight * maskExpanded;
            conv.weight.copy_(maskedWeight);

            var output = conv.forward(input);
            conv.weight.copy_(originalWeight);

            return output;
        }

        public void CopyWeightsFrom(ResNetBlock source)
        {
            using (no_grad())
            {
                _conv1.weight.copy_(source._conv1.weight);
                if (_conv1.bias != null) _conv1.bias.copy_(source._conv1.bias);
                _bn1.weight.copy_(source._bn1.weight);
                _bn1.bias.copy_(source._bn1.bias);
                _bn1.running_mean.copy_(source._bn1.running_mean);
                _bn1.running_var.copy_(source._bn1.running_var);

                _conv2.weight.copy_(source._conv2.weight);
                if (_conv2.bias != null) _conv2.bias.copy_(source._conv2.bias);
                _bn2.weight.copy_(source._bn2.weight);
                _bn2.bias.copy_(source._bn2.bias);
                _bn2.running_mean.copy_(source._bn2.running_mean);
                _bn2.running_var.copy_(source._bn2.running_var);
            }
        }
    }
}

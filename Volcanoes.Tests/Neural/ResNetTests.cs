using FluentAssertions;
using TorchSharp;
using Volcano.Game;
using Volcano.Neural;

namespace Volcanoes.Tests.Neural
{
    public class ResNetTests
    {
        [Fact]
        public void Forward_EveryParameterReceivesGradient()
        {
            var model = new ResNet(2, 16, torch.CPU);
            var encoder = new Encoder();
            model.train();

            var first = new Board();
            var second = new Board();
            for (int t = 0; t < 80; t++)
            {
                first.Tiles[t] = (t * 7 % 9) - 4;
                second.Tiles[t] = (t * 5 % 9) - 4;
            }

            var (policy, value) = model.forward(encoder.Encode(new[] { first, second }, torch.CPU));
            (policy.pow(2).sum() + value.sum()).backward();

            var missing = model.named_parameters()
                .Where(p => p.parameter.grad is null || p.parameter.grad.abs().sum().item<float>() == 0f)
                .Select(p => p.name + (p.parameter.grad is null ? ":null" : ":zero"))
                .ToArray();

            missing.Should().BeEmpty("every parameter should be used by the forward pass");
        }
    }
}
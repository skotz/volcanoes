using FluentAssertions;
using TorchSharp;
using Volcano.Neural;
using Xunit;

namespace Volcanoes.Tests.Neural
{
    public class GraphResNetTests
    {
        [Fact]
        public void BuildNormalizedAdjacency_IsSymmetric()
        {
            var adjacency = GraphResNet.BuildNormalizedAdjacency();

            var transposed = adjacency.transpose(0, 1);
            var diff = (adjacency - transposed).abs().max().item<float>();

            diff.Should().BeLessThan(1e-6f);
        }
    }
}

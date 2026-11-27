using FluentAssertions;
using TorchSharp;
using Volcano.Game;
using Volcano.Neural;

namespace Volcanoes.Tests.Neural
{
    public class GraphResNetTests
    {
        private const int Tiles = 80;

        [Fact]
        public void BuildTypedAdjacency_HasOneEntryPerRowForEachRelation()
        {
            var adjacency = GraphResNet.BuildTypedAdjacency();

            adjacency.shape.Should().Equal(new long[] { GraphResNet.relations, Tiles, Tiles });

            var data = adjacency.data<float>().ToArray();
            for (int k = 0; k < GraphResNet.relations; k++)
            {
                for (int t = 0; t < Tiles; t++)
                {
                    var row = data.Skip((k * Tiles + t) * Tiles).Take(Tiles).ToArray();
                    row.Sum().Should().Be(1f, $"relation {k} row {t} should have exactly one neighbor");
                    row[Constants.CounterClockwiseNeighbors[t][k]].Should().Be(1f, $"relation {k} row {t} should point at counter-clockwise neighbor {k}");
                }
            }
        }

        [Fact]
        public void BuildTypedAdjacency_SumOfRelationsMatchesUndirectedAdjacency()
        {
            var adjacency = GraphResNet.BuildTypedAdjacency();
            var summed = adjacency.sum(new long[] { 0 });

            var expected = new float[Tiles * Tiles];
            for (int t = 0; t < Tiles; t++)
            {
                foreach (var n in Constants.AdjacentIndexes[t])
                {
                    expected[t * Tiles + n] = 1f;
                }
            }

            summed.data<float>().ToArray().Should().Equal(expected);

            var diff = (summed - summed.transpose(0, 1)).abs().max().item<float>();
            diff.Should().BeLessThan(1e-6f);
        }

        [Fact]
        public void Forward_EveryParameterReceivesGradient()
        {
            var model = new GraphResNet(2, 16, GraphEncoder.features, torch.CPU);
            var encoder = new GraphEncoder();

            var board = new Board();
            for (int t = 0; t < Tiles; t++)
            {
                board.Tiles[t] = (t * 7 % 9) - 4;
            }

            var (policy, value) = model.forward(encoder.Encode(new[] { board }, torch.CPU));
            (policy.pow(2).sum() + value.sum()).backward();

            var missing = model.named_parameters()
                .Where(p => p.parameter.grad is null || p.parameter.grad.abs().sum().item<float>() == 0f)
                .Select(p => p.name + (p.parameter.grad is null ? ":null" : ":zero"))
                .ToArray();

            missing.Should().BeEmpty("every parameter should be used by the forward pass");
        }

        [Fact]
        public void Forward_ReturnsPolicyPerTileAndSingleValue()
        {
            var model = new GraphResNet(2, 16, GraphEncoder.features, torch.CPU);
            var encoder = new GraphEncoder();
            var boards = new[] { new Board(), new Board() };
            using (torch.no_grad())
            {
                var (policy, value) = model.forward(encoder.Encode(boards, torch.CPU));

                policy.shape.Should().Equal(new long[] { 2, Tiles });
                value.shape.Should().Equal(new long[] { 2, 1 });
            }
        }
    }
}
using System.Linq;
using FluentAssertions;
using TorchSharp;
using Volcano.Game;
using Volcano.Neural;
using Xunit;

namespace Volcanoes.Tests.Neural
{
    public class GraphEncoderTests
    {
        private const int Tiles = 80;

        [Fact]
        public void Encode_TileValuesMapToNineOneHotChannels()
        {
            var board = new Board();
            for (int t = 0; t < Tiles; t++)
            {
                board.Tiles[t] = (t % 9) - 4;
            }

            var encoded = new GraphEncoder().Encode(board, torch.CPU);

            encoded.shape.Should().Equal(new long[] { 1, Tiles, GraphEncoder.features });

            var data = encoded.data<float>().ToArray();
            for (int t = 0; t < Tiles; t++)
            {
                var channels = data.Skip(t * GraphEncoder.features).Take(9).ToArray();
                channels.Sum().Should().Be(1f, $"tile {t} should have exactly one value channel set");
                channels[t % 9].Should().Be(1f, $"tile {t} should set the channel for its value");
            }
        }

        [Fact]
        public void Encode_MoveTypeChannelIsConstantAcrossTiles()
        {
            var encoded = new GraphEncoder().Encode(new Board(), torch.CPU);

            var data = encoded.data<float>().ToArray();
            var flags = Enumerable.Range(0, Tiles).Select(t => data[t * GraphEncoder.features + 9]).Distinct().ToArray();

            flags.Should().HaveCount(1, "the allGrow flag is the same for every node");
            flags[0].Should().BeOneOf(0f, 1f);
        }
    }
}

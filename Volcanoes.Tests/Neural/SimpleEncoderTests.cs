using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Volcano.Neural;
using Xunit;

namespace Volcanoes.Tests.Neural
{
    public class SimpleEncoderTests
    {
        [Fact]
        public void TileMapping_ContainsEveryTileExactlyOnce()
        {
            var field = typeof(SimpleEncoder).GetField("tileMapping", BindingFlags.NonPublic | BindingFlags.Instance);
            field.Should().NotBeNull();

            var tileMapping = (string[][])field!.GetValue(new SimpleEncoder())!;
            var tiles = tileMapping.SelectMany(row => row).ToList();

            var expected = new List<string>();
            for (var i = 1; i <= 40; i++)
            {
                expected.Add($"N{i:00}");
                expected.Add($"S{i:00}");
            }

            tiles.Should().HaveCount(expected.Count, "the mapping should contain exactly one entry per tile");
            tiles.Distinct().Should().HaveCount(tiles.Count, "the mapping should not contain duplicate tiles");
            tiles.Should().BeEquivalentTo(expected, "the mapping should contain every tile and nothing else");
        }
    }
}

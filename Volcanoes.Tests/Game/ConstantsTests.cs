using System.Linq;
using FluentAssertions;
using Volcano.Game;
using Xunit;

namespace Volcanoes.Tests.Game
{
    public class ConstantsTests
    {
        private const int Tiles = 80;

        [Fact]
        public void AdjacentIndexes_HasEntryForEveryTile()
        {
            Constants.AdjacentIndexes.Should().HaveCount(Tiles);
        }

        [Fact]
        public void AdjacentIndexes_EveryTileHasThreeNeighbors()
        {
            for (int t = 0; t < Tiles; t++)
            {
                Constants.AdjacentIndexes[t].Should().HaveCount(3, $"tile {t} should touch three tiles");
            }
        }

        [Fact]
        public void AdjacentIndexes_NeighborsAreInRangeAndNotSelf()
        {
            for (int t = 0; t < Tiles; t++)
            {
                foreach (var n in Constants.AdjacentIndexes[t])
                {
                    n.Should().BeInRange(0, Tiles - 1, $"neighbor of tile {t}");
                    n.Should().NotBe(t, $"tile {t} should not neighbor itself");
                }
            }
        }

        [Fact]
        public void AdjacentIndexes_HasNoDuplicateNeighbors()
        {
            for (int t = 0; t < Tiles; t++)
            {
                Constants.AdjacentIndexes[t].Distinct().Count().Should().Be(3, $"tile {t} neighbors should be distinct");
            }
        }

        [Fact]
        public void AdjacentIndexes_IsSymmetric()
        {
            for (int t = 0; t < Tiles; t++)
            {
                foreach (var n in Constants.AdjacentIndexes[t])
                {
                    Constants.AdjacentIndexes[n].Should().Contain(t, $"tile {n} should list tile {t} as a neighbor");
                }
            }
        }
    }
}

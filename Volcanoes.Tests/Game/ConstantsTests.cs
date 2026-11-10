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

        [Fact]
        public void EquilateralIndexes_HasTwentyTiles()
        {
            Constants.EquilateralIndexes.Should().HaveCount(20);
        }

        [Fact]
        public void IsoscelesIndexes_HasSixtyTiles()
        {
            Constants.IsoscelesIndexes.Should().HaveCount(60);
        }

        [Fact]
        public void EquilateralIndexes_AreDistinctAndInRange()
        {
            Constants.EquilateralIndexes.Distinct().Should().HaveCount(20);
            Constants.EquilateralIndexes.Should().OnlyContain(t => t >= 0 && t < Tiles);
        }

        [Fact]
        public void IsoscelesIndexes_AreDistinctAndInRange()
        {
            Constants.IsoscelesIndexes.Distinct().Should().HaveCount(60);
            Constants.IsoscelesIndexes.Should().OnlyContain(t => t >= 0 && t < Tiles);
        }

        [Fact]
        public void TileCollections_TogetherContainEveryTileExactlyOnce()
        {
            var all = Constants.EquilateralIndexes.Concat(Constants.IsoscelesIndexes).OrderBy(t => t).ToArray();

            all.Should().HaveCount(Tiles);
            all.Should().BeEquivalentTo(Enumerable.Range(0, Tiles));
        }

        [Fact]
        public void TileCollections_DoNotOverlap()
        {
            Constants.EquilateralIndexes.Intersect(Constants.IsoscelesIndexes).Should().BeEmpty();
        }

        [Fact]
        public void EquilateralIndexes_SingleTileCanonicalizesToSameBoardForEveryTile()
        {
            AssertSingleTileCanonicalizesUniformly(Constants.EquilateralIndexes, "equilateral");
        }

        [Fact]
        public void IsoscelesIndexes_SingleTileCanonicalizesToSameBoardForEveryTile()
        {
            AssertSingleTileCanonicalizesUniformly(Constants.IsoscelesIndexes, "isosceles");
        }

        private static void AssertSingleTileCanonicalizesUniformly(int[] collection, string name)
        {
            var reference = CanonicalizeSingleTile(collection[0]);

            foreach (var tile in collection)
            {
                CanonicalizeSingleTile(tile).Should().Equal(reference, $"{name} tile {tile} should canonicalize like the others");
            }
        }

        private static int[] CanonicalizeSingleTile(int tile)
        {
            var board = new int[Tiles];
            board[tile] = 1;

            var canonical = new Canonical();
            canonical.SetIndex(board);
            return canonical.Canonicalize(board);
        }
    }
}

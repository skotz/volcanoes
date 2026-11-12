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
        public void CounterClockwiseNeighbors_EveryTileHasThreeDistinctNeighbors()
        {
            Constants.CounterClockwiseNeighbors.Should().HaveCount(Tiles);

            for (int t = 0; t < Tiles; t++)
            {
                var neighbors = Constants.CounterClockwiseNeighbors[t];

                neighbors.Should().NotBeNull($"tile {t} should have neighbors");
                neighbors.Should().HaveCount(3, $"tile {t} should touch three tiles");
                neighbors.Distinct().Should().HaveCount(3, $"tile {t} neighbors should be distinct");
                neighbors.Should().OnlyContain(n => n >= 0 && n < Tiles && n != t, $"tile {t} neighbors should be valid tiles other than itself");
            }
        }

        [Fact]
        public void CounterClockwiseNeighbors_MatchAdjacentIndexesIgnoringOrder()
        {
            for (int t = 0; t < Tiles; t++)
            {
                Constants.CounterClockwiseNeighbors[t].OrderBy(x => x).Should().Equal(
                    Constants.AdjacentIndexes[t].OrderBy(x => x),
                    $"tile {t} should have the same neighbors in both lists");
            }
        }

        [Fact]
        public void CounterClockwiseNeighbors_IsoscelesIndexZeroIsEquilateral()
        {
            foreach (var t in Constants.IsoscelesIndexes)
            {
                Constants.EquilateralIndexes.Should().Contain(Constants.CounterClockwiseNeighbors[t][0], $"isosceles tile {t} index 0 should be the neighbor opposite the 5-fold vertex");
            }
        }

        [Fact]
        public void CounterClockwiseNeighbors_IsoscelesLegsAreConsistentAroundVertex()
        {
            foreach (var t in Constants.IsoscelesIndexes)
            {
                var u = Constants.CounterClockwiseNeighbors[t][1];
                if (Constants.IsoscelesIndexes.Contains(u))
                {
                    Constants.CounterClockwiseNeighbors[u][2].Should().Be(t, $"tile {t} should be the counter-clockwise predecessor of its index 1 neighbor {u}");
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

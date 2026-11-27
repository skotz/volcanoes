using System;
using System.Linq;
using FluentAssertions;
using Volcano.Game;
using Xunit;

namespace Volcanoes.Tests.Game
{
    public class CanonicalTests
    {
        private const int Tiles = 80;

        private static int[] RandomTiles(int seed)
        {
            var random = new Random(seed);
            return Enumerable.Range(0, Tiles).Select(_ => random.Next(-4, 5)).ToArray();
        }

        private static Canonical IndexedCanonical(int[] board)
        {
            var canonical = new Canonical();
            canonical.SetIndex(board);
            return canonical;
        }

        [Fact]
        public void IndexToCanonical_FirstPermutationIsIdentity()
        {
            for (int i = 0; i < Tiles; i++)
            {
                Canonical._indexToCanonical[0, i].Should().Be(i);
            }
        }

        [Fact]
        public void IndexToCanonical_EveryPermutationContainsEveryTileOnce()
        {
            var permutations = Canonical._indexToCanonical.GetLength(0);
            permutations.Should().BeGreaterThan(0);

            for (int p = 0; p < permutations; p++)
            {
                var row = Enumerable.Range(0, Tiles).Select(i => Canonical._indexToCanonical[p, i]).OrderBy(x => x);
                row.Should().Equal(Enumerable.Range(0, Tiles), $"permutation {p} should map each tile exactly once");
            }
        }

        [Fact]
        public void CanonicalToIndex_IsInverseOfIndexToCanonical()
        {
            var permutations = Canonical._indexToCanonical.GetLength(0);
            Canonical._canonicalToIndex.GetLength(0).Should().Be(permutations);

            for (int p = 0; p < permutations; p++)
            {
                for (int i = 0; i < Tiles; i++)
                {
                    Canonical._canonicalToIndex[p, Canonical._indexToCanonical[p, i]].Should().Be(i, $"permutation {p}, tile {i}");
                }
            }
        }

        [Fact]
        public void BoardToCanonical_ThrowsBeforeIndexIsSet()
        {
            var canonical = new Canonical();

            Action action = () => canonical.BoardToCanonical(0);

            action.Should().Throw<Exception>();
        }

        [Fact]
        public void CanonicalToBoard_ThrowsBeforeIndexIsSet()
        {
            var canonical = new Canonical();

            Action action = () => canonical.CanonicalToBoard(0);

            action.Should().Throw<Exception>();
        }

        [Fact]
        public void BoardToCanonical_OutOfRangeIndexIsReturnedUnchanged()
        {
            var canonical = IndexedCanonical(RandomTiles(1));

            canonical.BoardToCanonical(-1).Should().Be(-1);
            canonical.BoardToCanonical(Tiles).Should().Be(Tiles);
        }

        [Fact]
        public void CanonicalToBoard_OutOfRangeIndexIsReturnedUnchanged()
        {
            var canonical = IndexedCanonical(RandomTiles(1));

            canonical.CanonicalToBoard(-1).Should().Be(-1);
            canonical.CanonicalToBoard(Tiles).Should().Be(Tiles);
        }

        [Fact]
        public void CanonicalToBoard_IsInverseOfBoardToCanonical()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                var canonical = IndexedCanonical(RandomTiles(seed));

                for (int i = 0; i < Tiles; i++)
                {
                    canonical.CanonicalToBoard(canonical.BoardToCanonical(i)).Should().Be(i);
                }
            }
        }

        [Fact]
        public void SetIndex_OnAllZeroBoard_UsesIdentityPermutation()
        {
            var canonical = IndexedCanonical(new int[Tiles]);

            for (int i = 0; i < Tiles; i++)
            {
                canonical.BoardToCanonical(i).Should().Be(i);
            }
        }

        [Fact]
        public void Canonicalize_IntBoard_PreservesValuesAsAMultiset()
        {
            var board = RandomTiles(7);
            var canonical = IndexedCanonical(board);

            var result = canonical.Canonicalize(board);

            result.Should().HaveCount(Tiles);
            result.OrderBy(x => x).Should().Equal(board.OrderBy(x => x));
        }

        [Fact]
        public void Decanonicalize_ReversesCanonicalize_IntBoard()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                var board = RandomTiles(seed);
                var canonical = IndexedCanonical(board);

                canonical.Decanonicalize(canonical.Canonicalize(board)).Should().Equal(board);
            }
        }

        [Fact]
        public void Canonicalize_BoolBoard_MovesEachTileToItsCanonicalIndex()
        {
            var random = new Random(3);
            var board = Enumerable.Range(0, Tiles).Select(_ => random.Next(2) == 0).ToArray();
            var canonical = IndexedCanonical(RandomTiles(3));

            var result = canonical.Canonicalize(board);

            for (int i = 0; i < Tiles; i++)
            {
                result[canonical.BoardToCanonical(i)].Should().Be(board[i]);
            }
        }

        [Fact]
        public void Canonicalize_IsIdempotent()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                var board = RandomTiles(seed);
                var canonical = IndexedCanonical(board);
                var once = canonical.Canonicalize(board);

                var again = IndexedCanonical(once);

                again.Canonicalize(once).Should().Equal(once, $"seed {seed} should already be canonical");
            }
        }

        [Fact]
        public void Canonicalize_IsInvariantUnderEverySymmetry()
        {
            var board = RandomTiles(11);
            var expected = IndexedCanonical(board).Canonicalize(board);
            var permutations = Canonical._indexToCanonical.GetLength(0);

            for (int p = 0; p < permutations; p++)
            {
                var transformed = new int[Tiles];
                for (int i = 0; i < Tiles; i++)
                {
                    transformed[Canonical._indexToCanonical[p, i]] = board[i];
                }

                var canonical = IndexedCanonical(transformed);

                canonical.Canonicalize(transformed).Should().Equal(expected, $"symmetry {p} should canonicalize to the same board");
            }
        }

        [Fact]
        public void Canonicalize_BoardObject_CopiesTilesAndDormantCanonically()
        {
            var board = new Board();
            var tiles = RandomTiles(5);
            Array.Copy(tiles, board.Tiles, Tiles);
            board.Dormant[0] = true;
            var canonical = IndexedCanonical(board.Tiles);

            var result = canonical.Canonicalize(board);

            result.Tiles.Should().Equal(canonical.Canonicalize(board.Tiles));
            result.Dormant.Should().Equal(canonical.Canonicalize(board.Dormant));
        }
    }
}

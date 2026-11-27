using System.Collections.Generic;
using Volcano.Game;

namespace Volcano.Search
{
    internal class CanonicalMoveFilter : IMoveFilter
    {
        public CanonicalMoveFilter()
        {
        }

        public List<int> GetMoves(Board state)
        {
            var moves = state.GetMoves();
            var unique = new List<int>(moves.Count);
            var seen = new HashSet<long>();
            var canonical = new Canonical();

            foreach (var move in moves)
            {
                var next = new Board(state);
                next.MakeMove(move);

                canonical.SetIndex(next);
                var hash = canonical.Canonicalize(next).GetFullHash();

                if (seen.Add(hash))
                {
                    unique.Add(move);
                }
            }

            return unique;
        }
    }
}
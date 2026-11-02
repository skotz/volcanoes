using System.Collections.Generic;
using System.IO;
using System.Linq;
using Volcano.Game;

namespace Volcano.Search
{
    public static class CanonicalCounter
    {
        public static void Run()
        {
            var queue = new Queue<(Board board, int depth)>();
            queue.Enqueue((new Board(), 0));

            var positions = new List<int>();
            var seen = new HashSet<long>();

            while (queue.Peek().depth <= 4)
            {
                var state = queue.Dequeue();

                if (positions.Count <= state.depth)
                {
                    positions.Add(0);
                }
                positions[state.depth]++;

                var moves = state.board.GetMoves();

                foreach (var move in moves)
                {
                    var copy = new Board(state.board);
                    copy.MakeMove(move);

                    var canonical = new Canonical();
                    canonical.SetIndex(copy);
                    copy = canonical.Canonicalize(copy);

                    var hash = copy.GetFullHash();
                    if (seen.Contains(hash))
                    {
                        continue;
                    }
                    seen.Add(hash);

                    queue.Enqueue((copy, state.depth + 1));
                }
            }

            File.WriteAllLines("canonical.txt", positions.Select(x => x.ToString()));
        }
    }
}
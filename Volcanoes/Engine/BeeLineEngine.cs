using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;
using Volcano.Search;

namespace Volcano.Engine
{
    class BeeLineEngine : IEngine
    {
        private static Random random = new Random();

        private int bestPathStart = -1;

        private PathFinder pathFinder = new NonEnemyPathFinder();

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            List<int> moves = state.GetMoves();

            // If we've played at lest once, find a suggested antipode path and try that
            if (bestPathStart >= 0)
            {
                if ((state.Tiles[bestPathStart] > 0 && state.Player == Player.One) || (state.Tiles[bestPathStart] < 0 && state.Player == Player.Two))
                {
                    List<int> suggestedPath = GetSuggestedAntipodePath(state, bestPathStart);
                    suggestedPath = Shuffle(suggestedPath);

                    foreach (int tile in suggestedPath)
                    {
                        if (((state.Tiles[tile] > 0 && state.Player == Player.One) || (state.Tiles[tile] < 0 && state.Player == Player.Two)) || state.Tiles[tile] == 0)
                        {
                            if (moves.Any(x => x == tile && state.Tiles[x] == 0))
                            {
                                // Return the next move in the path to the antipode
                                return new SearchResult(tile);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            // If all else fails, pick a random move
            int best = moves[random.Next(moves.Count)];
            bestPathStart = best;
            return new SearchResult(best);
        }

        private List<int> GetSuggestedAntipodePath(Board state, int index)
        {
            return pathFinder.FindPath(state, index, Constants.Antipodes[index]).Path;
        }

        private List<int> Shuffle(List<int> list)
        {
            int n = list.Count;

            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                int value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
    }
}

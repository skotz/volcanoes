using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Engine
{
    class SkipTileEngine : IEngine
    {
        private static Random random = new Random();

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            SearchResult result = new SearchResult();

            for (int i = 0; i < 80; i++)
            {
                // Start from a tile we don't own
                if (state.Tiles[i] == 0)
                {
                    List<int> neighbors = new List<int>();
                    foreach (int adjacent in Constants.AdjacentIndexes[i])
                    {
                        foreach (int twoStepsAway in Constants.AdjacentIndexes[adjacent])
                        {
                            // If the tile is exactly two steps away from a tile we own
                            if (twoStepsAway != i && !Constants.AdjacentIndexes[i].Contains(twoStepsAway) && ((state.Tiles[twoStepsAway] > 0 && state.Player == Player.One) || (state.Tiles[twoStepsAway] < 0 && state.Player == Player.Two)))
                            {
                                if (!neighbors.Contains(twoStepsAway))
                                {
                                    neighbors.Add(twoStepsAway);
                                }
                            }
                        }
                    }

                    result.BestMove = i;

                    // This tile is two steps away from one and only one of our other tiles, then it's an amazing move
                    if (neighbors.Count == 1)
                    {
                        return result;
                    }
                }
            }

            if (result.BestMove >= 0)
            {
                return result;
            }
            else
            {
                List<int> moves = state.GetMoves();
                result.BestMove = moves[random.Next(moves.Count)];

                return result;
            }
        }
    }
}

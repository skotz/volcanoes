using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;
using Volcano.Search;

namespace Volcano.Engine
{
    class KittyCornerEngine : IEngine
    {
        private static Random random = new Random();

        private int bestPathStart = -1;

        private PathFinder pathFinder = new KittyPathFinder(false);
        private PathFinder playerOnlyPathFinder = new KittyPathFinder(true);

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            List<int> moves = state.GetMoves();

            // See if we have any pair of points that are antipodes connected by kitty corners
            List<int> antipodePath = new List<int>();
            for (int i = 0; i < 80; i++)
            {
                if (state.Tiles[i].Owner == state.Player)
                {
                    List<int> path = GetAntipodePath(state, i);
                    if (path.Count > 0)
                    {
                        antipodePath = path;
                        break;
                    }
                }
            }

            // If we have an antipode path, fill in tiles around the path
            if (antipodePath.Count > 0)
            {
                // Find a move in a tile adjacent to a tile in our antipode path
                List<int> adjacentAntipodePath = new List<int>();
                foreach (int i in antipodePath)
                {
                    adjacentAntipodePath.AddRange(state.Tiles[i].AdjacentIndexes);
                }

                List<int> antipodeSupport = moves.Where(x => adjacentAntipodePath.Contains(x) && state.Tiles[x].Owner == Player.Empty).ToList();

                if (antipodeSupport.Count > 0)
                {
                    // There are no more antipode support tiles, so just buff the main path
                    antipodeSupport = moves.Where(x => adjacentAntipodePath.Contains(x)).ToList();
                }

                if (antipodeSupport.Count > 0)
                {
                    // Pick a random antipode path support tile
                    return new SearchResult(antipodeSupport[random.Next(antipodeSupport.Count)]);
                }
            }
            else
            {
                // If we've played at lest once, find a suggested antipode path and try that
                if (bestPathStart >= 0)
                {
                    if (state.Tiles[bestPathStart].Owner == state.Player)
                    {
                        List<int> suggestedPath = GetSuggestedAntipodePath(state, bestPathStart);
                        foreach (int tile in suggestedPath)
                        {
                            if (state.Tiles[tile].Owner == state.Player || state.Tiles[tile].Owner == Player.Empty)
                            {
                                if (moves.Any(x => x == tile && state.Tiles[x].Owner == Player.Empty))
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

                // If we don't have an antipode path, continue connecting A tiles kitty corner until we do
                List<int> alphaMoves = moves.Where(x => Constants.TileNames[x].EndsWith("A")).ToList();

                if (alphaMoves.Count > 0)
                {
                    List<int> primaryKittyCornerMoves = new List<int>();
                    List<int> kittyCornerMoves = new List<int>();
                    foreach (int move in alphaMoves)
                    {
                        if (state.Tiles[move].Owner == Player.Empty)
                        {
                            int adjacentCount = 0;
                            foreach (int i in state.Tiles[move].KittyCornerIndexes)
                            {
                                if (state.Tiles[i].Owner == state.Player)
                                {
                                    adjacentCount++;

                                    if (!kittyCornerMoves.Contains(move))
                                    {
                                        kittyCornerMoves.Add(move);
                                    }
                                }
                            }

                            if (adjacentCount == 1)
                            {
                                // Only one adjacent kitty corner, so this is good
                                primaryKittyCornerMoves.Add(move);
                            }
                        }
                    }

                    if (primaryKittyCornerMoves.Count > 0)
                    {
                        // Pick a random primary kitty corner move
                        int best = primaryKittyCornerMoves[random.Next(primaryKittyCornerMoves.Count)];
                        bestPathStart = best;
                        return new SearchResult(best);
                    }
                    else if (kittyCornerMoves.Count > 0)
                    {
                        // Pick a random kitty corner move
                        int best = kittyCornerMoves[random.Next(kittyCornerMoves.Count)];
                        bestPathStart = best;
                        return new SearchResult(best);
                    }
                    else
                    {
                        // Pick a random A tile
                        int best = alphaMoves[random.Next(alphaMoves.Count)];
                        bestPathStart = best;
                        return new SearchResult(best);
                    }
                }
            }

            // If all else fails, pick a random move
            return new SearchResult(moves[random.Next(moves.Count)]);
        }

        private List<int> GetAntipodePath(Board state, int index)
        {
            return playerOnlyPathFinder.FindPath(state, index, state.Tiles[index].Antipode).Path;
        }

        private List<int> GetSuggestedAntipodePath(Board state, int index)
        {
            return pathFinder.FindPath(state, index, state.Tiles[index].Antipode).Path;
        }
    }
}

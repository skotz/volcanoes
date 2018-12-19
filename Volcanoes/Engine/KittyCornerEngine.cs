using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Engine
{
    class KittyCornerEngine : IEngine
    {
        private static Random random = new Random();

        private int bestPathStart = -1;

        public SearchResult GetBestMove(Board state)
        {
            List<Move> moves = state.GetMoves();

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

                List<Move> antipodeSupport = moves.Where(x => adjacentAntipodePath.Contains(x.TileIndex) && state.Tiles[x.TileIndex].Owner == Player.Empty).ToList();

                if (antipodeSupport.Count > 0)
                {
                    // There are no more antipode support tiles, so just buff the main path
                    antipodeSupport = moves.Where(x => adjacentAntipodePath.Contains(x.TileIndex)).ToList();
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
                                Move move = moves.Where(x => x.TileIndex == tile && state.Tiles[x.TileIndex].Owner == Player.Empty).FirstOrDefault();
                                if (move != null)
                                {
                                    // Return the next move in the path to the antipode
                                    return new SearchResult(move);
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
                List<Move> alphaMoves = moves.Where(x => Constants.TileNames[x.TileIndex].EndsWith("A")).ToList();

                if (alphaMoves.Count > 0)
                {
                    List<Move> primaryKittyCornerMoves = new List<Move>();
                    List<Move> kittyCornerMoves = new List<Move>();
                    foreach (Move move in alphaMoves)
                    {
                        if (state.Tiles[move.TileIndex].Owner == Player.Empty)
                        {
                            int adjacentCount = 0;
                            foreach (int i in state.Tiles[move.TileIndex].KittyCornerIndexes)
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
                        Move best = primaryKittyCornerMoves[random.Next(primaryKittyCornerMoves.Count)];
                        bestPathStart = best.TileIndex;
                        return new SearchResult(best);
                    }
                    else if (kittyCornerMoves.Count > 0)
                    {
                        // Pick a random kitty corner move
                        Move best = kittyCornerMoves[random.Next(kittyCornerMoves.Count)];
                        bestPathStart = best.TileIndex;
                        return new SearchResult(best);
                    }
                    else
                    {
                        // Pick a random A tile
                        Move best = alphaMoves[random.Next(alphaMoves.Count)];
                        bestPathStart = best.TileIndex;
                        return new SearchResult(best);
                    }
                }
            }

            // If all else fails, pick a random move
            return new SearchResult(moves[random.Next(moves.Count)]);
        }

        private List<int> GetAntipodePath(Board state, int index)
        {
            return FindPath(state, index, state.Tiles[index].Antipode, true);
        }

        private List<int> GetSuggestedAntipodePath(Board state, int index)
        {
            return FindPath(state, index, state.Tiles[index].Antipode, false);
        }
        
        private List<int> FindPath(Board state, int startingIndex, int endingIndex, bool playerOnly)
        {
            // The set of nodes already evaluated
            List<int> closedSet = new List<int>();

            // The set of currently discovered nodes that are not evaluated yet.
            // Initially, only the start node is known.
            List<int> openSet = new List<int>();
            openSet.Add(startingIndex);

            // For each node, which node it can most efficiently be reached from.
            // If a node can be reached from many nodes, cameFrom will eventually contain the
            // most efficient previous step.
            int[] cameFrom = new int[80];
            for (int i = 0; i < 80; i++)
            {
                cameFrom[i] = -1;
            }

            // For each node, the cost of getting from the start node to that node.
            int[] gScore = new int[80];
            for (int i = 0; i < 80; i++)
            {
                gScore[i] = int.MaxValue;
            }

            // The cost of going from start to start is zero.
            gScore[startingIndex] = 0;

            // For each node, the total cost of getting from the start node to the goal
            // by passing by that node. That value is partly known, partly heuristic.
            int[] fScore = new int[80];
            for (int i = 0; i < 80; i++)
            {
                fScore[i] = int.MaxValue;
            }

            // For the first node, that value is completely heuristic.
            fScore[startingIndex] = Math.Abs(endingIndex - startingIndex);

            while (openSet.Count > 0)
            {
                // Get the next item in the open set with the lowest fScore
                int current = openSet[0];
                int best = fScore[current];
                foreach (int i in openSet)
                {
                    if (fScore[i] < best)
                    {
                        current = i;
                        best = fScore[i];
                    }
                }

                // If we found a path from the start to the end, reconstruct the path and return it
                if (current == endingIndex)
                {
                    List<int> path = new List<int>();
                    path.Add(current);

                    while (cameFrom[current] != -1)
                    {
                        current = cameFrom[current];
                        path.Add(current);
                    }

                    path.Reverse();

                    return path;
                }

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (int neighbor in state.Tiles[current].KittyCornerIndexes)
                {
                    // Ignore the neighbor which is already evaluated.
                    if (closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    // Ignore tiles that aren't the player's
                    if (playerOnly && state.Tiles[neighbor].Owner != state.Tiles[startingIndex].Owner)
                    {
                        continue;
                    }

                    // The distance from start to a neighbor
                    int tentative_gScore = gScore[current] + 1;

                    // Discover a new node
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                    else if (tentative_gScore >= gScore[neighbor])
                    {
                        continue;
                    }

                    // This path is the best until now. Record it!
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentative_gScore;
                    fScore[neighbor] = gScore[neighbor] + Math.Abs(neighbor - startingIndex);
                }
            }

            return new List<int>();
        }
    }
}

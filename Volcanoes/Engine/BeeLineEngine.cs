using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Engine
{
    class BeeLineEngine : IEngine
    {
        private static Random random = new Random();

        private int bestPathStart = -1;

        public SearchResult GetBestMove(Board state)
        {
            List<Move> moves = state.GetMoves();

            // If we've played at lest once, find a suggested antipode path and try that
            if (bestPathStart >= 0)
            {
                if (state.Tiles[bestPathStart].Owner == state.Player)
                {
                    List<int> suggestedPath = GetSuggestedAntipodePath(state, bestPathStart);
                    suggestedPath = Shuffle(suggestedPath);

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

            // If all else fails, pick a random move
            Move best = moves[random.Next(moves.Count)];
            bestPathStart = best.TileIndex;
            return new SearchResult(best);
        }

        private List<int> GetSuggestedAntipodePath(Board state, int index)
        {
            return FindPath(state, index, state.Tiles[index].Antipode);
        }
        
        private List<int> FindPath(Board state, int startingIndex, int endingIndex)
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

                foreach (int neighbor in state.Tiles[current].AdjacentIndexes)
                {
                    // Ignore the neighbor which is already evaluated.
                    if (closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    // Ignore tiles that aren't the player's or empty
                    if (state.Tiles[neighbor].Owner != state.Tiles[startingIndex].Owner && state.Tiles[neighbor].Owner != Player.Empty)
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

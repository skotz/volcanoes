using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Search
{
    class PathFinder
    {
        public PathFinder()
        {
        }

        protected virtual int[] GetNeighborTiles(Board state, int tileIndex)
        {
            return state.Tiles[tileIndex].AdjacentIndexes;
        }

        protected virtual bool IsTraversableTile(Board state, Player player, int tileIndex)
        {
            // Ignore tiles that aren't the player's
            if (state.Tiles[tileIndex].Owner != player)
            {
                return false;
            }

            // Ignore magma chambers
            if (state.Tiles[tileIndex].Value <= VolcanoGame.Settings.MaxMagmaChamberLevel)
            {
                return false;
            }

            return true;
        }

        protected virtual int GetDistance(Board state, int first, int second)
        {
            return 1;
        }

        public PathResult FindPath(Board state, int startingIndex, int endingIndex)
        {
            // The set of nodes already evaluated
            bool[] closedSet = new bool[80];

            // The set of currently discovered nodes that are not evaluated yet.
            // Initially, only the start node is known.
            List<int> openSet = new List<int>();
            openSet.Add(startingIndex);

            // For each node, which node it can most efficiently be reached from.
            // If a node can be reached from many nodes, cameFrom will eventually contain the
            // most efficient previous step.
            int[] cameFrom = new int[80];

            // For each node, the cost of getting from the start node to that node.
            int[] gScore = new int[80];

            for (int i = 0; i < 80; i++)
            {
                cameFrom[i] = -1;
                gScore[i] = int.MaxValue;
            }

            // The cost of going from start to start is zero.
            gScore[startingIndex] = 0;

            //// For each node, the total cost of getting from the start node to the goal
            //// by passing by that node. That value is partly known, partly heuristic.
            //int[] fScore = new int[80];
            //for (int i = 0; i < 80; i++)
            //{
            //    fScore[i] = int.MaxValue;
            //}

            //// For the first node, that value is completely heuristic. (All antipode paths are known to be 12 tiles long.)
            //fScore[startingIndex] = 12;

            while (openSet.Count > 0)
            {
                // Get the next item in the open set with the lowest fScore
                int current = openSet[0];
                //int best = fScore[current];
                //foreach (int i in openSet)
                //{
                //    if (fScore[i] < best)
                //    {
                //        current = i;
                //        best = fScore[i];
                //    }
                //}

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

                    return new PathResult(path, gScore[endingIndex]);
                }

                openSet.Remove(current);
                closedSet[current] = true;

                foreach (int neighbor in GetNeighborTiles(state, current))
                {
                    // Ignore the neighbor which is already evaluated.
                    if (closedSet[neighbor])
                    {
                        continue;
                    }

                    if (!IsTraversableTile(state, state.Tiles[startingIndex].Owner, neighbor))
                    {
                        continue;
                    }

                    // The distance from start to a neighbor
                    int tentative_gScore = gScore[current] + GetDistance(state, current, neighbor);

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
                    //fScore[neighbor] = gScore[neighbor] /* + Math.Abs(neighbor - startingIndex) */;
                }
            }

            return new PathResult();
        }
    }
}

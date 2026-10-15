using System.Collections.Generic;
using Volcano.Game;

namespace Volcano.Search
{
    internal class TotalPathFinder
    {
        public TotalPathFinderResult GetAllPaths(Board state)
        {
            var paths = new TotalPathFinderResult();

            var done = new bool[80];

            for (int i = 0; i < 80; i++)
            {
                if (state.Tiles[i] != 0 && !done[i])
                {
                    done[i] = true;

                    if (Constants.Antipodes[i] != 0)
                    {
                        if (state.Tiles[i] > 0 && state.Tiles[Constants.Antipodes[i]] > 0)
                        {
                            done[Constants.Antipodes[i]] = true;
                        }
                        else if (state.Tiles[i] < 0 && state.Tiles[Constants.Antipodes[i]] < 0)
                        {
                            done[Constants.Antipodes[i]] = true;
                        }
                    }

                    var path = FindPath(state, i, Constants.Antipodes[i], true);
                    if (path.Path.Count > 0)
                    {
                        var sourcePlayer = state.Tiles[i] == 0 ? Player.Empty : (state.Tiles[i] > 0 ? Player.One : Player.Two);
                        if (sourcePlayer == state.Player)
                        {
                            paths.MyPaths.Add(path);
                        }
                        else
                        {
                            paths.OpponentPaths.Add(path);
                        }
                    }
                }
            }

            return paths;
        }

        private PathResult FindPath(Board state, int startingIndex, int endingIndex, bool returnPath)
        {
            // The set of nodes already evaluated
            bool[] closedSet = new bool[80];

            Player player = state.Tiles[startingIndex] == 0 ? Player.Empty : (state.Tiles[startingIndex] > 0 ? Player.One : Player.Two);

            // The set of currently discovered nodes that are not evaluated yet.
            // Initially, only the start node is known.
            Queue<int> openSet = new Queue<int>();
            openSet.Enqueue(startingIndex);

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
                int current = openSet.Dequeue();
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
                    if (returnPath)
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
                    else
                    {
                        // Just return that we found a path, don't bother piecing it together
                        return new PathResult(true);
                    }
                }

                //openSet.Remove(current);
                closedSet[current] = true;

                foreach (int neighbor in GetNeighborTiles(state, current))
                {
                    // Ignore the neighbor which is already evaluated.
                    if (closedSet[neighbor])
                    {
                        continue;
                    }

                    if (!IsTraversableTile(state, player, neighbor))
                    {
                        continue;
                    }

                    // The distance from start to a neighbor
                    int tentative_gScore = gScore[current] + GetDistance(state, player, current, neighbor);

                    // Discover a new node
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Enqueue(neighbor);
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

        protected virtual int GetDistance(Board state, Player player, int first, int second)
        {
            var tile = state.Tiles[second] == 0 ? Player.Empty : (state.Tiles[second] > 0 ? Player.One : Player.Two);

            if (tile == player)
            {
                return 0;
            }
            else if (tile == Player.Empty)
            {
                return 1;
            }
            else if (tile != player)
            {
                return 20;
            }
            else
            {
                return 100;
            }
        }

        protected int[] GetNeighborTiles(Board state, int tileIndex)
        {
            return Constants.AdjacentIndexes[tileIndex];
        }

        protected bool IsTraversableTile(Board state, Player player, int tileIndex)
        {
            //// Ignore tiles that aren't the player's
            //if ((state.Tiles[tileIndex] > 0 && player != Player.One) || (state.Tiles[tileIndex] < 0 && player != Player.Two))
            //{
            //    return false;
            //}

            //// Ignore magma chambers
            //if (Math.Abs(state.Tiles[tileIndex]) <= VolcanoGame.Settings.MaxMagmaChamberLevel)
            //{
            //    return false;
            //}

            return true;
        }
    }

    internal class TotalPathFinderResult
    {
        public List<PathResult> MyPaths = new List<PathResult>();
        public List<PathResult> OpponentPaths = new List<PathResult>();
    }
}
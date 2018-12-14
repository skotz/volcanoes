using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Engine
{
    class LongestPathEngine : IEngine
    {
        private Stopwatch timer;
        private long evaluations;
        
        public SearchResult GetBestMove(Board state)
        {
            timer = Stopwatch.StartNew(); 
            evaluations = 0;

            SearchResult result = new SearchResult();
            result.Score = int.MinValue;

            Player player = state.Player;

            // Get a list of all possible moves, randomly shuffled
            List<Move> moves = Shuffle(state.GetMoves());

            foreach (Move move in moves)
            {
                Board copy = new Board(state);
                copy.MakeMove(move);

                move.Evaluation = EvaluateLongestPath(copy, player, move.TileIndex);

                if (move.Evaluation > result.Score)
                {
                    result.BestMove = move;
                    result.Score = move.Evaluation;
                }
            }

            result.Evaluations = evaluations;
            result.Milliseconds = timer.ElapsedMilliseconds;

            return result;
        }

        private int EvaluateLongestPath(Board state, Player player, int lastIndex)
        {
            evaluations++;

            // Get a list of tiles owned by the player
            List<int> tiles = new List<int>();
            for (int i = 0; i < 80; i++)
            {
                if (state.Tiles[i].Owner == player)
                {
                    tiles.Add(i);
                }
            }

            // Find the longest possible path
            int longest = 0;
            //for (int i = 0; i < tiles.Count; i++)
            //{
            //    for (int j = i + 1; j < tiles.Count; j++)
            //    {
            //        int length = FindPath(state, i, j).Count;
            //        longest = Math.Max(length, longest);
            //    }
            //}
            for (int i = 0; i < tiles.Count; i++)
            {
                int length = FindPath(state, lastIndex, i).Count;
                longest = Math.Max(length, longest);
            }

            return longest;
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

                    // Ignore tiles that aren't the player's
                    if (state.Tiles[neighbor].Owner != state.Tiles[startingIndex].Owner)
                    {
                        continue;
                    }

                    //// Ignore magma chambers
                    //if (state.Tiles[neighbor].Value <= Constants.MaxMagmaChamberLevel)
                    //{
                    //    continue;
                    //}

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

        private static Random random = new Random();

        private List<Move> Shuffle(List<Move> list)
        {
            int n = list.Count;

            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                Move value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
    }
}

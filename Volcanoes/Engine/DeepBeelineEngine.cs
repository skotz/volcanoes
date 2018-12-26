using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;
using Volcano.Search;

namespace Volcano.Engine
{
    class DeepBeelineEngine : IEngine
    {
        private long evaluations;
        private int searchDepth;

        private int hashHits;
        private int hashMisses;

        private int maxTilesToPick = 3;

        private Random random = new Random();
        private PathFinder pathFinder = new WeightedNonEnemyPathFinder();

        private bool enableHash = false;
        private PositionHash hash = new PositionHash();

        private EngineCancellationToken cancellationToken;

        public DeepBeelineEngine()
            : this(6)
        {
        }

        public DeepBeelineEngine(int depth)
        {
            searchDepth = depth;
        }

        public SearchResult GetBestMove(Board state, EngineCancellationToken token)
        {
            Stopwatch timer = Stopwatch.StartNew();
            evaluations = 0;
            hashHits = 0;
            hashMisses = 0;

            cancellationToken = token;

            SearchResult result = AlphaBetaSearch(state, searchDepth, int.MinValue, int.MaxValue);

            result.Evaluations = evaluations;
            result.Milliseconds = timer.ElapsedMilliseconds;
            result.HashPercentage = hashHits + hashMisses > 0 ? hashHits / ((decimal)hashHits + hashMisses) : 0m;

            return result;
        }

        private int EvaluatePosition(Board position)
        {
            evaluations++;

            // Check the hash table to see if we already calculated this path for this board
            if (enableHash)
            {
                int? saved = hash.Get(position);
                if (saved != null)
                {
                    hashHits++;
                    return (int)saved;
                }
                hashMisses++;
            }

            // For each tile, figure out how long it'll take to get to it's antipode
            PathResult[] playerOnePaths = new PathResult[80];
            PathResult[] playerTwoPaths = new PathResult[80];
            for (int i = 0; i < 80; i++)
            {
                if (position.Tiles[i].Owner == Player.One)
                {
                    playerOnePaths[i] = pathFinder.FindPath(position, i, position.Tiles[i].Antipode);
                }
                if (position.Tiles[i].Owner == Player.Two)
                {
                    playerTwoPaths[i] = pathFinder.FindPath(position, i, position.Tiles[i].Antipode);
                }
            }

            // Find the best paths for each player
            PathResult one = playerOnePaths.Where(x => x != null).OrderBy(x => x.Distance).FirstOrDefault();
            PathResult two = playerTwoPaths.Where(x => x != null).OrderBy(x => x.Distance).FirstOrDefault();

            // Positive scores are good for player one and negative are good for player two
            int score = (two?.Distance - one?.Distance) ?? 0;

            if (enableHash)
            {
                // Save the hash to speed up future evaluations
                hash.Set(position, score);
            }

            return score;
        }

        private List<Move> GetFilteredMoves(Board position)
        {
            // Get all non-volcano moves
            List<Move> allMoves = position.GetMoves(true, true, true, VolcanoGame.Settings.MaxMagmaChamberLevel + 1);
            if (allMoves.Count == 0)
            {
                // Get all available moves without condition
                allMoves = position.GetMoves();
            }
            if (allMoves.Count == 0)
            {
                // There are no valid moves
                return allMoves;
            }
            if (allMoves[0].MoveType == MoveType.AllGrow)
            {
                // It's a growth phase, so don't wast time
                return allMoves;
            }

            // For each tile we own, figure out how long it'll take to get to it's antipode
            PathResult[] paths = new PathResult[80];
            PathResult[] enemyPaths = new PathResult[80];
            for (int i = 0; i < 80; i++)
            {
                if (position.Tiles[i].Owner == position.Player)
                {
                    paths[i] = pathFinder.FindPath(position, i, position.Tiles[i].Antipode);
                }
                else if (position.Tiles[i].Owner != Player.Empty)
                {
                    enemyPaths[i] = pathFinder.FindPath(position, i, position.Tiles[i].Antipode);
                }
            }

            // Of all the calculated paths, find the one that's fastest
            PathResult best = paths.Where(x => x != null && x.Distance != 0).OrderBy(x => x.Distance).FirstOrDefault();
            PathResult bestEnemy = enemyPaths.Where(x => x != null && x.Distance != 0).OrderBy(x => x.Distance).FirstOrDefault();

            List<Move> moves = new List<Move>();
            if (best != null)
            {
                // Create a list of moves that are on the ideal path
                foreach (int i in best.Path)
                {
                    if (position.Tiles[i].Owner == Player.Empty)
                    {
                        Move move = allMoves.Where(x => x.TileIndex == i).FirstOrDefault();
                        if (move != null)
                        {
                            moves.Add(move);
                        }
                    }
                }

                if (bestEnemy != null)
                {
                    // Create a list of moves that are on the enemy's best path
                    foreach (int i in bestEnemy.Path)
                    {
                        if (position.Tiles[i].Owner == Player.Empty)
                        {
                            Move move = allMoves.Where(x => x.TileIndex == i).FirstOrDefault();
                            if (move != null)
                            {
                                moves.Insert(0, move);
                            }
                        }
                    }
                }

                // If there are no empty tiles left, pick magma chambers
                if (moves.Count == 0)
                {
                    foreach (int i in best.Path)
                    {
                        if (position.Tiles[i].Owner == position.Player && position.Tiles[i].Value <= VolcanoGame.Settings.MaxMagmaChamberLevel)
                        {
                            Move move = allMoves.Where(x => x.TileIndex == i).FirstOrDefault();
                            if (move != null)
                            {
                                moves.Add(move);
                            }
                        }
                    }
                }
            }

            // If we haven't found a move yet, pick a random one
            if (moves.Count == 0)
            {
                if (allMoves.Count == 0)
                {
                    // Get all available moves without condition
                    allMoves = position.GetMoves();
                }

                moves.Add(allMoves[random.Next(allMoves.Count)]);
            }

            // Enforce a maxmimum number of candidate moves to speed up the search
            while (moves.Count > maxTilesToPick)
            {
                moves.RemoveAt(random.Next(moves.Count));
            }

            return moves;
        }

        private SearchResult AlphaBetaSearch(Board position, int depth, int alpha, int beta)
        {
            // Evaluate the position
            int eval = EvaluatePosition(position);

            // See if someone won
            if (Math.Abs(eval) > int.MaxValue)
            {
                return new SearchResult()
                {
                    Score = eval
                };
            }

            // We've reached the depth of our search, so return the heuristic evaluation of the position
            if (depth <= 0 || cancellationToken.Cancelled)
            {
                return new SearchResult()
                {
                    Score = eval
                };
            }

            bool maximizingPlayer = position.Player == Player.One;
            SearchResult best = new SearchResult()
            {
                Score = maximizingPlayer ? int.MinValue : int.MaxValue
            };

            List<Move> moves = GetFilteredMoves(position);

            // If we have no moves, return the evaluation of the position
            if (moves.Count == 0)
            {
                return new SearchResult()
                {
                    Score = eval
                };
            }
            
            foreach (Move move in moves)
            {
                // Copy the board and make a move
                Board copy = new Board(position);
                copy.MakeMove(move, false, true);
                
                // Find opponents best counter move
                SearchResult child = AlphaBetaSearch(copy, depth - 1, alpha, beta);

                // Store the evaluation
                move.Evaluation = child.Score;

                if (maximizingPlayer)
                {
                    if (child.Score > best.Score)
                    {
                        best.Score = child.Score;
                        best.BestMove = move;
                    }

                    alpha = Math.Max(alpha, best.Score);

                    if (beta <= alpha)
                    {
                        // Beta cutoff
                        break;
                    }
                }
                else
                {
                    if (child.Score < best.Score)
                    {
                        best.Score = child.Score;
                        best.BestMove = move;
                    }

                    beta = Math.Min(beta, best.Score);

                    if (beta <= alpha)
                    {
                        // Alpha cutoff
                        break;
                    }
                }
            }

            return best;
        }
    }
}

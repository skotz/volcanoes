using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Engine
{
    class MiniMaxAlphaBetaEngine : IEngine
    {
        private long evaluations;
        private int searchDepth;

        public MiniMaxAlphaBetaEngine(int depth)
        {
            searchDepth = depth;
        }

        public SearchResult GetBestMove(Board state)
        {
            evaluations = 0;
            
            SearchResult result = AlphaBetaSearch(state, searchDepth, int.MinValue, int.MaxValue);

            result.Evaluations = evaluations;

            return result;
        }

        private int EvaluatePosition(Board position)
        {
            // Positive scores are good for player one and negative are good for player two
            int score = 0;

            // TODO: need to work in logic around longest paths

            for (int i = 0; i < 80; i++)
            {
                // Get a point for each connected tile
                if (position.Tiles[i].Value > 0)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        if (position.Tiles[i].Owner == Player.One && position.Tiles[position.Tiles[i].AdjacentIndexes[c]].Owner == Player.One)
                        {
                            score++;
                        }
                        if (position.Tiles[i].Owner == Player.Two && position.Tiles[position.Tiles[i].AdjacentIndexes[c]].Owner == Player.Two)
                        {
                            score--;
                        }
                    }
                }
            }

            return score;
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
            if (depth <= 0)
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
            
            List<Move> moves = position.GetMoves();

            // Remove moves that aren't adjacent to an exiting tile
            for (int i = moves.Count - 1; i >= 0; i--)
            {
                bool ok = false;
                if (position.Tiles[i].Owner == position.Player)
                {
                    ok = true;
                }

                for (int c = 0; c < 3; c++)
                {
                    if (position.Tiles[position.Tiles[i].AdjacentIndexes[c]].Value > 0)
                    {
                        ok = true;
                        break;
                    }
                }

                // Don't remove the last move
                if (!ok && moves.Count > 1)
                {
                    moves.RemoveAt(i);
                }
            }

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
                copy.MakeMove(move);
                
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

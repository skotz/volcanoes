using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Volcano.Game;
using Volcano.Search;

namespace Volcano.Engine
{
    /// <summary>
    /// Minimax with alpha-beta pruning and iterative deepening. Leaf positions are scored by the
    /// AlphaZero V2 value head, normalized to Player One's perspective. Leaf evaluations are cached,
    /// and the best move and score of every searched position are remembered to order moves on later
    /// iterations so alpha-beta gets more cutoffs.
    /// </summary>
    internal class MiniMaxPlusEngine : IEngine, IStatus
    {
        private const int _winScore = 1000000;
        private const int _bufferMilliseconds = 250;
        private const int _maxCacheEntries = 5000000;
        private const long _turnKey = unchecked((long)0x9E3779B97F4A7C15);

        private static readonly long[] _dormantKeys = CreateDormantKeys();

        private readonly int _maxDepth;
        private readonly IMoveFilter _moveFilter;
        private readonly AlphaZeroEngine _alphaZero;

        // Leaf evaluations do not depend on the search window, so they can be cached exactly
        private readonly Dictionary<long, int> _leafCache = new Dictionary<long, int>();

        // Best move and score from the most recent search of each position. Used for move ordering only.
        private readonly Dictionary<long, NodeInfo> _nodeInfo = new Dictionary<long, NodeInfo>();

        private long _evaluations;
        private bool _reachedDepthLimit;
        private Stopwatch _timer;
        private long _timeLimitMilliseconds;
        private EngineCancellationToken _token;

        public event EventHandler<EngineStatus> OnStatus;

        /// <summary>
        /// Searches with no depth limit, bounded by time, cancellation, or a proven result.
        /// </summary>
        public MiniMaxPlusEngine()
            : this(int.MaxValue, null)
        {
        }

        /// <summary>
        /// Searches up to a maximum depth, bounded also by time and cancellation.
        /// </summary>
        /// <param name="maxDepth">Maximum search depth in plies.</param>
        public MiniMaxPlusEngine(int maxDepth)
            : this(maxDepth, null)
        {
        }

        /// <summary>
        /// Searches up to a maximum depth using an optional move filter for child generation.
        /// </summary>
        /// <param name="maxDepth">Maximum search depth in plies.</param>
        /// <param name="moveFilter">Filter used to get child moves. When null, Board.GetMoves is used.</param>
        public MiniMaxPlusEngine(int maxDepth, IMoveFilter moveFilter)
        {
            _maxDepth = maxDepth;
            _moveFilter = moveFilter;
            _alphaZero = new AlphaZeroEngine(VolcanoZeroVersion.V2);
        }

        /// <summary>
        /// Runs iterative deepening until time runs out, cancellation is requested, the maximum depth
        /// is reached, the tree is fully solved, or the root result is a proven win or loss.
        /// </summary>
        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            _timer = Stopwatch.StartNew();
            _evaluations = 0;
            _token = token;
            _timeLimitMilliseconds = maxSeconds * 1000L - _bufferMilliseconds;

            // Keep the caches bounded in memory
            if (_leafCache.Count > _maxCacheEntries)
            {
                _leafCache.Clear();
            }
            if (_nodeInfo.Count > _maxCacheEntries)
            {
                _nodeInfo.Clear();
            }

            var rootMoves = GetChildMoves(state);
            if (rootMoves.Count == 0)
            {
                return new SearchResult
                {
                    BestMove = -3,
                    Evaluations = _evaluations,
                    Milliseconds = _timer.ElapsedMilliseconds
                };
            }

            int bestMove = rootMoves[0];
            int bestScore = 0;
            bool maximizing = state.Player == Player.One;
            long rootKey = GetNodeKey(state);

            for (int depth = 1; depth <= _maxDepth; depth++)
            {
                if (IsCancelled())
                {
                    break;
                }

                _reachedDepthLimit = false;

                // Order root moves using the previous depth's results
                var children = OrderChildren(state, rootMoves, maximizing, rootKey);

                var depthScores = new Dictionary<int, int>();
                int alpha = int.MinValue;
                int beta = int.MaxValue;
                int depthBest = children[0].Move;
                int depthBestScore = maximizing ? int.MinValue : int.MaxValue;
                bool completed = true;

                foreach (var child in children)
                {
                    int score;
                    try
                    {
                        score = Search(child.Board, depth - 1, alpha, beta);
                    }
                    catch (SearchCancelledException)
                    {
                        completed = false;
                        break;
                    }

                    depthScores[child.Move] = score;

                    if (maximizing ? score > depthBestScore : score < depthBestScore)
                    {
                        depthBestScore = score;
                        depthBest = child.Move;
                    }

                    if (maximizing)
                    {
                        alpha = Math.Max(alpha, depthBestScore);
                    }
                    else
                    {
                        beta = Math.Min(beta, depthBestScore);
                    }
                }

                // A partially searched depth is discarded
                if (!completed)
                {
                    break;
                }

                bestMove = depthBest;
                bestScore = depthBestScore;
                StoreNodeInfo(rootKey, depth, bestScore, bestMove);

                ReportStatus(depth, children, depthScores, maximizing);

                // The result is proven, so deeper search cannot change the move
                if (Math.Abs(bestScore) >= _winScore)
                {
                    break;
                }

                // Nothing was cut off by the depth limit, so the whole tree has been searched
                if (!_reachedDepthLimit)
                {
                    break;
                }
            }

            return new SearchResult
            {
                BestMove = bestMove,
                Score = bestScore,
                Evaluations = _evaluations,
                Milliseconds = _timer.ElapsedMilliseconds
            };
        }

        /// <summary>
        /// Minimax with alpha-beta pruning. Maximizing and minimizing are chosen from the side to move
        /// at each node rather than by negation, because grow moves can repeat the same player's turn.
        /// </summary>
        private int Search(Board node, int depth, int alpha, int beta)
        {
            ThrowIfCancelled();

            if (node.Winner != Player.Empty)
            {
                _evaluations++;
                return GetTerminalScore(node.Winner);
            }

            if (depth <= 0)
            {
                _reachedDepthLimit = true;
                return EvaluateLeaf(node);
            }

            var moves = GetChildMoves(node);
            if (moves.Count == 0)
            {
                _evaluations++;
                return 0;
            }

            bool maximizing = node.Player == Player.One;
            long key = GetNodeKey(node);
            var children = OrderChildren(node, moves, maximizing, key);

            int best = maximizing ? int.MinValue : int.MaxValue;
            int bestMove = children[0].Move;

            foreach (var child in children)
            {
                int score = Search(child.Board, depth - 1, alpha, beta);

                if (maximizing)
                {
                    if (score > best)
                    {
                        best = score;
                        bestMove = child.Move;
                    }
                    alpha = Math.Max(alpha, best);
                }
                else
                {
                    if (score < best)
                    {
                        best = score;
                        bestMove = child.Move;
                    }
                    beta = Math.Min(beta, best);
                }

                if (beta <= alpha)
                {
                    break;
                }
            }

            StoreNodeInfo(key, depth, best, bestMove);
            return best;
        }

        /// <summary>
        /// Returns the cached AlphaZero value head for a position, scaled to an int, from Player One's perspective.
        /// </summary>
        private int EvaluateLeaf(Board node)
        {
            _evaluations++;

            long key = GetNodeKey(node);
            if (_leafCache.TryGetValue(key, out int cached))
            {
                return cached;
            }

            float value = _alphaZero.Evaluate(node).Value;
            int score = (int)(value * _winScore);
            _leafCache[key] = score;
            return score;
        }

        /// <summary>
        /// Generates the children of a node and orders them for alpha-beta. The best move stored from a
        /// previous search of this position goes first, then the rest by their known score, best for the
        /// side to move first. Terminal children use their exact result, and unknown children go last.
        /// </summary>
        private List<Child> OrderChildren(Board node, List<int> moves, bool maximizing, long nodeKey)
        {
            int preferredMove = _nodeInfo.TryGetValue(nodeKey, out NodeInfo info) ? info.BestMove : -1;

            var children = new List<Child>(moves.Count);
            foreach (int move in moves)
            {
                var board = new Board(node);
                board.MakeMove(move, true, true);

                children.Add(new Child
                {
                    Move = move,
                    Board = board,
                    KnownScore = GetKnownScore(board),
                });
            }

            return children
                .OrderBy(c => c.Move == preferredMove ? 0 : 1)
                .ThenByDescending(c => c.KnownScore.HasValue ? (maximizing ? c.KnownScore.Value : -c.KnownScore.Value) : int.MinValue)
                .ToList();
        }

        /// <summary>
        /// Returns the exact terminal score, or the score stored from an earlier search of this position.
        /// Returns null when nothing is known.
        /// </summary>
        private int? GetKnownScore(Board board)
        {
            if (board.Winner != Player.Empty)
            {
                return GetTerminalScore(board.Winner);
            }

            if (_nodeInfo.TryGetValue(GetNodeKey(board), out NodeInfo info))
            {
                return info.Score;
            }

            return null;
        }

        private void StoreNodeInfo(long key, int depth, int score, int bestMove)
        {
            if (_nodeInfo.TryGetValue(key, out NodeInfo existing) && existing.Depth > depth)
            {
                return;
            }

            _nodeInfo[key] = new NodeInfo
            {
                Depth = depth,
                Score = score,
                BestMove = bestMove
            };
        }

        /// <summary>
        /// Hash of the full position: tiles, dormant volcanoes, and turn (which determines the player and grow schedule).
        /// </summary>
        private static long GetNodeKey(Board board)
        {
            long key = board.GetFullHash();

            key ^= unchecked(board.Turn * _turnKey);

            for (int i = 0; i < 80; i++)
            {
                if (board.Dormant[i])
                {
                    key ^= _dormantKeys[i];
                }
            }

            return key;
        }

        private static long[] CreateDormantKeys()
        {
            var random = new Random(1234);
            var keys = new long[80];
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = random.NextInt64();
            }
            return keys;
        }

        /// <summary>
        /// Exact score for a finished game. A draw scores 0, a Player One win is the maximum, and a Player Two win is the minimum.
        /// </summary>
        private static int GetTerminalScore(Player winner)
        {
            switch (winner)
            {
                case Player.One:
                    return _winScore;
                case Player.Two:
                    return -_winScore;
                default:
                    return 0;
            }
        }

        private List<int> GetChildMoves(Board node)
        {
            return _moveFilter != null ? _moveFilter.GetMoves(node) : node.GetMoves();
        }

        private bool IsCancelled()
        {
            return _token.Cancelled || _timer.ElapsedMilliseconds >= _timeLimitMilliseconds;
        }

        private void ThrowIfCancelled()
        {
            if (IsCancelled())
            {
                throw new SearchCancelledException();
            }
        }

        private void ReportStatus(int depth, List<Child> children, Dictionary<int, int> scores, bool maximizing)
        {
            if (OnStatus == null)
            {
                return;
            }

            var status = new EngineStatus();
            foreach (var child in children)
            {
                if (scores.TryGetValue(child.Move, out int score))
                {
                    status.Add(child.Move, score, $"depth {depth}", maximizing ? score : -score);
                }
            }
            status.Sort();
            OnStatus.Invoke(this, status);
        }

        private sealed class NodeInfo
        {
            public int Depth;
            public int Score;
            public int BestMove;
        }

        private sealed class Child
        {
            public int Move;
            public Board Board;
            public int? KnownScore;
        }

        private class SearchCancelledException : Exception
        {
        }
    }
}

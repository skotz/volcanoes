using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TorchSharp.Modules;
using Volcano.Game;
using Volcano.Search;

namespace Volcano.Engine
{
    internal enum MCTSVersion
    {
        /// <summary>
        /// default mcts
        /// </summary>
        V1,

        /// <summary>
        /// mcts with fast win, draw count, and player fix
        /// </summary>
        V2,

        /// <summary>
        /// mcts with puct
        /// </summary>
        V3,
    }

    internal class MonteCarloTreeSearchEngine : IEngine, IStatus
    {
        private Random random;
        private int simulationCount;
        private int maxSeconds;
        private int visitedNodes;
        public bool _allowForcedWins;
        private bool _allowHash;
        private bool _allowFastWinSearch;
        private bool _fixedLastPlayer;
        public bool _countDraws;
        private bool _useOpeningBook;
        private OpeningBook _book;

        private int bufferMilliseconds = 200;
        private EngineCancellationToken cancel;

        private Stopwatch timer;

        private Stopwatch statusUpdate;
        private int millisecondsBetweenUpdates = 500;

        public event EventHandler<EngineStatus> OnStatus;

        public ConcurrentDictionary<long, Player> winHashes = new ConcurrentDictionary<long, Player>();

        public double _ucbFactor = 2.0;
        public double _temperature = 0.25;
        public bool simplifyFirstMove;
        public int forcedIterations = -1;
        public int forcedGap = -1;

        public MonteCarloTreeSearchNode originalRootNode;

        public bool _persistable;
        public bool _persistableFull;

        public bool _puct;

        public MonteCarloTreeSearchEngine(double ucbFactor)
        {
            random = new Random();
            _ucbFactor = ucbFactor;
        }

        public MonteCarloTreeSearchEngine(bool allowForcedWins)
        {
            random = new Random();
            _allowForcedWins = allowForcedWins;
        }

        public MonteCarloTreeSearchEngine(bool allowForcedWins, bool allowHash, bool allowFastWinSearch, bool fixedLastPlayer, string openingBook)
        {
            random = new Random();
            _allowForcedWins = allowForcedWins;
            _allowHash = allowHash;
            _allowFastWinSearch = allowFastWinSearch;
            _fixedLastPlayer = fixedLastPlayer;
            _useOpeningBook = !string.IsNullOrEmpty(openingBook);

            if (_useOpeningBook)
            {
                _book = new OpeningBook(openingBook);
                _useOpeningBook = _book.Loaded;
            }
        }

        public MonteCarloTreeSearchEngine()
        {
            random = new Random();
            _allowForcedWins = true;
        }

        public MonteCarloTreeSearchEngine(MCTSVersion version)
            : this(version, (OpeningBook)null)
        {
        }

        public MonteCarloTreeSearchEngine(MCTSVersion version, string book)
            : this(version, new OpeningBook(book))
        {
        }

        public MonteCarloTreeSearchEngine(MCTSVersion version, OpeningBook book)
            : this()
        {
            switch (version)
            {
                case MCTSVersion.V1:
                    _allowForcedWins = true;
                    _allowHash = false;
                    _allowFastWinSearch = false;
                    _fixedLastPlayer = false;
                    _useOpeningBook = false;
                    _countDraws = false;
                    break;

                case MCTSVersion.V2:
                    _allowForcedWins = true;
                    _allowHash = false;
                    _allowFastWinSearch = true;
                    _fixedLastPlayer = true;
                    _useOpeningBook = false;
                    _countDraws = true;
                    break;

                case MCTSVersion.V3:
                    _allowForcedWins = true;
                    _allowHash = false;
                    _allowFastWinSearch = true;
                    _fixedLastPlayer = true;
                    _useOpeningBook = false;
                    _countDraws = true;
                    _puct = true;
                    _ucbFactor = 0.6; // 1.5;
                    _temperature = 0.75; // 0.55;
                    break;

                default:
                    throw new ArgumentException("Invalid MCTS Version");
            }

            if (book != null)
            {
                _book = book;
                _useOpeningBook = _book.Loaded;
            }
        }

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            if (_useOpeningBook)
            {
                if (maxSeconds <= _book.Seconds || _book.Seconds == -1)
                {
                    var bookMove = _book.GetMove(state.Transcript);

                    if (bookMove >= 0)
                    {
                        return new SearchResult
                        {
                            BestMove = bookMove
                        };
                    }
                }
            }

            timer = Stopwatch.StartNew();
            statusUpdate = Stopwatch.StartNew();
            visitedNodes = 0;
            simulationCount = 0;

            this.maxSeconds = maxSeconds;

            cancel = new EngineCancellationToken(() => token.Cancelled || timer.ElapsedMilliseconds >= maxSeconds * 1000L - bufferMilliseconds);

            int best = MonteCarloTreeSearch(state);

            return new SearchResult
            {
                BestMove = best,
                Evaluations = visitedNodes,
                Simulations = simulationCount,
                Milliseconds = timer.ElapsedMilliseconds
            };
        }

        protected virtual List<int> GetMoves(Board state)
        {
            if (simplifyFirstMove && state.Turn == 1)
            {
                return new List<int>() { 0, 1 };
            }

            return state.GetMoves();
        }

        private MonteCarloTreeSearchNode FindNodeByHash(MonteCarloTreeSearchNode node, long targetHash)
        {
            if (node == null)
            {
                return null;
            }

            var queue = new Queue<MonteCarloTreeSearchNode>();
            queue.Enqueue(node);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current.hash == targetHash)
                {
                    return current;
                }

                foreach (var child in current.Children)
                {
                    queue.Enqueue(child);
                }
            }

            return null;
        }

        protected virtual int MonteCarloTreeSearch(Board rootState)
        {
            if (_persistable && originalRootNode != null)
            {
                var targetHash = rootState.GetFullHash();
                var matchingNode = FindNodeByHash(originalRootNode, targetHash);

                if (matchingNode != null)
                {
                    if (!_persistableFull)
                    {
                        // for regular bot play we don't need to retain previous history
                        originalRootNode = matchingNode;
                    }

                    return MonteCarloTreeSearchInternal(rootState, matchingNode);
                }
            }

            var rootNode = new MonteCarloTreeSearchNode(rootState, GetMoves, _fixedLastPlayer, _persistable);

            if (_persistable && originalRootNode == null)
            {
                originalRootNode = rootNode;
            }

            return MonteCarloTreeSearchInternal(rootState, rootNode);
        }

        private int MonteCarloTreeSearchInternal(Board rootState, MonteCarloTreeSearchNode rootNode)
        {
            var forceWin = false;
            var iterations = 0;

            // Force tree expansion for openening book creation
            var forceExpand = _persistable && forcedIterations == 80;
            if (_persistable)
            {
                iterations = (int)rootNode.Children.Sum(x => x.Visits);
            }

            while (forcedIterations == -1 ? (!cancel.Cancelled && !forceWin) : (iterations < forcedIterations))
            {
                iterations++;

                var node = rootNode;
                var state = new Board(rootState);

                state.allowHash = _allowHash;
                state.fastWinSearch = _allowFastWinSearch;

                if (_allowHash)
                {
                    state.winHashes = winHashes;
                }

                simulationCount++;

                // Select
                while (node.Untried.Count == 0 && node.Children.Count > 0)
                {
                    if (forceExpand && node.hash == rootState.GetFullHash())
                    {
                        node = node.Children.OrderBy(x => x.Visits).FirstOrDefault();
                    }
                    else
                    {
                        if (_puct)
                        {
                            node = node.SelectChildPUCT(state, _ucbFactor, _temperature, forcedIterations > 0 ? forcedIterations : (maxSeconds * 10000));
                        }
                        else
                        {
                            node = node.SelectChild(_ucbFactor);
                        }
                    }

                    state.MakeMove(node.Move);
                    visitedNodes++;
                }

                // Expand
                if (node.Untried.Count > 0)
                {
                    var move = node.Untried[random.Next(node.Untried.Count)];
                    var player = state.Player;
                    state.MakeMove(move);
                    node = node.AddChild(state, move, player, _persistable);
                    visitedNodes++;
                }

                // Simulate
                while (state.Winner == Player.Empty && state.Turn < VolcanoGame.Settings.TournamentAdjudicateMaxTurns)
                {
                    var moves = state.GetMoves();
                    if (moves.Count == 0)
                    {
                        break;
                    }
                    state.MakeMove(moves[random.Next(moves.Count)]);
                    visitedNodes++;
                }

                // Backpropagate
                while (node != null)
                {
                    if (_countDraws)
                    {
                        node.Update(state.Winner == Player.Draw ? 0.5 : (state.Winner == node.LastToMove ? 1.0 : 0.0));
                    }
                    else
                    {
                        node.Update(state.Winner == node.LastToMove ? 1.0 : 0.0);
                    }
                    node = node.Parent;
                    visitedNodes++;
                }

                // Cut Short
                if (_allowForcedWins)
                {
                    foreach (var child in rootNode.Children)
                    {
                        // If we have a potential move that has a 100% win rate and it's been visited a lot of times, stop searching
                        if (child.Visits > 500 && child.Wins == child.Visits)
                        {
                            forceWin = true;
                        }
                    }
                }

                // Update Status
                if ((statusUpdate.ElapsedMilliseconds > millisecondsBetweenUpdates || forceWin) && OnStatus != null)
                {
                    EngineStatus status = new EngineStatus();
                    foreach (var child in rootNode.Children)
                    {
                        double eval = Math.Round((child.Visits > 0 ? 200.0 * child.Wins / child.Visits : 0) - 100.0, 2);
                        string pv = "";
                        if (_puct)
                        {
                            pv += $"[{child.Prior.ToString("0.000000")}]   ";
                        }
                        var c = child;
                        while (c != null && c.Move >= 0 && c.Move <= 80)
                        {
                            pv += Constants.TileNames[c.Move] + " (" + c.Wins + "/" + c.Visits + ")   ";
                            c = c.Children?.OrderBy(x => x.Visits)?.ThenBy(x => x.Wins)?.LastOrDefault();
                        }
                        status.Add(child?.Move ?? 80, eval, pv, child.Visits);
                    }
                    status.Sort();
                    OnStatus?.Invoke(this, status);
                    statusUpdate = Stopwatch.StartNew();
                }

                // Cut Short (for book gen)
                if (forcedGap > 0)
                {
                    var nodes = rootNode.Children.OrderByDescending(x => x.Visits).ToList();
                    if (nodes.Count >= 2 && nodes[0].Visits >= forcedGap && nodes[1].Visits >= forcedGap && nodes.All(x => x.Visits > 0))
                    {
                        var gap = nodes[0].Visits - nodes[1].Visits;
                        var score = nodes[0].Wins / nodes[0].Visits - nodes[1].Wins / nodes[1].Visits;
                        if (gap >= forcedGap && score > 0)
                        {
                            break;
                        }
                    }
                }
            }

            return rootNode.Children.OrderBy(x => x.Visits).LastOrDefault().Move;
        }

        public class MonteCarloTreeSearchNode
        {
            private Func<Board, List<int>> _getMoves;
            private bool _fixedLastPlayer;
            public double Wins;
            public double Visits;
            public MonteCarloTreeSearchNode Parent;
            public Player LastToMove;
            public int Move;
            public List<MonteCarloTreeSearchNode> Children;
            public List<int> Untried;
            public long hash;
            public double Prior;
            public bool SetPrior;
            public int Interesting;

            public MonteCarloTreeSearchNode(Board state, Func<Board, List<int>> getMoves, bool fixedLastPlayer, bool persist)
                : this(state, -2, null, getMoves, fixedLastPlayer, Player.Empty, persist)
            {
            }

            public MonteCarloTreeSearchNode(Board state, int move, MonteCarloTreeSearchNode parent, Func<Board, List<int>> getMoves, bool fixedLastPlayer, Player player, bool persist)
            {
                _getMoves = getMoves;

                _fixedLastPlayer = fixedLastPlayer;

                Move = move;
                Parent = parent;

                Children = new List<MonteCarloTreeSearchNode>();
                Wins = 0.0;
                Visits = 0.0;

                if (state != null)
                {
                    if (persist)
                    {
                        hash = state.GetFullHash();
                    }

                    Untried = _getMoves(state);

                    if (_fixedLastPlayer)
                    {
                        if (move == -2)
                        {
                            // this is the state before making any moves
                            LastToMove = state.Player;
                        }
                        else
                        {
                            LastToMove = player;
                        }
                    }
                    else
                    {
                        LastToMove = state.GetPlayerForPreviousTurn();
                    }
                }
                else
                {
                    Untried = new List<int>();
                }
            }

            public MonteCarloTreeSearchNode SelectChild(double ucbFactor)
            {
                return Children.OrderBy(x => UpperConfidenceBound(ucbFactor, x)).LastOrDefault();
            }

            public MonteCarloTreeSearchNode AddChild(Board state, int move, Player player, bool persist)
            {
                var newNode = new MonteCarloTreeSearchNode(state, move, this, _getMoves, _fixedLastPlayer, player, persist);
                Untried.Remove(move);
                Children.Add(newNode);
                return newNode;
            }

            public void Update(double result)
            {
                Visits++;
                Wins += result;
            }

            private double UpperConfidenceBound(double ucbFactor, MonteCarloTreeSearchNode node)
            {
                return node.Wins / node.Visits + Math.Sqrt(ucbFactor * Math.Log(Visits) / node.Visits);
            }

            public MonteCarloTreeSearchNode SelectChildPUCT(Board state, double factor, double temperature, int simulations)
            {
                if (Untried.Count == 0 && !SetPrior)
                {
                    var paths = new TotalPathFinder().GetAllPaths(state);
                    var scores = new double[80];

                    var bestPath = paths.MyPaths.OrderBy(x => x.Distance).FirstOrDefault();
                    var bestOppPath = paths.OpponentPaths.OrderBy(x => x.Distance).FirstOrDefault();

                    if (bestPath != null)
                    {
                        foreach (var tile in bestPath.Path)
                        {
                            scores[tile] += state.Tiles[tile] == 0 ? 1 : 0.5;

                            if (state.Tiles[tile] == 0)
                            {
                                Interesting++;
                            }
                        }
                    }

                    if (bestOppPath != null)
                    {
                        foreach (var tile in bestOppPath.Path)
                        {
                            scores[tile] += state.Tiles[tile] == 0 ? 1 : 0.5;

                            if (state.Tiles[tile] == 0)
                            {
                                Interesting++;
                            }
                        }
                    }

                    foreach (var path in paths.MyPaths)
                    {
                        foreach (var tile in path.Path)
                        {
                            scores[tile] += 0.01;
                        }
                    }

                    foreach (var path in paths.OpponentPaths)
                    {
                        foreach (var tile in path.Path)
                        {
                            scores[tile] += 0.01;
                        }
                    }

                    foreach (var child in Children)
                    {
                        child.Prior = scores[child.Move];
                    }

                    ApplySoftmaxPriors(temperature);

                    SetPrior = true;
                }

                var minAllowedBranches = Math.Clamp(Interesting, 5, 20);

                // 1. Sort all children by their raw heuristic score / prior descending
                var sortedChildren = Children.OrderByDescending(c => c.Prior).ToList();

                // 2. Calculate how many branches are allowed to be active right now
                // Alpha controls baseline width, Beta controls how fast it widens
                double alpha = minAllowedBranches;
                // double beta = 0.15;  // Slowly unlock the rest over 1,000,000 sims
                double beta = (Math.Log(80) - Math.Log(minAllowedBranches)) / Math.Log(simulations + 1);

                var maxAllowedBranches = (int)(alpha * Math.Pow(Visits + 1, beta));

                // Ensure we don't exceed the total 80 moves, but always allow at least 20
                if (sortedChildren.Count > minAllowedBranches)
                {
                    maxAllowedBranches = Math.Clamp(maxAllowedBranches, minAllowedBranches, sortedChildren.Count);
                }

                // 3. Run your UCB selection ONLY on the allowed subset
                MonteCarloTreeSearchNode bestChild = null;
                double bestScore = double.MinValue;

                for (int i = 0; i < Math.Min(sortedChildren.Count, maxAllowedBranches); i++)
                {
                    var child = sortedChildren[i];
                    double ucbScore = PUCT(factor, child);

                    if (ucbScore > bestScore)
                    {
                        bestScore = ucbScore;
                        bestChild = child;
                    }
                }

                return bestChild;
            }

            private double PUCT(double factor, MonteCarloTreeSearchNode node)
            {
                double exploitation = node.Visits == 0 ? 0.5 : (double)node.Wins / node.Visits;
                double exploration = factor * node.Prior * (Math.Sqrt(Visits) / (1 + node.Visits));

                return exploitation + exploration;
            }

            private void ApplySoftmaxPriors(double temperature)
            {
                Children.ForEach(x => x.Prior += 0.00001);

                // 1. Find the maximum score to ensure numerical stability (prevents float overflow)
                double maxScore = Children.Max(x => x.Prior);

                // 2. Compute the exponentiated values, adjusted by temperature
                double[] expScores = new double[Children.Count];
                double sumExp = 0.0;

                for (int i = 0; i < Children.Count; i++)
                {
                    // Subtracting maxScore ensures the largest exponent evaluated is e^0 = 1
                    expScores[i] = Math.Exp((Children[i].Prior - maxScore) / temperature);
                    sumExp += expScores[i];
                }

                // 3. Assign the normalized prior probabilities back to your nodes
                for (int i = 0; i < Children.Count; i++)
                {
                    // Each prior will now properly sit between 0 and 1, and all priors will sum to exactly 1.0
                    Children[i].Prior = expScores[i] / sumExp;
                }
            }
        }
    }
}
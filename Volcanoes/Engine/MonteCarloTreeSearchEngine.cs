using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Volcano.Game;

namespace Volcano.Engine
{
    internal class MonteCarloTreeSearchEngine : IEngine, IStatus
    {
        private Random random;
        private int simulationCount;
        private int visitedNodes;
        private bool _allowForcedWins;

        private int bufferMilliseconds = 200;
        private EngineCancellationToken cancel;

        private Stopwatch statusUpdate;
        private int millisecondsBetweenUpdates = 500;

        public event EventHandler<EngineStatus> OnStatus;

        public MonteCarloTreeSearchEngine(bool allowForcedWins)
        {
            random = new Random();
            _allowForcedWins = allowForcedWins;
        }

        public MonteCarloTreeSearchEngine()
        {
            random = new Random();
            _allowForcedWins = true;
        }

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            Stopwatch timer = Stopwatch.StartNew();
            statusUpdate = Stopwatch.StartNew();
            visitedNodes = 0;
            simulationCount = 0;

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
            return state.GetMoves();
        }

        private int MonteCarloTreeSearch(Board rootState)
        {
            var rootNode = new MonteCarloTreeSearchNode(rootState, GetMoves);
            var forceWin = false;

            while (!cancel.Cancelled && !forceWin)
            {
                var node = rootNode;
                var state = new Board(rootState);
                simulationCount++;

                // Select
                while (node.Untried.Count == 0 && node.Children.Count > 0)
                {
                    node = node.SelectChild();
                    state.MakeMove(node.Move);
                    visitedNodes++;
                }

                // Expand
                if (node.Untried.Count > 0)
                {
                    var move = node.Untried[random.Next(node.Untried.Count)];
                    state.MakeMove(move);
                    node = node.AddChild(state, move);
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
                    node.Update(state.Winner == node.LastToMove ? 1.0 : 0.0);
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
                if (statusUpdate.ElapsedMilliseconds > millisecondsBetweenUpdates && OnStatus != null)
                {
                    EngineStatus status = new EngineStatus();
                    foreach (var child in rootNode.Children)
                    {
                        double eval = Math.Round((child.Visits > 0 ? 200.0 * child.Wins / child.Visits : 0) - 100.0, 2);
                        string pv = "";
                        var c = child;
                        while (c != null && c.Move >= 0 && c.Move <= 80)
                        {
                            pv += Constants.TileNames[c.Move] + " (" + c.Wins + "/" + c.Visits + ")   ";
                            c = c.Children?.OrderBy(x => x.Visits)?.ThenBy(x => x.Wins)?.LastOrDefault();
                        }
                        status.Add(child?.Move ?? 80, eval, pv);
                    }
                    status.Sort();
                    OnStatus(this, status);
                    statusUpdate = Stopwatch.StartNew();
                }
            }

            return rootNode.Children.OrderBy(x => x.Visits).LastOrDefault().Move;
        }

        private class MonteCarloTreeSearchNode
        {
            private Func<Board, List<int>> _getMoves;

            public double Wins;
            public double Visits;
            public MonteCarloTreeSearchNode Parent;
            public Player LastToMove;
            public int Move;
            public List<MonteCarloTreeSearchNode> Children;
            public List<int> Untried;

            public MonteCarloTreeSearchNode(Board state, Func<Board, List<int>> getMoves)
                : this(state, -2, null, getMoves)
            {
            }

            public MonteCarloTreeSearchNode(Board state, int move, MonteCarloTreeSearchNode parent, Func<Board, List<int>> getMoves)
            {
                _getMoves = getMoves;

                Move = move;
                Parent = parent;

                Children = new List<MonteCarloTreeSearchNode>();
                Wins = 0.0;
                Visits = 0.0;

                if (state != null)
                {
                    Untried = _getMoves(state);
                    LastToMove = state.GetPlayerForPreviousTurn();
                }
                else
                {
                    Untried = new List<int>();
                }
            }

            public MonteCarloTreeSearchNode SelectChild()
            {
                return Children.OrderBy(x => UpperConfidenceBound(x)).LastOrDefault();
            }

            public MonteCarloTreeSearchNode AddChild(Board state, int move)
            {
                var newNode = new MonteCarloTreeSearchNode(state, move, this, _getMoves);
                Untried.Remove(move);
                Children.Add(newNode);
                return newNode;
            }

            public void Update(double result)
            {
                Visits++;
                Wins += result;
            }

            private double UpperConfidenceBound(MonteCarloTreeSearchNode node)
            {
                return node.Wins / node.Visits + Math.Sqrt(2.0 * Math.Log(Visits) / node.Visits);
            }
        }
    }
}
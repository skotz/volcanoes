using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Engine
{
    class MonteCarloTreeSearchEngine : IEngine, IStatus
    {
        private Random random;
        private int evaluations;
        private int maxIterations;

        private int bufferMilliseconds = 200;
        private EngineCancellationToken cancel;

        private Stopwatch statusUpdate;
        private int millisecondsBetweenUpdates = 500;
        public event EventHandler<EngineStatus> OnStatus;
        
        public MonteCarloTreeSearchEngine()
        {
            random = new Random();
            maxIterations = 1000000;
        }

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            Stopwatch timer = Stopwatch.StartNew();
            statusUpdate = Stopwatch.StartNew();
            evaluations = 0;

            cancel = new EngineCancellationToken(() => token.Cancelled || timer.ElapsedMilliseconds >= maxSeconds * 1000 - bufferMilliseconds);

            Move best = MonteCarloTreeSearch(state);

            return new SearchResult
            {
                BestMove = best,
                Evaluations = evaluations,
                Milliseconds = timer.ElapsedMilliseconds
            };
        }
        
        protected virtual List<Move> GetMoves(Board state)
        {
            return state.GetMoves();
        }

        private Move MonteCarloTreeSearch(Board rootState)
        {
            var rootNode = new MonteCarloTreeSearchNode(rootState, GetMoves);
            
            for (int i = 0; i < maxIterations && !cancel.Cancelled; i++)
            {
                var node = rootNode;
                var state = new Board(rootState);
                evaluations++;

                // Select
                while (node.Untried.Count == 0 && node.Children.Count > 0)
                {
                    node = node.SelectChild();
                    state.MakeMove(node.Move);
                }

                // Expand
                if (node.Untried.Count > 0)
                {
                    var move = node.Untried[random.Next(node.Untried.Count)];
                    state.MakeMove(move);
                    node = node.AddChild(state, move);
                }

                // Simulate
                while (state.Winner == Player.Empty && state.Turn < VolcanoGame.Settings.TournamentAdjudicateMaxTurns)
                {
                    var moves = state.GetMoves();
                    state.MakeMove(moves[random.Next(moves.Count)]);
                }

                // Backpropagate
                while (node != null)
                {
                    node.Update(state.Winner == node.LastToMove ? 1.0 : 0.0);
                    node = node.Parent;
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
                        while (c != null && c.Move != null && c.Move.TileIndex >= 0 && c.Move.TileIndex < 80)
                        {
                            pv += Constants.TileNames[c.Move.TileIndex] + " (" + c.Wins + "/" + c.Visits + ")   ";
                            c = c.Children?.OrderBy(x => x.Visits)?.LastOrDefault();
                        }
                        status.Add(child?.Move?.TileIndex ?? 80, eval, pv);
                    }
                    status.Sort();
                    OnStatus(this, status);
                    statusUpdate = Stopwatch.StartNew();
                }
            }

            return rootNode.Children.OrderBy(x => x.Visits).LastOrDefault().Move;
        }

        class MonteCarloTreeSearchNode
        {
            private Board _state;
            private Func<Board, List<Move>> _getMoves;

            public double Wins;
            public double Visits;
            public MonteCarloTreeSearchNode Parent;
            public Player LastToMove;
            public Move Move;
            public List<MonteCarloTreeSearchNode> Children;
            public List<Move> Untried;

            public MonteCarloTreeSearchNode(Board state, Func<Board, List<Move>> getMoves)
                : this(state, null, null, getMoves)
            {
            }

            public MonteCarloTreeSearchNode(Board state, Move move, MonteCarloTreeSearchNode parent, Func<Board, List<Move>> getMoves)
            {
                _state = state;
                _getMoves = getMoves;

                Move = move;
                Parent = parent;

                Children = new List<MonteCarloTreeSearchNode>();
                Wins = 0.0;
                Visits = 0.0;

                if (_state != null)
                {
                    Untried = _getMoves(_state);
                    LastToMove = _state.GetPlayerForPreviousTurn();
                }
                else
                {
                    Untried = new List<Move>();
                }
            }

            public MonteCarloTreeSearchNode SelectChild()
            {
                return Children.OrderBy(x => UpperConfidenceBound(x)).LastOrDefault();
            }

            public MonteCarloTreeSearchNode AddChild(Board state, Move move)
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

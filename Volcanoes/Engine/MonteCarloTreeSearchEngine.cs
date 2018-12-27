using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Engine
{
    class MonteCarloTreeSearchEngine : IEngine
    {
        private Random random;
        private int evaluations;

        private int millisecondsPerMove;
        private int maxIterations;

        private EngineCancellationToken cancel;

        public MonteCarloTreeSearchEngine()
            : this(5)
        {
        }

        public MonteCarloTreeSearchEngine(int secondsPerMove)
        {
            random = new Random();
            millisecondsPerMove = secondsPerMove * 1000;
            maxIterations = 1000000;
        }

        public SearchResult GetBestMove(Board state, EngineCancellationToken token)
        {
            Stopwatch timer = Stopwatch.StartNew();
            evaluations = 0;

            cancel = new EngineCancellationToken(() => token.Cancelled || timer.ElapsedMilliseconds >= millisecondsPerMove);

            Move best = MonteCarloTreeSearch(state);

            return new SearchResult
            {
                BestMove = best,
                Evaluations = evaluations,
                Milliseconds = timer.ElapsedMilliseconds
            };
        }

        private Move MonteCarloTreeSearch(Board rootState)
        {
            var rootNode = new MonteCarloTreeSearchNode(rootState);
            
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
            }

            return rootNode.Children.OrderBy(x => x.Visits).LastOrDefault().Move;
        }

        class MonteCarloTreeSearchNode
        {
            private Board _state;
            private double wins;

            public double Visits;
            public MonteCarloTreeSearchNode Parent;
            public Player LastToMove;
            public Move Move;
            public List<MonteCarloTreeSearchNode> Children;
            public List<Move> Untried;

            public MonteCarloTreeSearchNode(Board state)
                : this(state, null, null)
            {
            }

            public MonteCarloTreeSearchNode(Board state, Move move, MonteCarloTreeSearchNode parent)
            {
                _state = state;
                Move = move;
                Parent = parent;

                Children = new List<MonteCarloTreeSearchNode>();
                wins = 0.0;
                Visits = 0.0;

                if (_state != null)
                {
                    Untried = _state.GetMoves();
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
                var newNode = new MonteCarloTreeSearchNode(state, move, this);
                Untried.Remove(move);
                Children.Add(newNode);
                return newNode;
            }

            public void Update(double result)
            {
                Visits++;
                wins += result;
            }

            private double UpperConfidenceBound(MonteCarloTreeSearchNode node)
            {
                return node.wins / node.Visits + Math.Sqrt(2.0 * Math.Log(Visits) / node.Visits);
            }
        }
    }
}

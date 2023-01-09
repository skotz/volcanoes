using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Volcano.Game;
using Volcano.Neural;

namespace Volcano.Engine
{
    internal class NeuralNetworkEngine : IEngine, IStatus, ITrainable
    {
        private Random random;
        private int simulationCount;
        private int visitedNodes;
        private bool _allowForcedWins;
        private bool _allowHash;
        private bool _allowFastWinSearch;

        private int bufferMilliseconds = 200;
        private EngineCancellationToken cancel;

        private Stopwatch timer;

        private Stopwatch statusUpdate;
        private int millisecondsBetweenUpdates = 500;

        public event EventHandler<EngineStatus> OnStatus;

        public event EventHandler<TrainStatus> OnTrainStatus;

        public ConcurrentDictionary<long, Player> winHashes = new ConcurrentDictionary<long, Player>();

        private double _ucbFactor = 2.0;

        private NeuralNetwork _nn;
        private bool _train = true;

        private List<ISample> _samples;

        private static Semaphore _nnFileSemaphore = new Semaphore(1, 1);

        public NeuralNetworkEngine()
        {
            random = new Random();
            _allowForcedWins = true;

            _samples = new List<ISample>();

            LoadNeuralNetwork();
        }

        public void LoadNeuralNetwork()
        {
            _nnFileSemaphore.WaitOne();

            if (_nn == null)
            {
                ForceLoad();
            }

            _nnFileSemaphore.Release();
        }

        private void ForceLoad()
        {
            _nn = new NeuralNetwork(new SquaredErrorLoss(), 0.0005);
            _nn.Add(new FullyConnectedLayer(80, 320, new LeakyReLuActivation()));
            _nn.Add(new FullyConnectedLayer(320, 160, new LeakyReLuActivation()));
            _nn.Add(new FullyConnectedLayer(160, 160, new LeakyReLuActivation()));
            _nn.Add(new FullyConnectedLayer(160, 80, new LeakyReLuActivation()));

            if (File.Exists("nn.dat"))
            {
                using (var save = new FileStream("nn.dat", FileMode.Open))
                {
                    _nn.Load(save);
                }
            }
        }

        public void SaveSamples()
        {
            if (_samples.Count == 0)
            {
                return;
            }

            _nnFileSemaphore.WaitOne();

            var now = DateTime.Now.ToString("yyyyMMddHHmmss");

            if (File.Exists("training-data.json"))
            {
                var rev = 0;
                var filename = $"training-data.{now}.{rev}.bak";
                while (File.Exists(filename))
                {
                    rev++;
                    filename = $"training-data.{now}.{rev}.bak";
                }
                File.Copy("training-data.json", filename);
                var lines = File.ReadLines("training-data.json");
                foreach (var line in lines)
                {
                    var samples = JsonConvert.DeserializeObject<List<BoardSample>>(line);
                    _samples.AddRange(samples);
                }
            }
            File.WriteAllText("training-data.json", JsonConvert.SerializeObject(_samples));

            _nnFileSemaphore.Release();
        }

        public void Train()
        {
            _nnFileSemaphore.WaitOne();

            var now = DateTime.Now.ToString("yyyyMMddHHmmss");

            if (File.Exists("nn.dat"))
            {
                File.Copy("nn.dat", $"nn.{now}.bak");
            }

            if (!File.Exists("training-data.json"))
            {
                return;
            }

            var lines = File.ReadLines("training-data.json");
            foreach (var line in lines)
            {
                var samples = JsonConvert.DeserializeObject<List<BoardSample>>(line);
                _samples.AddRange(samples);
            }

            var loss = 1.0;
            using (var w = new StreamWriter($"training.{now}.csv"))
            {
                w.WriteLine("iteration,trainLoss,testLoss,testRate");

                for (int i = 0; i < 1000 && loss > 0.05; i++)
                {
                    _samples.Shuffle();
                    var subset = _samples.Take(100).ToList();

                    loss = _nn.Train(subset);

                    var test = _nn.TestLoss(_samples);
                    var rate = _nn.TestRate<BoardSample>(_samples, (c, n) => c.OutputToIndex() == n.OutputToIndex());

                    OnTrainStatus?.Invoke(this, new TrainStatus()
                    {
                        Status = $"Iteration {i} total loss = {test}"
                    });

                    w.WriteLine($"{i},{loss},{test},{rate}");

                    using (var save = new FileStream("nn.dat", FileMode.Create))
                    {
                        _nn.Save(save);
                    }
                }
            }

            _nnFileSemaphore.Release();
        }

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            timer = Stopwatch.StartNew();
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
            var rootNode = new MonteCarloTreeSearchNode(rootState, GetMoves, _nn);
            var forceWin = false;

            while (!cancel.Cancelled && !forceWin)
            {
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
                    node = node.SelectChild(_ucbFactor);
                    state.MakeMove(node.Move);
                    visitedNodes++;
                }

                // Expand
                if (node.Untried.Count > 0)
                {
                    var move = node.Untried[random.Next(node.Untried.Count)];
                    state.MakeMove(move);
                    node = node.AddChild(state, move, _nn);
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
                        status.Add(child?.Move ?? 80, eval, pv, child.Visits);
                    }
                    status.Sort();
                    OnStatus?.Invoke(this, status);
                    statusUpdate = Stopwatch.StartNew();
                }
            }

            if (_train)
            {
                var scores = new double[80];

                var minVisits = rootNode.Children.Min(x => x.Visits);
                var maxVisits = rootNode.Children.Max(x => x.Visits);
                var diff = (maxVisits - minVisits) * 2;
                if (diff == 0)
                {
                    diff = 1;
                }
                foreach (var child in rootNode.Children)
                {
                    scores[child.Move] = child.Visits / diff - 1;
                }

                if (rootState.Player == Player.One || rootState.Player == Player.Two)
                {
                    _samples.Add(new BoardSample(rootState, scores));
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

            public MonteCarloTreeSearchNode(Board state, Func<Board, List<int>> getMoves, NeuralNetwork nn)
                : this(state, -2, null, getMoves, nn)
            {
            }

            public MonteCarloTreeSearchNode(Board state, int move, MonteCarloTreeSearchNode parent, Func<Board, List<int>> getMoves, NeuralNetwork nn)
            {
                _getMoves = getMoves;

                Move = move;
                Parent = parent;

                var sample = new BoardSample(state);
                var feedForward = nn.FeedForward(sample);
                var scores = (feedForward as BoardSample).OutputToArray();

                var maxPrediction = move >= 0 ? 100 : 0;
                var prediction = move >= 0 ? Math.Max(0, Math.Round((scores[move] + 1) * (maxPrediction / 2), 2)) : 0;

                Children = new List<MonteCarloTreeSearchNode>();
                Wins = prediction;
                Visits = maxPrediction;

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

            public MonteCarloTreeSearchNode SelectChild(double ucbFactor)
            {
                return Children.OrderBy(x => UpperConfidenceBound(ucbFactor, x)).LastOrDefault();
            }

            public MonteCarloTreeSearchNode AddChild(Board state, int move, NeuralNetwork nn)
            {
                var newNode = new MonteCarloTreeSearchNode(state, move, this, _getMoves, nn);
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
        }
    }
}
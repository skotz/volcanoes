using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TorchSharp.Modules;
using Volcano.Game;
using Volcano.Neural;
using static TorchSharp.torch;
using static TorchSharp.torch.optim;
using static TorchSharp.torch.optim.lr_scheduler;

namespace Volcano.Engine
{
    internal class AlphaZeroEngine : IEngine, IStatus, ILearn
    {
        private AlphaZeroConfig _config;
        private Device _device;
        private Tensor _topology;
        private ResNet _model;
        private Adam _optimizer;
        private LRScheduler _scheduler;
        private Encoder _encoder;
        private Volcano.Neural.GameRule _game;
        private AlphaZero _alphaZero;

        public event EventHandler<EngineStatus> OnStatus;

        public event EventHandler<LearnStatus> OnDebug;

        private List<string> _debug = new List<string>();

        public static Action<string> WriteLine;

        private string _savePath = "models/";
        internal bool _policyOnly;

        public AlphaZeroEngine()
            : this(false)
        {
        }

        public AlphaZeroEngine(bool forTraining)
        {
            _config = new AlphaZeroConfig();
            _device = cuda.is_available() ? new Device("cuda") : new Device("cpu");
            _model = new ResNet(_config.NumResBlocks, _config.NumHidden, _device);

            string modelPath = Path.Combine(_savePath, "training-model.dat");
            string optimizerPath = Path.Combine(_savePath, "training-optimizer.dat");

            if (forTraining)
            {
                Directory.CreateDirectory(_savePath);
                if (File.Exists(modelPath))
                {
                    _model.load(modelPath);
                    Debug($"loaded checkpoint model {modelPath}");
                }
            }

            _optimizer = new Adam(_model.parameters(), lr: _config.InitialLearningRate);

            if (forTraining)
            {
                if (File.Exists(optimizerPath))
                {
                    _optimizer.load_state_dict(optimizerPath);
                    Debug($"loaded checkpoint optimizer {optimizerPath}");
                }
            }

            _scheduler = lr_scheduler.ExponentialLR(_optimizer, _config.LearningRateDecay);
            _encoder = new Volcano.Neural.Encoder();
            _game = new Volcano.Neural.GameRule();

            if (!forTraining)
            {
                _alphaZero = new AlphaZero(_model, _optimizer, _scheduler, _encoder, _game, _config);
                _alphaZero.OnStatus += alphaZero_OnStatus;
            }
        }

        private void alphaZero_OnStatus(object sender, EngineStatus e)
        {
            OnStatus?.Invoke(sender, e);
        }

        /// <summary>
        /// Gets a normalized adjacency matrix of the board
        /// </summary>
        /// <returns></returns>
        private Tensor GetAdjacencyMatrix()
        {
            var adjacent = new float[80, 80];
            for (var t = 0; t < 80; t++)
            {
                // maps to itself
                adjacent[t, t] = 0.5f;

                foreach (var a in Constants.AdjacentIndexes[t])
                {
                    // connects to a neighbor
                    adjacent[t, a] = 0.5f / 3.0f;
                }
            }
            return tensor(adjacent);
        }

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            var timer = Stopwatch.StartNew();

            //var canonical = new Canonical();
            //canonical.SetIndex(state);
            //var canonicalized = canonical.Canonicalize(state);

            var move = _alphaZero.GetBestMove(state, maxSeconds, token, _policyOnly);

            return new SearchResult
            {
                BestMove = move.Item1, // canonical.CanonicalToBoard(move.Item1),
                Evaluations = move.Item2, // TODO
                Simulations = move.Item2,
                Milliseconds = timer.ElapsedMilliseconds,
            };
        }

        public void Train()
        {
            WriteLine = Debug;

            var alphaZero = new AlphaZeroParallel(_model, _optimizer, _scheduler, _encoder, _game, _config);

            var watch = Stopwatch.StartNew();
            alphaZero.Learn(_savePath);
            watch.Stop();

            Debug($"Training time: {watch.ElapsedMilliseconds / 1000.0}s");
        }

        private void Debug(string status)
        {
            try
            {
                if (OnDebug == null)
                {
                    _debug.Add(status);
                }
                else
                {
                    if (_debug.Count > 0)
                    {
                        foreach (var s in _debug)
                        {
                            OnDebug.Invoke(this, new LearnStatus(s));
                        }
                        _debug.Clear();
                    }

                    OnDebug.Invoke(this, new LearnStatus(status));
                }
            }
            catch
            {
                Console.WriteLine(status);
            }
        }
    }
}
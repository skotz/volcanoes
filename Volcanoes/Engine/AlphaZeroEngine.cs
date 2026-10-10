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
    internal enum VolcanoZeroVersion
    {
        /// <summary>
        /// 6r 128f 102400g
        /// </summary>
        V1,

        /// <summary>
        /// 6r 128f 233472g
        /// </summary>
        V2,

        /// <summary>
        /// 10r 192f 204800g
        /// </summary>
        V3,
    }

    internal class VolcanoZeroConfig
    {
        public string ModelPath { get; private set; }

        public int ResidualBlocks { get; private set; }

        public int ResidualFeatures { get; private set; }

        public VolcanoZeroConfig(string modelPath, int residualBlocks, int residualFeatures)
        {
            ModelPath = modelPath;
            ResidualBlocks = residualBlocks;
            ResidualFeatures = residualFeatures;
        }

        public static VolcanoZeroConfig FromFile(string modelPath, int residualBlocks, int residualFeatures)
        {
            return new VolcanoZeroConfig(modelPath, residualBlocks, residualFeatures);
        }
    }

    internal class AlphaZeroEngine : IEngine, IStatus, ILearn
    {
        private const string _defaultModel = "Models\\volcanozero-v1-6r-128f-102400g.dat";
        private const int _resBlocks = 10;
        private const int _resFeatures = 192;

        private VolcanoZeroConfig _settings;
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
        public int forcedIterations = -1;

        private OpeningBook _book;

        public AlphaZeroEngine()
            : this(VolcanoZeroConfig.FromFile(_defaultModel, _resBlocks, _resFeatures))
        {
        }

        public AlphaZeroEngine(VolcanoZeroVersion version)
            : this(GetConfig(version), false)
        {
        }

        public AlphaZeroEngine(VolcanoZeroVersion version, string book)
            : this(GetConfig(version), false)
        {
            _book = new OpeningBook(book);
        }

        public AlphaZeroEngine(VolcanoZeroVersion version, OpeningBook book)
            : this(GetConfig(version), false)
        {
            _book = book;
        }

        public AlphaZeroEngine(VolcanoZeroConfig config)
            : this(config, false)
        {
        }

        public AlphaZeroEngine(bool forTraining)
            : this(VolcanoZeroConfig.FromFile(_defaultModel, _resBlocks, _resFeatures), forTraining)
        {
        }

        private static VolcanoZeroConfig GetConfig(VolcanoZeroVersion version)
        {
            switch (version)
            {
                case VolcanoZeroVersion.V1:
                    return VolcanoZeroConfig.FromFile("models\\volcanozero-v1-6r-128f-102400g.dat", 6, 128);

                case VolcanoZeroVersion.V2:
                    return VolcanoZeroConfig.FromFile("models\\volcanozero-v1-6r-128f-233472g.dat", 6, 128);

                case VolcanoZeroVersion.V3:
                    return VolcanoZeroConfig.FromFile("models\\volcanozero-v1-10r-192f-204800g.dat", 10, 192);

                default:
                    throw new ArgumentException("Invalid VolcanoZero Version");
            }
        }

        public AlphaZeroEngine(VolcanoZeroConfig config, bool forTraining)
        {
            _settings = config;
            _config = new AlphaZeroConfig();
            _device = cuda.is_available() ? new Device("cuda") : new Device("cpu");
            _model = new ResNet(_settings.ResidualBlocks, _settings.ResidualFeatures, _device);

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
                _alphaZero = new AlphaZero(_model, _optimizer, _scheduler, _encoder, _game, _config, _settings);
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
            if (_book != null)
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

            var timer = Stopwatch.StartNew();

            //var canonical = new Canonical();
            //canonical.SetIndex(state);
            //var canonicalized = canonical.Canonicalize(state);

            var move = _alphaZero.GetBestMove(state, maxSeconds, forcedIterations, token, _policyOnly);

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

            Debug($"Model: {_settings.ResidualBlocks} blocks, {_settings.ResidualFeatures} features");

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
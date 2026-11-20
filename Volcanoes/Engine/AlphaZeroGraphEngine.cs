using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TorchSharp.Modules;
using Volcano.Game;
using Volcano.Neural;
using static TorchSharp.torch;
using static TorchSharp.torch.optim;
using static TorchSharp.torch.optim.lr_scheduler;

namespace Volcano.Engine
{
    internal enum VolcanoGraphZeroVersion
    {
        /// <summary>
        /// 12 GCNII layers 128 neurons
        /// </summary>
        V1,
    }

    internal class VolcanoGraphZeroConfig
    {
        public string ModelPath { get; private set; }

        public int Layers { get; private set; }

        public int Width { get; private set; }

        public IBoardEncoder BoardEncoder { get; private set; }

        public VolcanoGraphZeroConfig(string modelPath, int layers, int width, IBoardEncoder encoder = null)
        {
            ModelPath = modelPath;
            Layers = layers;
            Width = width;
            BoardEncoder = encoder ?? new GraphEncoder();
        }

        public static VolcanoGraphZeroConfig FromFile(string modelPath, int layers, int width, IBoardEncoder encoder = null)
        {
            return new VolcanoGraphZeroConfig(modelPath, layers, width, encoder);
        }
    }

    internal class AlphaZeroGraphEngine : IEngine, IStatus, ILearn
    {
        private VolcanoGraphZeroConfig _settings;
        private AlphaZeroConfig _config;
        private Device _device;
        private GraphResNet _model;
        private Adam _optimizer;
        private LRScheduler _scheduler;
        private IBoardEncoder _encoder;
        private GameRule _game;
        private AlphaZero _alphaZero;

        public event EventHandler<EngineStatus> OnStatus;

        public event EventHandler<LearnStatus> OnDebug;

        private List<string> _debug = new List<string>();

        private static readonly string _modelDirectory = Path.Combine(AppContext.BaseDirectory, "Models");
        private string _savePath = Path.Combine(AppContext.BaseDirectory, "models", "graph");
        internal bool _policyOnly;
        public int forcedIterations = -1;
        public bool forceFirstMove = true;

        public AlphaZeroGraphEngine(VolcanoGraphZeroVersion version)
            : this(GetConfig(version), false)
        {
        }

        public AlphaZeroGraphEngine(VolcanoGraphZeroConfig config)
            : this(config, false)
        {
        }

        public AlphaZeroGraphEngine(bool forTraining)
            : this(VolcanoGraphZeroConfig.FromFile("x", 12, 128, new PathGraphEncoder()), forTraining)
        {
        }

        private static VolcanoGraphZeroConfig GetConfig(VolcanoGraphZeroVersion version)
        {
            switch (version)
            {
                case VolcanoGraphZeroVersion.V1:
                    return VolcanoGraphZeroConfig.FromFile(Path.Combine(_modelDirectory, "volcanozero-gcnii-v1-12r-128w-204800g.dat"), 12, 128, new GraphEncoder());

                default:
                    throw new ArgumentException("Invalid VolcanoGraphZero Version");
            }
        }

        public AlphaZeroGraphEngine(VolcanoGraphZeroConfig config, bool forTraining)
        {
            _settings = config;
            _config = new AlphaZeroConfig();
            _device = cuda.is_available() ? new Device("cuda") : new Device("cpu");
            _model = new GraphResNet(_settings.Layers, _settings.Width, _settings.BoardEncoder.InputChannels, _device);

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
            _encoder = _settings.BoardEncoder;
            _game = new GameRule();

            if (!forTraining)
            {
                _alphaZero = new AlphaZero(_model, _device, _optimizer, _scheduler, _encoder, _game, _config, _settings.ModelPath);
                _alphaZero.OnStatus += alphaZero_OnStatus;
            }
        }

        private void alphaZero_OnStatus(object sender, EngineStatus e)
        {
            OnStatus?.Invoke(sender, e);
        }

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            var timer = Stopwatch.StartNew();

            if (forceFirstMove && state.Turn == 1 && state.Tiles.All(t => t == 0))
            {
                return new SearchResult
                {
                    BestMove = Constants.EquilateralIndexes[Random.Shared.Next(20)],
                    Milliseconds = timer.ElapsedMilliseconds,
                };
            }

            var move = _alphaZero.GetBestMove(state, maxSeconds, forcedIterations, token, _policyOnly);

            return new SearchResult
            {
                BestMove = move.Item1,
                Evaluations = move.Item2,
                Simulations = move.Item2,
                Milliseconds = timer.ElapsedMilliseconds,
            };
        }

        public void Train()
        {
            AlphaZeroLog.WriteLine = Debug;

            var alphaZero = new AlphaZeroParallel(_model, _device, _optimizer, _scheduler, _encoder, _game, _config);

            Debug($"Model: {_settings.Layers} layers, {_settings.Width} width");

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

        public AlphaZeroEval Evaluate(Board state)
        {
            return _alphaZero.Evaluate(state);
        }
    }
}
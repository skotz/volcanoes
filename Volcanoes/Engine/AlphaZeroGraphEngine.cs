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

        public VolcanoGraphZeroConfig(string modelPath, int layers, int width)
        {
            ModelPath = modelPath;
            Layers = layers;
            Width = width;
        }

        public static VolcanoGraphZeroConfig FromFile(string modelPath, int layers, int width)
        {
            return new VolcanoGraphZeroConfig(modelPath, layers, width);
        }
    }

    internal class AlphaZeroGraphEngine : IEngine, IStatus, ILearn
    {
        private const string _defaultModel = "x";
        private const int _layers = 12;
        private const int _width = 128;

        private VolcanoGraphZeroConfig _settings;
        private AlphaZeroConfig _config;
        private Device _device;
        private GraphResNet _model;
        private Adam _optimizer;
        private LRScheduler _scheduler;
        private GraphEncoder _encoder;
        private GameRule _game;
        private AlphaZero _alphaZero;

        public event EventHandler<EngineStatus> OnStatus;

        public event EventHandler<LearnStatus> OnDebug;

        private List<string> _debug = new List<string>();

        private string _savePath = "models/graph/";
        internal bool _policyOnly;
        public int forcedIterations = -1;

        public AlphaZeroGraphEngine()
            : this(VolcanoGraphZeroConfig.FromFile(_defaultModel, _layers, _width))
        {
        }

        public AlphaZeroGraphEngine(VolcanoGraphZeroVersion version)
            : this(GetConfig(version), false)
        {
        }

        public AlphaZeroGraphEngine(VolcanoGraphZeroConfig config)
            : this(config, false)
        {
        }

        public AlphaZeroGraphEngine(bool forTraining)
            : this(VolcanoGraphZeroConfig.FromFile(_defaultModel, _layers, _width), forTraining)
        {
        }

        private static VolcanoGraphZeroConfig GetConfig(VolcanoGraphZeroVersion version)
        {
            switch (version)
            {
                case VolcanoGraphZeroVersion.V1:
                    return VolcanoGraphZeroConfig.FromFile("models\\graph\\volcanograph-v1-12l-128w.dat", 12, 128);

                default:
                    throw new ArgumentException("Invalid VolcanoGraphZero Version");
            }
        }

        public AlphaZeroGraphEngine(VolcanoGraphZeroConfig config, bool forTraining)
        {
            _settings = config;
            _config = new AlphaZeroConfig();
            _device = cuda.is_available() ? new Device("cuda") : new Device("cpu");
            _model = new GraphResNet(_settings.Layers, _settings.Width, _device);

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
            _encoder = new GraphEncoder();
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

using System;
using System.Diagnostics;
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
        private Volcano.Neural.Game _game;

        public event EventHandler<EngineStatus> OnStatus;

        public event EventHandler<LearnStatus> OnDebug;

        public static Action<string> WriteLine;

        public AlphaZeroEngine()
        {
            _config = new AlphaZeroConfig();
            _device = cuda.is_available() ? new Device("cuda") : new Device("cpu");
            _topology = GetGraphTopology();
            _model = new ResNet(_config.NumResBlocks, _config.NumHidden, _device, _topology);
            _optimizer = new Adam(_model.parameters(), lr: _config.InitialLearningRate);
            _scheduler = lr_scheduler.ExponentialLR(_optimizer, _config.LearningRateDecay);
            _encoder = new Volcano.Neural.Encoder();
            _game = new Volcano.Neural.Game();

            WriteLine = Debug;
        }

        private Tensor GetGraphTopology()
        {
            var adjacent = new float[80, 80];
            for (var t = 0; t < 80; t++)
            {
                foreach (var a in Constants.AdjacentIndexes[t])
                {
                    adjacent[t, a] = 1.0f;
                }
            }
            return tensor(adjacent);
        }

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            throw new NotImplementedException();
        }

        public void Train()
        {
            var alphaZero = new AlphaZeroParallel(_model, _optimizer, _scheduler, _encoder, _game, _config);

            var watch = Stopwatch.StartNew();
            alphaZero.Learn("models/");
            watch.Stop();

            Debug($"Training time: {watch.ElapsedMilliseconds / 1000.0}s");
        }

        private void Debug(string status)
        {
            OnDebug?.Invoke(this, new LearnStatus(status));
        }

        private void Report(EngineStatus status)
        {
            OnStatus?.Invoke(this, status);
        }
    }
}
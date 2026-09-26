using System;
using TorchSharp.Modules;
using Volcano.Game;
using Volcano.Neural;
using static TorchSharp.torch;
using static TorchSharp.torch.optim;

namespace Volcano.Engine
{
    internal class AlphaZeroEngine : IEngine, IStatus, ILearn
    {
        public event EventHandler<EngineStatus> OnStatus;

        public event EventHandler<LearnStatus> OnDebug;

        public AlphaZeroEngine()
        {
            var device = cuda.is_available() ? new Device("cuda") : new Device("cpu");
            var topology = GetGraphTopology();
            var model = new ResNet(6, 128, device, topology);
            var optimizer = new Adam(model.parameters(), lr: 0.001);
            var scheduler = lr_scheduler.ExponentialLR(optimizer, gamma: 0.95);
            var encoder = new Volcano.Neural.Encoder();
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
            throw new NotImplementedException();
        }
    }
}
using System;
using TorchSharp;
using TorchSharp.Modules;
using Volcano.Engine;
using Volcano.Game;

namespace Volcano.Neural
{
    internal class AlphaZero : AlphaZeroBase
    {
        private readonly NNMCTS nnmcts;

        public event EventHandler<EngineStatus> OnStatus;

        public AlphaZero(torch.nn.Module<torch.Tensor, Tuple<torch.Tensor, torch.Tensor>> model, torch.Device device, Adam optimizer, torch.optim.lr_scheduler.LRScheduler scheduler, IBoardEncoder encoder, GameRule game, AlphaZeroConfig config, string modelPath)
            : base(model, device, optimizer, scheduler, encoder, game, config)
        {
            nnmcts = new NNMCTS(game, config, model, encoder, device);

            // "C:\\Users\\Scott\\Documents\\GitHub\\volcanoes\\Volcanoes\\bin\\Release\\net10.0-windows\\models\\training-model.dat"
            if (modelPath != null)
            {
                model.load(modelPath);
            }
            model.eval();

            nnmcts.OnStatus += Nnmcts_OnStatus;
        }

        private void Nnmcts_OnStatus(object sender, EngineStatus e)
        {
            OnStatus?.Invoke(sender, e);
        }

        protected override int SelfPlayCallsPerIteration => config.NumSelfPlayIterations;

        protected override IEngine CreateBenchmarkEngine(int forcedIterations)
        {
            return new ModelBenchmarkEngine(new AlphaZero(model, device, optimizer, scheduler, encoder, game, config, null), forcedIterations);
        }

        protected override TrainingData SelfPlay()
        {
            return null;
        }

        internal (int, int) GetBestMove(Board state, int maxSeconds, int forcedIterations, EngineCancellationToken token, bool policyOnly)
        {
            var neutral = game.ChangePerspective(state, Player.One);
            neutral.fastWinSearch = true;

            var probs = nnmcts.Search(neutral, maxSeconds, forcedIterations, policyOnly, token);

            var action = 0;
            for (var i = 1; i < probs.Length; i++)
            {
                if (probs[i] > probs[action])
                {
                    action = i;
                }
            }

            return (action, nnmcts.simulations);
        }

        public AlphaZeroEval Evaluate(Board state)
        {
            var neutral = game.ChangePerspective(state, Player.One);
            return nnmcts.EvaluateState(neutral);
        }
    }
}
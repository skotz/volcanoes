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

        public AlphaZero(ResNet model, Adam optimizer, torch.optim.lr_scheduler.LRScheduler scheduler, Encoder encoder, GameRule game, AlphaZeroConfig config)
            : base(model, optimizer, scheduler, encoder, game, config)
        {
            nnmcts = new NNMCTS(game, config, model, encoder);

            model.load("C:\\Users\\Scott\\Documents\\GitHub\\volcanoes\\Volcanoes\\bin\\Debug\\net10.0-windows\\models\\model-102400-20260927045000.dat");
            model.eval();

            nnmcts.OnStatus += Nnmcts_OnStatus;
        }

        private void Nnmcts_OnStatus(object sender, EngineStatus e)
        {
            OnStatus?.Invoke(sender, e);
        }

        protected override int SelfPlayCallsPerIteration => config.NumSelfPlayIterations;

        protected override TrainingData SelfPlay()
        {
            return null;
        }

        internal (int, int) GetBestMove(Board state, int maxSeconds)
        {
            var neutral = game.ChangePerspective(state, Player.One);
            var probs = nnmcts.Search(neutral, maxSeconds);

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
    }
}
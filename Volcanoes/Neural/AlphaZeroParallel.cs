using System;
using System.Collections.Generic;
using System.Linq;
using TorchSharp;
using TorchSharp.Modules;
using Volcano.Game;

namespace Volcano.Neural
{
    internal class AlphaZeroParallel : AlphaZeroBase
    {
        private readonly NNMCTSParallel nnmcts;

        public AlphaZeroParallel(ResNet model, Adam optimizer, torch.optim.lr_scheduler.LRScheduler scheduler, Encoder encoder, Game game, AlphaZeroConfig config)
            : base(model, optimizer, scheduler, encoder, game, config)
        {
            this.nnmcts = new NNMCTSParallel(game, config, model, encoder);
        }

        protected override int SelfPlayCallsPerIteration => Math.Max(1, config.NumSelfPlayIterations / config.NumParallelGames);

        protected override TrainingData SelfPlay()
        {
            TrainingData data = new TrainingData();

            SPG[] spgs = new SPG[config.NumParallelGames];
            for (int i = 0; i < spgs.Length; i++)
            {
                spgs[i] = new SPG(game);
            }

            int player = 1;
            int moveCount = 0;

            while (true)
            {
                // Only search games that are still running — as games finish, the batch shrinks.
                SPG[] active = spgs.Where(s => !s.Terminated).ToArray();
                if (active.Length == 0) break;

                Board[] neutralStates = active.Select(s => game.ChangePerspective(s.State, player)).ToArray();
                nnmcts.Search(neutralStates, active);

                double temperature = TemperatureForMove(moveCount);

                foreach (SPG spg in active)
                {
                    double[] actionProbs = GetActionProbs(spg.Root!);
                    spg.AddEntry(spg.Root!.State, actionProbs, player);

                    double[] temperatureProbs = ApplyTemperature(actionProbs, temperature);
                    int action = Sampling.SampleFromWeights(Random.Shared, temperatureProbs);

                    spg.State = game.GetNextState(spg.State, action, player);

                    if (game.GetTerminated(spg.State, action, out int outcome))
                    {
                        spg.Terminated = true;

                        IReadOnlyList<Board> gameStates = spg.GetStates();
                        IReadOnlyList<double[]> gameProbs = spg.GetProbs();
                        IReadOnlyList<int> gamePlayers = spg.GetPlayers();

                        for (int j = 0; j < gamePlayers.Count; j++)
                        {
                            float value = gamePlayers[j] == player ? outcome : -outcome;
                            data.Add(gameStates[j], gameProbs[j], value);
                        }
                    }
                }
                player = game.GetOpponent(player);
                moveCount++;
            }
            return data;
        }

        public bool AllGameTerminated(SPG[] spgs) => spgs.All(s => s.Terminated);

        public int CountNotTerminatedGames(SPG[] spgs) => spgs.Count(s => !s.Terminated);
    }
}
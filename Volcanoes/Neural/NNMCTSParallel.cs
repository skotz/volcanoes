using System;
using System.Collections.Generic;
using TorchSharp;
using Volcano.Game;

namespace Volcano.Neural
{
    internal class NNMCTSParallel
    {
        private readonly ResNet model;
        private readonly GameRule game;
        private readonly AlphaZeroConfig config;
        private readonly Encoder encoder;

        public NNMCTSParallel(GameRule game, AlphaZeroConfig config, ResNet model, Encoder encoder)
        {
            this.game = game;
            this.config = config;
            this.model = model;
            this.encoder = encoder;
        }

        /// <summary>
        /// The single tensor boundary: encode a batch of positions, run the network, and bring
        /// the results back as plain arrays in one transfer.
        /// </summary>
        private (float[][] policies, float[] values) Evaluate(IReadOnlyList<Board> states)
        {
            using var scope = torch.NewDisposeScope();
            using (torch.no_grad())
            {
                var output = model.forward(encoder.Encode(states, model.Device));
                float[] flatPolicy = torch.softmax(output.Item1, 1).cpu().data<float>().ToArray();
                float[] flatValue = output.Item2.cpu().data<float>().ToArray();

                int n = states.Count;
                int actions = game.ActionSize;
                float[][] policies = new float[n][];
                for (int i = 0; i < n; i++)
                {
                    policies[i] = new float[actions];
                    Array.Copy(flatPolicy, i * actions, policies[i], 0, actions);
                }
                return (policies, flatValue);
            }
        }

        public void Search(IReadOnlyList<Board> neutralStates, SPG[] spGames)
        {
            if (neutralStates.Count != spGames.Length)
            {
                throw new ArgumentException(
                    $"Expected one position per game, got {neutralStates.Count} for {spGames.Length} games.");
            }

            // --- Root: one batched evaluation for every game ---
            (float[][] rootPolicies, _) = Evaluate(neutralStates);

            for (int i = 0; i < spGames.Length; i++)
            {
                float[] policy = rootPolicies[i];
                if (config.DirichletEpsilon > 0)
                {
                    float[] noise = Sampling.Dirichlet(Random.Shared, game.ActionSize, config.DirichletAlpha);
                    PolicyMath.AddDirichletNoise(policy, noise, config.DirichletEpsilon);
                }
                PolicyMath.MaskAndNormalize(policy, game.GetValidMoves(neutralStates[i]));

                spGames[i].Root = new NNNode(game, config, neutralStates[i], visitCount: 1);
                spGames[i].Root!.Expand(policy);
            }

            // --- Simulations ---
            for (int search = 0; search < config.NumSearches; search++)
            {
                // Walk every game's tree down to a leaf.
                for (int j = 0; j < spGames.Length; j++)
                {
                    spGames[j].Node = null;
                    NNNode node = spGames[j].Root!;
                    while (node.IsFullyExpanded())
                    {
                        node = node.Select();
                    }

                    bool terminated = game.GetTerminated(node.State, node.ActionTaken, out var winner);
                    if (terminated)
                    {
                        var absoluteWinner = node.State.GetAbsoluteWinner();
                        var discount = node.State.GetAbsoluteTurn() * 0.001f;
                        var value = absoluteWinner == node.AbsolutePlayer ? 1 - discount : (absoluteWinner == Player.Draw ? 0 : -1 + discount);

                        node.Backpropagate(value);
                    }
                    else
                    {
                        spGames[j].Node = node; // needs a network evaluation
                    }
                }

                // Evaluate all pending leaves in one batch.
                List<int> pending = new();
                for (int j = 0; j < spGames.Length; j++)
                {
                    if (spGames[j].Node != null) pending.Add(j);
                }
                if (pending.Count == 0) continue;

                List<Board> leafStates = new(pending.Count);
                foreach (int j in pending) leafStates.Add(spGames[j].Node!.State);

                (float[][] policies, float[] values) = Evaluate(leafStates);

                for (int k = 0; k < pending.Count; k++)
                {
                    NNNode node = spGames[pending[k]].Node!;
                    float[] policy = policies[k];
                    PolicyMath.MaskAndNormalize(policy, game.GetValidMoves(node.State));
                    node.Expand(policy);

                    var absolutePlayer = node.State.GetAbsolutePlayer();
                    var value = absolutePlayer == node.AbsolutePlayer ? values[k] : -values[k];

                    node.Backpropagate(value);
                }
            }
        }
    }
}
using System;
using System.Diagnostics;
using System.Linq;
using TorchSharp;
using Volcano.Engine;
using Volcano.Game;

namespace Volcano.Neural
{
    internal class NNMCTS
    {
        private readonly ResNet model;
        private readonly GameRule game;
        private readonly AlphaZeroConfig config;
        private readonly Encoder encoder;

        public int simulations;

        public event EventHandler<EngineStatus> OnStatus;

        public NNMCTS(GameRule game, AlphaZeroConfig config, ResNet model, Encoder encoder)
        {
            this.game = game;
            this.config = config;
            this.model = model;
            this.encoder = encoder;
        }

        /// <summary>One network call for one position. Returns the softmaxed priors and the value.</summary>
        private (float[] policy, float value) Evaluate(Board state)
        {
            using var scope = torch.NewDisposeScope();
            using (torch.no_grad())
            {
                var output = model.forward(encoder.Encode(state, model.Device));
                float[] policy = torch.softmax(output.Item1, 1).cpu().data<float>().ToArray();
                float value = output.Item2.cpu().data<float>()[0];
                //policy = new float[80]; // TEMP: TEST
                //value = 0; // TEMP: TEST
                return (policy, value);
            }
        }

        public double[] Search(Board state, int seconds, EngineCancellationToken token)
        {
            // visitCount starts at 1 so the sqrt(parent visits) term in PUCT is non-zero on the
            // first simulation; with 0 every child scores exactly 0 and the priors are ignored.
            // This matches NNMCTSParallel, which is the path the network was trained with.
            NNNode root = new NNNode(game, config, state, visitCount: 1);

            (float[] rootPolicy, _) = Evaluate(state);
            //if (config.DirichletEpsilon > 0)
            //{
            //    float[] noise = Sampling.Dirichlet(Random.Shared, game.ActionSize, config.DirichletAlpha);
            //    PolicyMath.AddDirichletNoise(rootPolicy, noise, config.DirichletEpsilon);
            //}
            PolicyMath.MaskAndNormalize(rootPolicy, game.GetValidMoves(state));
            root.Expand(rootPolicy);

            var stopwatch = Stopwatch.StartNew();
            var buffer = 200;

            var statusUpdate = Stopwatch.StartNew();
            var millisecondsBetweenUpdates = 500;

            simulations = 0;

            //for (int i = 0; i < config.NumSearches; i++)
            while (stopwatch.ElapsedMilliseconds <= seconds * 1000 - buffer && !token.Cancelled)
            {
                simulations++;
                NNNode node = root;
                while (node.IsFullyExpanded())
                {
                    node = node.Select();
                }

                // this is calculated AFTER the move is made on the state
                bool terminated = game.GetTerminated(node.State, node.ActionTaken, out _);

                if (!terminated)
                {
                    (float[] policy, float leafValue) = Evaluate(node.State);
                    PolicyMath.MaskAndNormalize(policy, game.GetValidMoves(node.State));
                    node.Expand(policy);

                    var absolutePlayer = node.State.GetAbsolutePlayer();
                    var value = absolutePlayer == node.AbsolutePlayer ? leafValue : -leafValue;

                    node.Backpropagate(value, node.State.GetMoveNumber() == 1 ? 2 : 1);
                }
                else
                {
                    var absoluteWinner = node.State.GetAbsoluteWinner();
                    var discount = node.State.GetAbsoluteTurn() * 0.001f;
                    var value = absoluteWinner == node.AbsolutePlayer ? 1 - discount : (absoluteWinner == Player.Draw ? 0 : -1 + discount);

                    node.Backpropagate(value, node.State.GetMoveNumber() == 1 ? 2 : 1);
                }

                // Update Status
                if (statusUpdate.ElapsedMilliseconds > millisecondsBetweenUpdates && OnStatus != null)
                {
                    EngineStatus status = new EngineStatus();
                    foreach (var child in root.Children)
                    {
                        //double eval = child.VisitCount;
                        // TODO: why negated?
                        double eval = Math.Round(100 * (child.VisitCount > 0 ? child.valueSum / child.VisitCount : 0), 2);
                        string pv = $"[{rootPolicy[child.ActionTaken].ToString("0.000000")}]   ";
                        var c = child;
                        while (c != null && c.ActionTaken >= 0 && c.ActionTaken <= 80)
                        {
                            pv += Constants.TileNames[c.ActionTaken] + " (" + c.VisitCount + ")   ";
                            c = c.Children?.OrderBy(x => x.VisitCount)?.LastOrDefault();
                        }
                        status.Add(child?.ActionTaken ?? 80, eval, pv, child.VisitCount);
                    }
                    status.Sort();
                    OnStatus?.Invoke(this, status);
                    statusUpdate = Stopwatch.StartNew();
                }
            }

            double[] actionProbs = new double[game.ActionSize];
            double total = 0;
            foreach (NNNode child in root.Children)
            {
                actionProbs[child.ActionTaken] = child.VisitCount;
                total += child.VisitCount;
            }
            if (total > 0)
            {
                for (int i = 0; i < actionProbs.Length; i++) actionProbs[i] /= total;
            }
            return actionProbs;
        }
    }
}
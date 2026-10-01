using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TorchSharp;
using TorchSharp.Modules;
using Volcano.Engine;
using Volcano.Game;

namespace Volcano.Neural
{
    internal abstract class AlphaZeroBase
    {
        protected readonly ResNet model;
        protected readonly Adam optimizer;
        protected readonly torch.optim.lr_scheduler.LRScheduler scheduler;
        protected readonly GameRule game;
        protected readonly AlphaZeroConfig config;
        protected readonly Encoder encoder;

        protected AlphaZeroBase(ResNet model, Adam optimizer, torch.optim.lr_scheduler.LRScheduler scheduler, Encoder encoder, GameRule game, AlphaZeroConfig config)
        {
            this.model = model;
            this.optimizer = optimizer;
            this.scheduler = scheduler;
            this.encoder = encoder;
            this.game = game;
            this.config = config;
        }

        /// <summary>Plays out games and returns the positions gathered.</summary>
        protected abstract TrainingData SelfPlay();

        /// <summary>How many times <see cref="SelfPlay"/> is called per training iteration.</summary>
        protected abstract int SelfPlayCallsPerIteration { get; }

        /// <summary>
        /// Reweights visit counts before sampling a self-play move. A temperature of 1 samples in
        /// proportion to visits; near zero it becomes "always play the best move".
        /// </summary>
        protected static double[] ApplyTemperature(double[] probs, double temperature)
        {
            // Treat a near-zero temperature as an explicit argmax. Computing it as pow(p, 1/T)
            // with T = 0.01 means raising to the hundredth power, which only avoids underflowing
            // to all-zeros by luck.
            if (temperature <= 0.02)
            {
                double[] greedy = new double[probs.Length];
                int best = 0;
                for (int i = 1; i < probs.Length; i++)
                {
                    if (probs[i] > probs[best]) best = i;
                }
                greedy[best] = 1;
                return greedy;
            }

            double power = 1.0 / temperature;
            double[] result = new double[probs.Length];
            for (int i = 0; i < probs.Length; i++)
            {
                result[i] = Math.Pow(probs[i], power);
            }
            return result;
        }

        /// <summary>Temperature for a given ply: exploratory in the opening, greedy afterwards.</summary>
        protected double TemperatureForMove(int moveCount) =>
            moveCount < config.TemperatureMoves ? config.Temperature : 0.0;

        /// <summary>Visit-count distribution over actions at a search root.</summary>
        protected double[] GetActionProbs(NNNode root)
        {
            double[] probs = new double[game.ActionSize];
            double total = 0;
            foreach (NNNode child in root.Children)
            {
                probs[child.ActionTaken] = child.VisitCount;
                total += child.VisitCount;
            }
            if (total > 0)
            {
                for (int i = 0; i < probs.Length; i++) probs[i] /= total;
            }
            return probs;
        }

        /// <summary>One pass over the data: shuffle, then fit policy (cross-entropy) and value (MSE).</summary>
        public (double, double, double) Train(TrainingData data)
        {
            int n = data.Count;
            if (n == 0) return (0, 0, 0);

            int[] order = Enumerable.Range(0, n).ToArray();
            for (int i = n - 1; i > 0; i--)      // Fisher-Yates shuffle
            {
                int j = Random.Shared.Next(i + 1);
                (order[i], order[j]) = (order[j], order[i]);
            }

            int actions = game.ActionSize;
            float lastPolicyLoss = 0, lastValueLoss = 0, lastLoss = 0;

            for (int start = 0; start < n; start += config.BatchSize)
            {
                int size = Math.Min(config.BatchSize, n - start);

                Board[] states = new Board[size];
                float[] flatPolicy = new float[size * actions];
                float[] values = new float[size];

                for (int k = 0; k < size; k++)
                {
                    int idx = order[start + k];
                    states[k] = data.States[idx];
                    double[] p = data.Probs[idx];
                    for (int a = 0; a < actions; a++)
                    {
                        flatPolicy[k * actions + a] = (float)p[a];
                    }
                    values[k] = data.Values[idx];
                }

                using var scope = torch.NewDisposeScope();

                torch.Tensor input = encoder.Encode(states, model.Device);
                torch.Tensor policyTarget = torch.tensor(flatPolicy, new long[] { size, actions }).to(model.Device);
                torch.Tensor valueTarget = torch.tensor(values, new long[] { size, 1 }).to(model.Device);

                var output = model.forward(input);
                torch.Tensor policyLoss = torch.nn.functional.cross_entropy(output.Item1, policyTarget);
                torch.Tensor valueLoss = torch.nn.functional.mse_loss(output.Item2, valueTarget);
                torch.Tensor loss = policyLoss + valueLoss;

                optimizer.zero_grad();
                loss.backward();
                optimizer.step();

                lastPolicyLoss = policyLoss.item<float>();
                lastValueLoss = valueLoss.item<float>();
                lastLoss = loss.item<float>();
            }

            AlphaZeroEngine.WriteLine($"Loss policy: {lastPolicyLoss}");
            AlphaZeroEngine.WriteLine($"Loss value: {lastValueLoss}");
            AlphaZeroEngine.WriteLine($"Loss: {lastLoss}");

            return (lastPolicyLoss, lastValueLoss, lastLoss);
        }

        /// <summary>The AlphaZero loop: self-play, train on the result, checkpoint, repeat.</summary>
        public void Learn(string savePath)
        {
            Directory.CreateDirectory(savePath);

            // Replay buffer holding the last N iterations of self-play. Training on only the
            // newest games makes each iteration overfit a small, freshly-generated sample and
            // partially forget the previous one; mixing in recent history keeps the value
            // targets stable and stops rare tactical positions from vanishing after one round.
            // Game outcomes are ground truth, so older entries stay valid targets — only their
            // policy distributions are slightly stale, which is the intended trade.
            Queue<TrainingData> replayBuffer = new();

            string replayBufferPath = Path.Combine(savePath, "training-buffer.dat");
            if (File.Exists(replayBufferPath))
            {
                var replays = File.ReadAllLines(replayBufferPath);
                foreach (var replay in replays)
                {
                    replayBuffer.Enqueue(JsonConvert.DeserializeObject<TrainingData>(replay));
                }
                var count = replayBuffer.Sum(x => x.Count);
                AlphaZeroEngine.WriteLine($"loaded {count} positions into replay buffer from {replays.Length} iteration(s)");
            }

            for (int iteration = 0; iteration < config.NumIterations; iteration++)
            {
                AlphaZeroEngine.WriteLine($"iteration {iteration}");

                TrainingData fresh = new TrainingData();
                model.eval();
                for (int j = 0; j < SelfPlayCallsPerIteration; j++)
                {
                    AlphaZeroEngine.WriteLine($"Self-play {j + 1}");
                    fresh.Add(SelfPlay());
                }

                replayBuffer.Enqueue(fresh);
                while (replayBuffer.Count > config.ReplayBufferIterations)
                {
                    replayBuffer.Dequeue();
                }

                TrainingData data = new TrainingData();
                foreach (TrainingData batch in replayBuffer)
                {
                    data.Add(batch);
                }
                AlphaZeroEngine.WriteLine($"training on {data.Count} positions ({fresh.Count} fresh) from {replayBuffer.Count} iteration(s)");

                model.train();
                var lastLoss = (0.0, 0.0, 0.0);
                for (int epoch = 0; epoch < config.NumEpochs; epoch++)
                {
                    AlphaZeroEngine.WriteLine($"epoch {epoch}");
                    lastLoss = Train(data);
                }

                scheduler.step();
                var lr = scheduler.get_last_lr();
                AlphaZeroEngine.WriteLine($"Current LR: {string.Join(",", lr)}");

                string format = "0.##################################################";

                string status = Path.Combine(savePath, "status.csv");
                if (!File.Exists(status))
                {
                    File.AppendAllLines(status, ["iteration,timestamp,games,policy loss,value loss,loss,lr"]);
                }
                File.AppendAllLines(status, [$"{iteration + 1},{DateTime.Now.ToString("yyyyMMddHHmmss")},{config.NumSelfPlayIterations * (iteration + 1)},{lastLoss.Item1.ToString(format)},{lastLoss.Item2.ToString(format)},{lastLoss.Item3.ToString(format)},{lr.First().ToString(format)}"]);
                string path1 = Path.Combine(savePath, $"model-{config.NumSelfPlayIterations * (iteration + 1)}-{DateTime.Now.ToString("yyyyMMddHHmmss")}.dat");
                model.save(path1);

                string path2 = Path.Combine(savePath, "training-model.dat");
                string path3 = Path.Combine(savePath, "training-optimizer.dat");
                model.save(path2);
                optimizer.save_state_dict(path3);
                File.WriteAllLines(replayBufferPath, replayBuffer.Select(x => JsonConvert.SerializeObject(x)));
            }
        }
    }
}
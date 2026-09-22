using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Volcano.Engine.Neural;
using Volcano.Game;

namespace Volcano.Engine
{
    internal class DeepQNetwork : IEngine, IStatus, ILearn
    {
        public event EventHandler<LearnStatus> OnDebug;

        public event EventHandler<EngineStatus> OnStatus;

        private NeuralNetwork network;
        private ReplayBuffer replayBuffer;
        private Random random;

        private double epsilon;
        private double epsilonStart = 1.0;
        private double epsilonEnd = 0.1;
        private int totalEpisodes = 100;

        private double gamma = 0.99; // discount factor
        private double learningRate = 0.001;
        private int miniBatchSize = 32;

        private int episodeCounter = 0;
        private int trainingGamesMask = 0; // bitmask to track play against opponents
        private const string NETWORK_FILE = "dqn.dat";

        public DeepQNetwork()
        {
            random = new Random();
            network = new NeuralNetwork(learningRate);
            replayBuffer = new ReplayBuffer(5000);

            // Try to load existing network
            if (File.Exists(NETWORK_FILE))
            {
                network.Load(NETWORK_FILE);
            }
        }

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            double[] qValues = network.Forward(EncodeState(state));
            List<int> validMoves = state.GetMoves();

            int bestMove = -1;
            double bestQValue = double.MinValue;

            // Collect move evaluations
            EngineStatus status = new EngineStatus();

            // Epsilon-greedy policy (only during training/inference distinction if needed)
            bool explore = random.NextDouble() < epsilon;

            foreach (int move in validMoves)
            {
                double qValue = qValues[move];
                double visits = Math.Abs(qValue); // pseudo-visit count for display

                status.Add(move, qValue, "", visits);

                if (explore)
                {
                    // Random selection during exploration
                    if (bestMove == -1 || random.NextDouble() < 1.0 / validMoves.Count)
                    {
                        bestMove = move;
                        bestQValue = qValue;
                    }
                }
                else
                {
                    // Greedy selection
                    if (qValue > bestQValue)
                    {
                        bestQValue = qValue;
                        bestMove = move;
                    }
                }
            }

            if (bestMove == -1 && validMoves.Count > 0)
            {
                bestMove = validMoves[random.Next(validMoves.Count)];
            }

            status.Sort();
            Report(status);

            return new SearchResult(bestMove);
        }

        public void Train()
        {
            Debug("Starting Q-Learning training for 100 episodes");

            episodeCounter = 0;
            int totalWins = 0;
            int totalLosses = 0;
            int totalDraws = 0;

            for (int episode = 0; episode < totalEpisodes; episode++)
            {
                episodeCounter = episode;

                // Decay epsilon
                epsilon = epsilonStart - (epsilonStart - epsilonEnd) * (episode / (double)totalEpisodes);

                // Decide if we play against opponent or self/random
                bool playAgainstOpponent = (episode % 10 == 0 && episode > 0);

                VolcanoGame game = new VolcanoGame();
                Player winner = PlayGame(game, playAgainstOpponent);

                // Collect outcome
                if (winner == Player.One)
                {
                    totalWins++;
                }
                else if (winner == Player.Two)
                {
                    totalLosses++;
                }
                else if (winner == Player.Draw)
                {
                    totalDraws++;
                }

                // Train on mini-batches
                if (replayBuffer.Count >= miniBatchSize)
                {
                    TrainOnMiniBatch();
                }

                double winRate = totalWins / (double)(episode + 1);
                Debug($"Episode {episode + 1}/100 | Win Rate: {winRate:F3} | Epsilon: {epsilon:F3} | Opponent: {playAgainstOpponent}");
            }

            // Save network
            network.Save(NETWORK_FILE);
            Debug("Training complete. Network saved to dqn.dat");
        }

        private Player PlayGame(VolcanoGame game, bool playAgainstOpponent)
        {
            IEngine opponent = playAgainstOpponent ? LoadOpponentEngine() : new RandomEngine();

            game.RegisterEngine(Player.One, this, true);
            game.RegisterEngine(Player.Two, opponent, true);
            game.SecondsPerEngineMove = 1;
            game.StartNewGame();

            // Play game to completion
            while (game.CurrentState.Winner == Player.Empty && game.CurrentState.Turn < 500)
            {
                // Store transition before move
                Board stateBefore = new Board(game.CurrentState);
                double[] stateEnc = EncodeState(stateBefore);

                game.ComputerPlay();

                Board stateAfter = new Board(game.CurrentState);
                double[] nextStateEnc = EncodeState(stateAfter);

                // Determine reward and action
                // Note: This is simplified; tracking which move led to which state is complex
                // For now, we reward only at game end
                if (game.CurrentState.Winner != Player.Empty)
                {
                    double reward = 0;
                    if (game.CurrentState.Winner == Player.One)
                    {
                        reward = 1.0;
                    }
                    else if (game.CurrentState.Winner == Player.Two)
                    {
                        reward = -1.0;
                    }

                    // Store final transition (approximation)
                    replayBuffer.Add(new Transition(stateEnc, 0, reward, nextStateEnc, true));
                    break;
                }
            }

            return game.CurrentState.Winner;
        }

        private IEngine LoadOpponentEngine()
        {
            // hardcoded to just mcts for now
            return new MonteCarloTreeSearchEngine();
        }

        private void TrainOnMiniBatch()
        {
            List<Transition> batch = replayBuffer.SampleMiniBatch(miniBatchSize);

            foreach (Transition transition in batch)
            {
                // Compute target Q-value
                double[] nextQValues = network.Forward(transition.NextState);
                double maxNextQ = nextQValues.Max();
                double targetQ = transition.Done ? transition.Reward : transition.Reward + gamma * maxNextQ;

                // Forward pass
                double[] qValues = network.Forward(transition.State);
                double currentQ = qValues[transition.Action];

                // Compute loss gradient (Bellman error)
                double[] lossGradient = new double[80];
                lossGradient[transition.Action] = currentQ - targetQ;

                // Backward pass
                network.Backward(lossGradient, 1.0 / miniBatchSize);
            }
        }

        private double[] EncodeState(Board board)
        {
            double[] encoded = new double[726]; // 80 tiles * 9 + 6 phases

            // Encode tiles: each tile is one-hot encoded into 9 dimensions
            // Values: empty(0), -4, -3, -2, -1, +1, +2, +3, +4
            for (int i = 0; i < 80; i++)
            {
                int tileValue = board.Tiles[i];
                int oneHotIndex = 0;

                if (tileValue == 0)
                    oneHotIndex = 0;
                else if (tileValue < 0)
                    oneHotIndex = 5 + tileValue; // -4 -> 1, -3 -> 2, -2 -> 3, -1 -> 4
                else
                    oneHotIndex = 4 + tileValue; // +1 -> 5, +2 -> 6, +3 -> 7, +4 -> 8

                encoded[i * 9 + oneHotIndex] = 1.0;
            }

            // Encode phase: which of 6 positions in the turn cycle
            int phase = (board.Turn - 1) % 6;
            encoded[720 + phase] = 1.0;

            return encoded;
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
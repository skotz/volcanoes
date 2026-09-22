using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Volcano.Engine.Neural;
using Volcano.Game;

namespace Volcano.Engine
{
    internal class DeepQNetworkEngine : IEngine, IStatus, ILearn
    {
        public event EventHandler<LearnStatus> OnDebug;

        public event EventHandler<EngineStatus> OnStatus;

        private NeuralNetwork network;
        private NeuralNetwork targetNetwork; // Target network for stable Q-value estimation
        private ReplayBuffer replayBuffer;
        private Random random;

        private double epsilon;
        private double epsilonStart = 1.0;
        private double epsilonEnd = 0.1;
        private int totalEpisodes = 100;

        private double gamma = 0.99; // discount factor
        private double learningRate = 0.00001; // Reduced from 0.001
        private int miniBatchSize = 32;

        private int episodeCounter = 0;
        private int targetUpdateFrequency = 10; // Update target network every 10 episodes
        private const string NETWORK_FILE = "dqn.dat";

        public DeepQNetworkEngine()
        {
            random = new Random();
            network = new NeuralNetwork(learningRate);
            targetNetwork = new NeuralNetwork(learningRate);
            replayBuffer = new ReplayBuffer(5000);

            // Initialize target network with same weights
            targetNetwork.CopyWeightsFrom(network);

            // Try to load existing network
            if (File.Exists(NETWORK_FILE))
            {
                network.Load(NETWORK_FILE);
                targetNetwork.CopyWeightsFrom(network);
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
            double episodeLoss = 0.0;

            const string CSV_FILE = "training.csv";

            // Initialize CSV with headers
            using (System.IO.StreamWriter csv = new System.IO.StreamWriter(CSV_FILE, false))
            {
                csv.WriteLine("Episode,Loss,WinRate");
            }

            for (int episode = 0; episode < totalEpisodes; episode++)
            {
                episodeCounter = episode;

                // Decay epsilon
                epsilon = epsilonStart - (epsilonStart - epsilonEnd) * (episode / (double)totalEpisodes);

                // Randomly assign learner to Player 1 or 2
                bool learnerIsPlayerOne = random.Next(2) == 0;

                // Decide if we play against opponent or random
                bool playAgainstOpponent = (episode % 10 == 0 && episode > 0);

                Board gameBoard = new Board();
                Player winner = PlayGame(gameBoard, learnerIsPlayerOne, playAgainstOpponent);

                // Track wins/losses from learner's perspective
                Player learnerPlayer = learnerIsPlayerOne ? Player.One : Player.Two;
                if (winner == learnerPlayer)
                {
                    totalWins++;
                }
                else if (winner != Player.Draw)
                {
                    totalLosses++;
                }
                else
                {
                    totalDraws++;
                }

                // Train on mini-batches and get loss
                episodeLoss = 0.0;
                if (replayBuffer.Count >= miniBatchSize)
                {
                    // Run multiple minibatch updates for stability
                    for (int i = 0; i < 4; i++)
                    {
                        episodeLoss += TrainOnMiniBatch();
                    }
                    episodeLoss /= 4.0; // Average loss across 4 updates
                }

                // Update target network periodically
                if ((episode + 1) % targetUpdateFrequency == 0)
                {
                    targetNetwork.CopyWeightsFrom(network);
                    Debug($"Target network synchronized after episode {episode + 1}");
                }

                double winRate = totalWins / (double)(episode + 1);
                Debug($"Episode {episode + 1}/{totalEpisodes} | Win Rate: {winRate:F3} | Loss: {episodeLoss:F6} | Epsilon: {epsilon:F3} | Opponent: {playAgainstOpponent} | LearnerIsP1: {learnerIsPlayerOne}");

                // Write to CSV
                using (System.IO.StreamWriter csv = new System.IO.StreamWriter(CSV_FILE, true))
                {
                    csv.WriteLine($"{episode + 1},{episodeLoss:F6},{winRate:F6}");
                }

                // Save network every 10 episodes
                if ((episode + 1) % 10 == 0)
                {
                    network.Save(NETWORK_FILE);
                    Debug($"Checkpoint: Network saved after episode {episode + 1}");
                }
            }

            // Save network at end
            network.Save(NETWORK_FILE);
            Debug("Training complete. Network saved to dqn.dat");
        }

        private Player PlayGame(Board board, bool learnerIsPlayerOne, bool playAgainstOpponent)
        {
            IEngine opponentEngine = playAgainstOpponent ? LoadOpponentEngine() : new RandomEngine();
            EngineCancellationToken token = new EngineCancellationToken(() => false);

            // Assign engines based on learner's player role
            IEngine engineP1 = learnerIsPlayerOne ? this : opponentEngine;
            IEngine engineP2 = learnerIsPlayerOne ? opponentEngine : this;

            // Play game step by step
            while (board.Winner == Player.Empty && board.Turn < 500)
            {
                // Determine current engine
                IEngine currentEngine = board.Player == Player.One ? engineP1 : engineP2;
                bool isLearnerMove = (board.Player == Player.One) == learnerIsPlayerOne;

                // Encode state before move
                double[] stateEnc = EncodeState(board);

                // Get best move from current engine
                SearchResult searchResult = currentEngine.GetBestMove(board, 1, token);
                int move = searchResult.BestMove;

                // Validate move
                if (move < 0 || move >= 80)
                {
                    List<int> validMoves = board.GetMoves();
                    if (validMoves.Count > 0)
                        move = validMoves[random.Next(validMoves.Count)];
                    else
                        break;
                }

                // Make the move
                board.MakeMove(move);

                // Encode state after move
                double[] nextStateEnc = EncodeState(board);

                // Only store transition if learner made this move
                if (isLearnerMove)
                {
                    double reward = 0;
                    bool done = board.Winner != Player.Empty;

                    if (done)
                    {
                        // Reward from learner's perspective
                        Player learnerPlayer = learnerIsPlayerOne ? Player.One : Player.Two;
                        if (board.Winner == learnerPlayer)
                            reward = 1.0;
                        else if (board.Winner != Player.Draw)
                            reward = -1.0;
                        // Draw stays 0
                    }

                    replayBuffer.Add(new Transition(stateEnc, move, reward, nextStateEnc, done));
                }
            }

            return board.Winner;
        }

        private IEngine LoadOpponentEngine()
        {
            // hardcoded to just mcts for now
            return new MonteCarloTreeSearchEngine();
        }

        private double TrainOnMiniBatch()
        {
            List<Transition> batch = replayBuffer.SampleMiniBatch(miniBatchSize);
            double totalLoss = 0.0;

            foreach (Transition transition in batch)
            {
                // Use target network for stable Q-value estimation
                double[] nextQValues = targetNetwork.Forward(transition.NextState);
                double maxNextQ = nextQValues.Max();
                double targetQ = transition.Done ? transition.Reward : transition.Reward + gamma * maxNextQ;

                // Forward pass
                double[] qValues = network.Forward(transition.State);
                double currentQ = qValues[transition.Action];

                // Compute loss (Bellman error squared)
                double bellmanError = currentQ - targetQ;
                totalLoss += bellmanError * bellmanError;

                // Compute loss gradient
                double[] lossGradient = new double[80];
                lossGradient[transition.Action] = bellmanError;

                // Backward pass
                network.Backward(lossGradient, 1.0 / miniBatchSize);
            }

            return totalLoss / miniBatchSize;
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
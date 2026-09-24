using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private double epsilonEnd = 0.05;
        private int totalEpisodes = 100000;

        private double gamma = 0.95;
        private double learningRate = 0.001;
        private int miniBatchSize = 32;

        private int episodeCounter = 0;
        private int targetUpdateFrequency = 5;
        private bool currentGameExplore = false;
        private const string NETWORK_FILE = "dqn.dat";
        private const string BEST_NETWORK_FILE = "dqn_best.dat";

        // Validation-based early stopping
        private int validationFrequency = 100;

        private int validationGamesPerCheckpoint = 10;
        private double bestValidationWinRate = -1.0;
        private int patienceCounter = 0;
        private int patienceLimit = 500;

        private int replayBufferSize = 100000;
        private int replayBufferMinCount = 1000;

        private Canonical _canonical;

        public DeepQNetworkEngine()
        {
            random = new Random();
            network = new NeuralNetwork(learningRate);
            targetNetwork = new NeuralNetwork(learningRate);
            replayBuffer = new ReplayBuffer(replayBufferSize);

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
            // TODO: canonicalize
            _canonical = new Canonical();
            _canonical.SetIndex(state);

            double[] qValues = network.Forward(EncodeState(state));
            List<int> validMoves = state.GetMoves();

            int bestMove = -1;

            // Collect move evaluations
            EngineStatus status = new EngineStatus();

            if (currentGameExplore)
            {
                // Pure random exploration for this entire game
                bestMove = validMoves[random.Next(validMoves.Count)];
            }
            else
            {
                // Greedy: pick move with highest Q-value
                double bestQValue = double.MinValue;
                foreach (int move in validMoves)
                {
                    double qValue = qValues[move];
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

            // Report Q-values for all moves
            foreach (int move in validMoves)
            {
                status.Add(move, qValues[move], "", qValues[move]);
            }

            status.Sort();
            Report(status);

            return new SearchResult(bestMove);
        }

        public void Train()
        {
            Debug($"Starting Deep Q-Learning training for up to {totalEpisodes} episodes with validation-based early stopping");

            episodeCounter = 0;
            int totalWins = 0;
            int totalLosses = 0;
            int totalDraws = 0;
            double episodeLoss = 0.0;

            const string CSV_FILE = "training_progress.csv";
            const string VALIDATION_CSV_FILE = "validation_progress.csv";

            // Initialize training CSV with headers
            if (!File.Exists(CSV_FILE))
            {
                using (System.IO.StreamWriter csv = new System.IO.StreamWriter(CSV_FILE, false))
                {
                    csv.WriteLine("Episode,Loss,WinRate");
                }
            }

            // Initialize validation CSV with headers
            if (!File.Exists(VALIDATION_CSV_FILE))
            {
                using (System.IO.StreamWriter csv = new System.IO.StreamWriter(VALIDATION_CSV_FILE, false))
                {
                    csv.WriteLine("Episode,ValidationWinRate,BestWinRate,Patience");
                }
            }

            // Load best network if it exists
            if (File.Exists(BEST_NETWORK_FILE))
            {
                network.Load(BEST_NETWORK_FILE);
                targetNetwork.CopyWeightsFrom(network);
                Debug("Loaded best network from checkpoint");
            }

            for (int batch = 0; batch < totalEpisodes; batch++)
            {
                // Collect 5 games per batch
                const int gamesPerBatch = 5;

                for (int gameInBatch = 0; gameInBatch < gamesPerBatch; gameInBatch++)
                {
                    episodeCounter = batch * gamesPerBatch + gameInBatch;

                    // Decay epsilon based on total games played
                    int totalGamesPlayed = episodeCounter;
                    epsilon = epsilonStart - (epsilonStart - epsilonEnd) * (totalGamesPlayed / (double)(totalEpisodes * gamesPerBatch));

                    // Randomly assign learner to Player 1 or 2
                    bool learnerIsPlayerOne = random.Next(2) == 0;

                    // Decide if we play against opponent or random
                    // TODO: enable when the bot is stronger
                    bool playAgainstOpponent = false; // (episodeCounter % 10 == 0 && episodeCounter > 0);

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
                }

                // Train on mini-batches after collecting 5 games
                episodeLoss = 0.0;
                if (replayBuffer.Count >= miniBatchSize && replayBuffer.Count >= replayBufferMinCount)
                {
                    // Run multiple minibatch updates for stability
                    for (int i = 0; i < 8; i++)
                    {
                        episodeLoss += TrainOnMiniBatch();
                    }
                    episodeLoss /= 8.0; // Average loss across 8 updates
                }

                double winRate = totalWins / (double)(episodeCounter + 1);
                Debug($"Batch {batch + 1} (Games {episodeCounter + 1}) | Win Rate: {winRate:F3} | Loss: {episodeLoss:F6} | Epsilon: {epsilon:F3}");

                // Update target network periodically
                if ((batch + 1) % targetUpdateFrequency == 0)
                {
                    targetNetwork.CopyWeightsFrom(network);
                    Debug($"Target network synchronized after batch {batch + 1}");
                }

                // Write to CSV
                using (System.IO.StreamWriter csv = new System.IO.StreamWriter(CSV_FILE, true))
                {
                    csv.WriteLine($"{episodeCounter + 1},{episodeLoss:F6},{winRate:F6}");
                }

                // Validation every N batches (~N*5 games)
                if ((batch + 1) % (validationFrequency / 5) == 0 && replayBuffer.Count >= replayBufferMinCount)
                {
                    Debug($"Running validation at batch {batch + 1}...");
                    currentGameExplore = false;
                    double validationWinRate = ValidateNetwork();

                    // Check if this is the best performance so far
                    if (validationWinRate > bestValidationWinRate)
                    {
                        bestValidationWinRate = validationWinRate;
                        patienceCounter = 0; // Reset patience

                        // Save best network
                        network.Save(BEST_NETWORK_FILE);
                        Debug($"Validation win rate improved to {validationWinRate:F3}! Saving best network.");
                    }
                    else
                    {
                        // Only apply patience if we've seen at least one win
                        if (bestValidationWinRate > 0.0)
                        {
                            patienceCounter += validationFrequency / 5;
                            Debug($"Validation win rate: {validationWinRate:F3} (no improvement). Patience: {patienceCounter}/{patienceLimit}");
                        }
                        else
                        {
                            Debug($"Validation win rate: {validationWinRate:F3} (still searching for first win, patience disabled)");
                        }
                    }

                    // Log validation result
                    using (System.IO.StreamWriter csv = new System.IO.StreamWriter(VALIDATION_CSV_FILE, true))
                    {
                        csv.WriteLine($"{episodeCounter + 1},{validationWinRate:F6},{bestValidationWinRate:F6},{patienceCounter}");
                    }

                    // Early stopping check (only if we've had at least one win)
                    if (bestValidationWinRate > 0.0 && patienceCounter >= patienceLimit)
                    {
                        Debug($"Early stopping triggered! No improvement for {patienceLimit} games. Training complete.");
                        network.Load(BEST_NETWORK_FILE); // Load best weights before ending
                        network.Save(NETWORK_FILE);
                        break;
                    }
                }

                // Save network every 10 batches (~50 games)
                if ((batch + 1) % 10 == 0)
                {
                    network.Save(NETWORK_FILE);
                    Debug($"Checkpoint: Network saved after batch {batch + 1}");
                }
            }

            // Save network at end
            network.Save(NETWORK_FILE);
            Debug($"Training complete. Best validation win rate: {bestValidationWinRate:F3}. Networks saved to dqn.dat and dqn_best.dat");
        }

        private Player PlayGame(Board board, bool learnerIsPlayerOne, bool playAgainstOpponent)
        {
            IEngine opponentEngine = playAgainstOpponent ? LoadOpponentEngine() : new RandomEngine();
            EngineCancellationToken token = new EngineCancellationToken(() => false);

            // Assign engines based on learner's player role
            IEngine engineP1 = learnerIsPlayerOne ? this : opponentEngine;
            IEngine engineP2 = learnerIsPlayerOne ? opponentEngine : this;

            // Collect learner transitions for this game
            List<Transition> gameTransitions = new List<Transition>();

            // Play game step by step
            while (board.Winner == Player.Empty && board.Turn < 500)
            {
                // Set exploration for this move
                currentGameExplore = random.NextDouble() < epsilon;

                // Determine current engine
                IEngine currentEngine = board.Player == Player.One ? engineP1 : engineP2;
                bool isLearnerMove = (board.Player == Player.One && learnerIsPlayerOne) || (board.Player == Player.Two && !learnerIsPlayerOne);

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

                var playerToMove = board.Player;

                // Make the move
                board.MakeMove(move);

                var playerSwap = board.Player != playerToMove;

                // Encode state after move
                double[] nextStateEnc = EncodeState(board);

                // Collect transition if learner made this move
                if (isLearnerMove)
                {
                    // Use higher intermediate reward for stronger signal propagation
                    double reward = 0.05;
                    bool done = false; // Assume not done; will be corrected for final move
                    gameTransitions.Add(new Transition(stateEnc, move, reward, nextStateEnc, done, playerSwap));
                }
            }

            // After game ends, backup terminal reward through trajectory
            if (gameTransitions.Count > 0)
            {
                Player learnerPlayer = learnerIsPlayerOne ? Player.One : Player.Two;
                double terminalReward = 0.0;
                if (board.Winner == learnerPlayer)
                    terminalReward = 1.0; // Win
                else if (board.Winner != Player.Draw)
                    terminalReward = -1.0; // Loss
                // else terminalReward = 0.0; // Draw

                // Backup the terminal reward through the trajectory with proper discounting
                // Earlier moves get a discounted version of the terminal outcome
                double backupReward = terminalReward;

                for (int i = gameTransitions.Count - 1; i >= 0; i--)
                {
                    var transition = gameTransitions[i];
                    bool isDone = (i == gameTransitions.Count - 1); // Only final move is terminal

                    // For this move, use the backup reward
                    double moveReward = isDone ? backupReward : 0.01;

                    gameTransitions[i] = new Transition(
                        transition.State,
                        transition.Action,
                        moveReward,
                        transition.NextState,
                        isDone,
                        transition.NextTurnIsOpponent
                    );

                    // For earlier moves, discount the future signal
                    backupReward = 0.01 + gamma * backupReward;
                }
            }

            // Add all transitions to replay buffer
            foreach (var transition in gameTransitions)
            {
                replayBuffer.Add(transition);
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

            // Set training mode for batch normalization
            network.SetTrainingMode(true);

            // Create batch-wide accumulator arrays for gradients (new dimensions)
            double[,] totalConvWGrad = new double[52, 32];
            double[,] totalW1Grad = new double[2560, 128];
            double[] totalB1Grad = new double[128];
            double[,] totalW2Grad = new double[128, 80];
            double[] totalB2Grad = new double[80];

            const double GRAD_CLIP = 0.1;

            foreach (Transition transition in batch)
            {
                // 1. Calculate Target using Target Network
                targetNetwork.SetTrainingMode(false);
                double[] nextQValues = targetNetwork.Forward(transition.NextState);
                double maxNextQ = nextQValues.Max();
                if (transition.NextTurnIsOpponent)
                {
                    maxNextQ = -maxNextQ;
                }
                double targetQ = transition.Done ? transition.Reward : transition.Reward + gamma * maxNextQ;

                // 2. Forward pass on Main Network to cache its internal states
                double[] qValues = network.Forward(transition.State);
                double currentQ = qValues[transition.Action];

                // 3. Compute loss metrics
                double bellmanError = currentQ - targetQ;
                totalLoss += bellmanError * bellmanError;

                // 4. Calculate local gradients for this individual transition
                double[] lossGradient = new double[80];
                lossGradient[transition.Action] = 2.0 * bellmanError;

                // Clip the initial loss gradient to prevent explosion
                lossGradient[transition.Action] = Math.Max(-GRAD_CLIP, Math.Min(GRAD_CLIP, lossGradient[transition.Action]));

                // 5. Run a modified backprop pass that extracts gradients WITHOUT updating weights
                network.ComputeGradients(lossGradient, out double[,] convWG, out double[,] w1G, out double[] b1G, out double[,] w2G, out double[] b2G);

                // 6. Accumulate the gradients across the mini-batch with clipping
                AccumulateGradientsWithClip(totalConvWGrad, convWG, GRAD_CLIP);
                AccumulateGradientsWithClip(totalW1Grad, w1G, GRAD_CLIP);
                AccumulateArraysWithClip(totalB1Grad, b1G, GRAD_CLIP);
                AccumulateGradientsWithClip(totalW2Grad, w2G, GRAD_CLIP);
                AccumulateArraysWithClip(totalB2Grad, b2G, GRAD_CLIP);
            }

            double avgLoss = totalLoss / miniBatchSize;
            if (avgLoss > 1e6 || double.IsNaN(avgLoss) || double.IsInfinity(avgLoss))
            {
                Debug($"WARNING: Loss explosion! avgLoss={avgLoss}");
            }

            // 7. Apply the accumulated mini-batch gradients to the weights exactly ONCE
            network.ApplyMiniBatchUpdates(totalConvWGrad, totalW1Grad, totalB1Grad, totalW2Grad, totalB2Grad, 1.0 / miniBatchSize);

            return avgLoss;
        }

        private void AccumulateGradientsWithClip(double[,] target, double[,] source, double clip)
        {
            for (int i = 0; i < target.GetLength(0); i++)
            {
                for (int j = 0; j < target.GetLength(1); j++)
                {
                    double clipped = Math.Max(-clip, Math.Min(clip, source[i, j]));
                    target[i, j] += clipped;
                }
            }
        }

        private void AccumulateArraysWithClip(double[] target, double[] source, double clip)
        {
            for (int i = 0; i < target.Length; i++)
            {
                double clipped = Math.Max(-clip, Math.Min(clip, source[i]));
                target[i] += clipped;
            }
        }

        // Simple helper methods to sum up your arrays
        private void AccumulateGradients(double[,] target, double[,] source)
        {
            for (int i = 0; i < target.GetLength(0); i++)
                for (int j = 0; j < target.GetLength(1); j++)
                    target[i, j] += source[i, j];
        }

        private void AccumulateArrays(double[] target, double[] source)
        {
            for (int i = 0; i < target.Length; i++)
                target[i] += source[i];
        }

        private double[] EncodeState(Board board)
        {
            const int NUM_TILES = 80;
            const int NUM_PIECE_CHANNELS = 9;
            const int NUM_TURN_CHANNELS = 4;
            const int TOTAL_INPUT = NUM_TILES * (NUM_PIECE_CHANNELS + NUM_TURN_CHANNELS);

            double[] encoded = new double[TOTAL_INPUT]; // 80 tiles * 13 channels

            // Determine turn phase: (0=p1, 1=p2, 2=grow, 3=p2, 4=p1, 5=grow)
            int phase = (board.Turn - 1) % 6;

            // Compute turn-state channels
            bool p1Active = (phase == 0 || phase == 4);
            bool p2Active = (phase == 1 || phase == 3);
            bool nextNonGrowthIsP1 = (phase == 1 || phase == 2); // Next is 3 or 0
            bool nextNonGrowthIsP2 = (phase == 0 || phase == 5); // Next is 1 or 4

            for (int i = 0; i < NUM_TILES; i++)
            {
                int tileValue = board.Tiles[i];

                //if (invertForPlayer2)
                //{
                //    tileValue = -tileValue;
                //}

                int oneHotIndex = 0;

                if (tileValue == 0)
                    oneHotIndex = 0;
                else if (tileValue < 0)
                    oneHotIndex = 5 + tileValue;
                else
                    oneHotIndex = 4 + tileValue;

                encoded[i * NUM_PIECE_CHANNELS + oneHotIndex] = 1.0;

                // Encode 4 turn-state channels per tile
                int turnChannelBase = NUM_TILES * NUM_PIECE_CHANNELS + i * NUM_TURN_CHANNELS;
                if (p1Active) encoded[turnChannelBase + 0] = 1.0;
                if (p2Active) encoded[turnChannelBase + 1] = 1.0;
                if (nextNonGrowthIsP1) encoded[turnChannelBase + 2] = 1.0;
                if (nextNonGrowthIsP2) encoded[turnChannelBase + 3] = 1.0;
            }

            return encoded;
        }

        private double ValidateNetwork()
        {
            //Debug("VALIDATION DISABLED");
            //return 0;

            int validationWins = 0;

            Parallel.For(0, validationGamesPerCheckpoint, gameNum =>
            {
                // Play validation games against MCTS to measure current performance
                EngineCancellationToken token = new EngineCancellationToken(() => false);

                var playAsP1 = gameNum % 2 == 0;

                // Learner is Player.One for validation
                Board gameBoard = new Board();
                IEngine test = this;
                IEngine enemy = new RandomEngine(); // TODO: new MonteCarloTreeSearchEngine();
                IEngine engineP1 = playAsP1 ? test : enemy;
                IEngine engineP2 = playAsP1 ? enemy : test;

                // Play game without storing transitions
                while (gameBoard.Winner == Player.Empty && gameBoard.Turn < 500)
                {
                    IEngine currentEngine = gameBoard.Player == Player.One ? engineP1 : engineP2;

                    SearchResult searchResult = currentEngine.GetBestMove(gameBoard, 1, token);
                    int move = searchResult.BestMove;

                    // Validate move
                    if (move < 0 || move >= 80)
                    {
                        List<int> validMoves = gameBoard.GetMoves();
                        if (validMoves.Count > 0)
                            move = validMoves[random.Next(validMoves.Count)];
                        else
                            break;
                    }

                    gameBoard.MakeMove(move);
                }

                // Check if learner (Player.One) won
                if (gameBoard.Winner == Player.One && playAsP1)
                {
                    validationWins++;
                }
                else if (gameBoard.Winner == Player.Two && !playAsP1)
                {
                    validationWins++;
                }
            });

            double validationWinRate = validationWins / (double)validationGamesPerCheckpoint;
            return validationWinRate;
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
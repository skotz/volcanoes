namespace Volcano.Neural
{
    public class AlphaZeroConfig
    {
        /// <summary>Exploration constant in the PUCT formula.</summary>
        public double C { get; init; } = 1.2;

        /// <summary>
        /// MCTS simulations per move. This sets the quality of the policy targets the network
        /// learns to imitate, so it matters more than network size.
        /// </summary>
        public int NumSearches { get; init; } = 50; // 500

        /// <summary>Residual blocks in the network.</summary>
        public int NumResBlocks { get; init; } = 6;

        /// <summary>Channels (hidden width) in the network.</summary>
        public int NumHidden { get; init; } = 128;

        /// <summary>Outer self-play -> train iterations.</summary>
        public int NumIterations { get; init; } = 50;

        /// <summary>Self-play games per iteration.</summary>
        public int NumSelfPlayIterations { get; init; } = 2048;

        /// <summary>
        /// Games searched simultaneously. Every network call evaluates this many positions, so
        /// raising it is nearly free on a latency-bound GPU. Lower it if CUDA runs out of memory.
        /// </summary>
        public int NumParallelGames { get; init; } = 2048;

        /// <summary>
        /// How many recent self-play iterations to train on. With 1 the network overfits each
        /// fresh sample and forgets the last one; a window of a few iterations keeps the value
        /// targets stable. Memory cost is small — positions are stored as raw boards.
        /// </summary>
        public int ReplayBufferIterations { get; init; } = 5;

        /// <summary>
        /// Training epochs over the replay buffer each iteration. Keep this low: with a window
        /// of N iterations every position is already revisited across N rounds, so a high epoch
        /// count re-fits the same data many times over.
        /// </summary>
        public int NumEpochs { get; init; } = 5;

        /// <summary>Mini-batch size for training.</summary>
        public int BatchSize { get; init; } = 512;

        /// <summary>
        /// Temperature applied to visit counts when sampling self-play moves. 1.0 samples in
        /// proportion to visits; above 1.0 flattens the distribution and adds randomness.
        /// </summary>
        public double Temperature { get; init; } = 1.0;

        /// <summary>
        /// Plies played with <see cref="Temperature"/> before switching to greedy play. Keeping
        /// randomness in the opening diversifies the games; keeping it in the endgame would throw
        /// away won positions and corrupt the value targets.
        /// </summary>
        public int TemperatureMoves { get; init; } = 12;

        /// <summary>Weight of Dirichlet exploration noise at the root. Set to 0 for a deterministic demo.</summary>
        public double DirichletEpsilon { get; init; } = 0.0; // 0.25 for training

        /// <summary>
        /// Concentration of the Dirichlet noise. Below 1 the samples are spiky, which is what
        /// makes them useful — they boost a few random moves. Above 1 they concentrate near
        /// uniform and add almost no exploration. AlphaZero's rule of thumb is
        /// alpha ~ 10 / (typical legal moves), so ~0.15 for a 65-action space.
        /// </summary>
        public double DirichletAlpha { get; init; } = 0.15;

        /// <summary>Initial learning rate for the Adam optimizer.</summary>
        public double InitialLearningRate { get; init; } = 0.001;

        /// <summary>
        /// Learning rate decay factor (gamma) applied by the scheduler after each iteration.
        /// e.g. 0.95 means the LR drops by 5% each macro-iteration.
        /// </summary>
        public double LearningRateDecay { get; init; } = 0.95;
    }
}
using Volcano.Engine;
using Volcano.Game;

namespace Volcano.Neural
{
    internal class ModelBenchmarkEngine : IEngine
    {
        private readonly AlphaZero _alphaZero;
        private readonly int _forcedIterations;

        public ModelBenchmarkEngine(AlphaZero alphaZero, int forcedIterations)
        {
            _alphaZero = alphaZero;
            _forcedIterations = forcedIterations;
        }

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            var move = _alphaZero.GetBestMove(state, maxSeconds, _forcedIterations, token, false);

            return new SearchResult
            {
                BestMove = move.Item1,
                Simulations = move.Item2,
            };
        }
    }
}

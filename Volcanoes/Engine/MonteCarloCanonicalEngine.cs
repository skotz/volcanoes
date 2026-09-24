using Volcano.Game;

namespace Volcano.Engine
{
    internal class MonteCarloCanonicalEngine : MonteCarloTreeSearchEngine
    {
        protected override int MonteCarloTreeSearch(Board rootState)
        {
            var canonical = new Canonical();
            canonical.SetIndex(rootState);

            var canonicalized = canonical.Canonicalize(rootState);

            var best = base.MonteCarloTreeSearch(canonicalized);

            return canonical.CanonicalToBoard(best);
        }
    }
}
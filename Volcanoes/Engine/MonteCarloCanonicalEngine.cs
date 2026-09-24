using Volcano.Game;

namespace Volcano.Engine
{
    internal class MonteCarloCanonicalEngine : MonteCarloTreeSearchEngine
    {
        protected override int MonteCarloTreeSearch(Board rootState)
        {
            var canonical = new Canonical();
            canonical.SetIndex(rootState.Tiles);

            var canonicalized = new Board(rootState)
            {
                Tiles = canonical.Canonicalize(rootState.Tiles),
                Dormant = canonical.Canonicalize(rootState.Dormant)
            };

            var best = base.MonteCarloTreeSearch(canonicalized);

            return canonical.CanonicalToBoard(best);
        }
    }
}
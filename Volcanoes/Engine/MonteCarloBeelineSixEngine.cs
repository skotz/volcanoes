using System.Collections.Generic;
using Volcano.Game;
using Volcano.Search;

namespace Volcano.Engine
{
    internal class MonteCarloBeelineSixEngine : MonteCarloTreeSearchEngine
    {
        private PathFinder pathFinder = new BeelinePathFinder();

        protected override List<int> GetMoves(Board state)
        {
            var valid = state.GetMoves();
            var moves = new List<int>();

            for (var i = 0; i < 80; i++)
            {
                if (state.Tiles[i] != 0)
                {
                    var path = pathFinder.FindPath(state, i, Constants.Antipodes[i]).Path;
                    foreach (var tile in path)
                    {
                        if (valid.Contains(tile) && !moves.Contains(tile))
                        {
                            moves.Add(tile);
                        }
                    }
                }
            }

            if (moves.Count == 0)
            {
                moves = valid;
            }

            return moves;
        }
    }
}
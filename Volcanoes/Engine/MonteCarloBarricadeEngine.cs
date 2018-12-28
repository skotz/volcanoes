using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;
using Volcano.Search;

namespace Volcano.Engine
{
    class MonteCarloBarricadeEngine : MonteCarloTreeSearchEngine
    {
        private PathFinder pathFinder = new WeightedNonEnemyPathFinder();

        protected override List<int> GetMoves(Board position)
        {
            List<int> moves = position.GetMoves();
            List<int> candidates = new List<int>();

            // For each tile, figure out how long it'll take to get to it's antipode
            for (int i = 0; i < 80; i++)
            {
                if (position.Tiles[i].Owner != Player.Empty && position.Tiles[i].Owner != position.Player)
                {
                    var path = pathFinder.FindPath(position, i, position.Tiles[i].Antipode);
                    if (path != null)
                    {
                        // Try to obstruct our opponent's path
                        foreach (int tile in path.Path)
                        {
                            if (moves.Contains(tile))
                            {
                                candidates.Add(tile);
                            }
                        }
                    }
                }
            }

            // Just return all of the moves
            if (candidates.Count == 0)
            {
                candidates = moves;
            }

            return candidates;
        }
    }
}

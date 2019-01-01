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
            List<int> allMoves = position.GetMoves();
            List<int> candidates = new List<int>();

            // For each tile, find an unobstructed path to it's antipode
            List<PathResult> enemyPaths = new List<PathResult>();
            for (int i = 0; i < 80; i++)
            {
                if ((position.Tiles[i] > 0 && position.Player != Player.One) || (position.Tiles[i] < 0 && position.Player != Player.Two) && position.Tiles[i] != 0)
                {
                    var path = pathFinder.FindPath(position, i, Constants.Antipodes[i]);
                    if (path != null && path.Distance != 0)
                    {
                        enemyPaths.Add(path);
                    }
                }
            }

            // Of all the calculated paths, find the one that's fastest for each player
            PathResult bestEnemy = enemyPaths.OrderBy(x => x.Distance).FirstOrDefault();

            // Add tiles on the calculated paths to the candidate moves
            if (bestEnemy != null)
            {
                foreach (int index in bestEnemy.Path)
                {
                    if (allMoves.Contains(index))
                    {
                        candidates.Add(index);
                    }
                }
            }

            // Just return all of the moves
            if (candidates.Count == 0)
            {
                candidates = allMoves;
            }

            return candidates;
        }
    }
}

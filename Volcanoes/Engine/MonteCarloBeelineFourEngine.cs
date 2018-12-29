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
    class MonteCarloBeelineFourEngine : MonteCarloTreeSearchEngine
    {
        private PathFinder pathFinder = new WeightedNonEnemyPathFinder();

        protected override List<int> GetMoves(Board position)
        {
            List<int> allMoves = position.GetMoves();
            List<int> candidateMoves = new List<int>();

            // For each tile, find an unobstructed path to it's antipode
            List<PathResult> selfPaths = new List<PathResult>();
            List<PathResult> enemyPaths = new List<PathResult>();
            for (int i = 0; i < 80; i++)
            {
                if (position.Tiles[i].Owner == position.Player)
                {
                    var path = pathFinder.FindPath(position, i, position.Tiles[i].Antipode);
                    if (path != null && path.Distance != 0)
                    {
                        selfPaths.Add(path);
                    }
                }
                else if (position.Tiles[i].Owner != Player.Empty)
                {
                    var path = pathFinder.FindPath(position, i, position.Tiles[i].Antipode);
                    if (path != null && path.Distance != 0)
                    {
                        enemyPaths.Add(path);
                    }
                }
            }

            // Of all the calculated paths, find the one that's fastest for each player
            PathResult bestSelf = selfPaths.OrderBy(x => x.Distance).FirstOrDefault();
            PathResult bestEnemy = enemyPaths.OrderBy(x => x.Distance).FirstOrDefault();

            // Add tiles on the calculated paths to the candidate moves
            if (bestSelf != null)
            {
                foreach (int index in bestSelf.Path)
                {
                    if (allMoves.Contains(index))
                    {
                        candidateMoves.Add(index);
                    }
                }
            }
            if (bestEnemy != null)
            {
                foreach (int index in bestEnemy.Path)
                {
                    if (allMoves.Contains(index))
                    {
                        candidateMoves.Add(index);
                    }
                }
            }

            // If we didn't find any candidate moves, just return everything
            if (candidateMoves.Count == 0)
            {
                candidateMoves = allMoves;
            }

            return candidateMoves;
        }
    }
}

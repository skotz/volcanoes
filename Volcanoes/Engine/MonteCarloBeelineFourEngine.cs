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

            // For each tile, find an unobstructed path to it's antipode and add tiles on that path to the candidate moves
            for (int i = 0; i < 80; i++)
            {
                if (position.Tiles[i].Owner != Player.Empty)
                {
                    PathResult path = pathFinder.FindPath(position, i, position.Tiles[i].Antipode);
                    if (path != null)
                    {
                        foreach (int index in path.Path)
                        {
                            if (allMoves.Contains(index))
                            {
                                candidateMoves.Add(index);
                            }
                        }
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

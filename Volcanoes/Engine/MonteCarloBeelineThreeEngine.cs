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
    class MonteCarloBeelineThreeEngine : MonteCarloTreeSearchEngine
    {
        private PathFinder pathFinder = new WeightedNonEnemyPathFinder();

        protected override List<Move> GetMoves(Board position)
        {
            // Get all non-volcano moves
            List<Move> allMoves = position.GetMoves(true, true, true, VolcanoGame.Settings.MaxMagmaChamberLevel + 1);
            if (allMoves.Count == 0)
            {
                // Get all available moves without condition
                allMoves = position.GetMoves();
            }
            if (allMoves.Count == 0)
            {
                // There are no valid moves
                return allMoves;
            }
            if (allMoves[0].MoveType == MoveType.AllGrow)
            {
                // It's a growth phase, so don't wast time
                return allMoves;
            }

            // For each tile we own, figure out how long it'll take to get to it's antipode
            PathResult[] paths = new PathResult[80];
            PathResult[] enemyPaths = new PathResult[80];
            for (int i = 0; i < 80; i++)
            {
                if (position.Tiles[i].Owner == position.Player)
                {
                    paths[i] = pathFinder.FindPath(position, i, position.Tiles[i].Antipode);
                }
                else if (position.Tiles[i].Owner != Player.Empty)
                {
                    enemyPaths[i] = pathFinder.FindPath(position, i, position.Tiles[i].Antipode);
                }
            }

            // Of all the calculated paths, find the one that's fastest
            PathResult best = paths.Where(x => x != null && x.Distance != 0).OrderBy(x => x.Distance).FirstOrDefault();
            PathResult bestEnemy = enemyPaths.Where(x => x != null && x.Distance != 0).OrderBy(x => x.Distance).FirstOrDefault();

            List<Move> moves = new List<Move>();
            if (best != null)
            {
                // Create a list of moves that are on the ideal path
                foreach (int i in best.Path)
                {
                    if (position.Tiles[i].Owner == Player.Empty)
                    {
                        Move move = allMoves.Where(x => x.TileIndex == i).FirstOrDefault();
                        if (move != null)
                        {
                            moves.Add(move);
                        }
                    }
                }

                if (bestEnemy != null)
                {
                    // Create a list of moves that are on the enemy's best path
                    foreach (int i in bestEnemy.Path)
                    {
                        if (position.Tiles[i].Owner == Player.Empty)
                        {
                            Move move = allMoves.Where(x => x.TileIndex == i).FirstOrDefault();
                            if (move != null)
                            {
                                moves.Insert(0, move);
                            }
                        }
                    }
                }

                // If there are no empty tiles left, pick magma chambers
                if (moves.Count == 0)
                {
                    foreach (int i in best.Path)
                    {
                        if (position.Tiles[i].Owner == position.Player && position.Tiles[i].Value <= VolcanoGame.Settings.MaxMagmaChamberLevel)
                        {
                            Move move = allMoves.Where(x => x.TileIndex == i).FirstOrDefault();
                            if (move != null)
                            {
                                moves.Add(move);
                            }
                        }
                    }
                }
            }
            
            if (moves.Count == 0)
            {
                moves = position.GetMoves();
            }

            return moves;
        }
    }
}

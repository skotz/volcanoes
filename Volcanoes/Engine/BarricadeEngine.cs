using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;
using Volcano.Search;

namespace Volcano.Engine
{
    class BarricadeEngine : IEngine
    {
        private PathFinder pathFinder = new WeightedNonEnemyPathFinder();
        private Random random = new Random();

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            List<int> moves = state.GetMoves();
            int best = -3;

            // For each tile we own, figure out how long it'll take to get to it's antipode
            PathResult[] selfPaths = new PathResult[80];
            PathResult[] enemyPaths = new PathResult[80];
            for (int i = 0; i < 80; i++)
            {
                if (state.Tiles[i].Owner == state.Player)
                {
                    selfPaths[i] = pathFinder.FindPath(state, i, Constants.Antipodes[i]);
                }
                else if (state.Tiles[i].Owner != Player.Empty)
                {
                    enemyPaths[i] = pathFinder.FindPath(state, i, Constants.Antipodes[i]);
                }
            }

            // Of all the calculated paths, find the one that's fastest
            PathResult bestSelfPath = selfPaths.Where(x => x != null && x.Distance != 0).OrderBy(x => x.Distance).FirstOrDefault();
            PathResult bestEnemyPath = enemyPaths.Where(x => x != null && x.Distance != 0).OrderBy(x => x.Distance).FirstOrDefault();

            if (bestSelfPath != null && bestEnemyPath != null)
            {
                if (bestSelfPath.Distance <= bestEnemyPath.Distance - 2)
                {
                    // If we're two moves ahead of our opponent, then run to the finish line
                    foreach (int tile in bestSelfPath.Path)
                    {
                        if (state.Tiles[tile].Owner == Player.Empty && moves.Contains(tile))
                        {
                            best = tile;
                            break;
                        }
                    }
                }
                else
                {
                    // Try to obstruct our opponent's path
                    foreach (int tile in bestEnemyPath.Path)
                    {
                        if (state.Tiles[tile].Owner == Player.Empty && moves.Contains(tile))
                        {
                            best = tile;
                            break;
                        }
                    }
                }
            }

            // When all else fails, pick a random move
            if (best < 0)
            {
                best = moves[random.Next(moves.Count)];
            }

            return new SearchResult(best);
        }
    }
}

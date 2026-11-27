using System;
using System.Linq;
using Volcano.Game;
using Volcano.Search;

namespace Volcano.Engine
{
    internal class BeeLineTwoEngine : IEngine, IStatus
    {
        private TotalPathFinder pathFinder = new TotalPathFinder();

        public event EventHandler<EngineStatus> OnStatus;

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            var paths = pathFinder.GetAllPaths(state);
            var scores = new double[80];

            var bestPath = paths.MyPaths.OrderBy(x => x.Distance).FirstOrDefault();
            var bestOppPath = paths.OpponentPaths.OrderBy(x => x.Distance).FirstOrDefault();

            if (state.Turn == 2)
            {
                var canonical = new Canonical();
                canonical.SetIndex(state);
                var board = canonical.Canonicalize(state);

                // N07
                if (board.Tiles[0] != 0)
                {
                    var starter = canonical.CanonicalToBoard(Constants.TileIndexes["N22"]);
                    scores[starter] = 100;
                }
            }

            if (bestPath != null)
            {
                foreach (var tile in bestPath.Path)
                {
                    var neighbors = 0;
                    foreach (var n in Constants.AdjacentIndexes[tile])
                    {
                        if (state.Tiles[n] > 0 && state.Tiles[tile] > 0)
                        {
                            neighbors++;
                        }
                        else if (state.Tiles[n] < 0 && state.Tiles[tile] < 0)
                        {
                            neighbors++;
                        }
                    }

                    scores[tile] += state.Tiles[tile] == 0 ? 1 : 0.5;
                    scores[tile] -= neighbors / 6.0;
                }
            }

            if (bestOppPath != null)
            {
                foreach (var tile in bestOppPath.Path)
                {
                    scores[tile] += state.Tiles[tile] == 0 ? 0.75 : 0.25;
                }
            }

            var ordered = state.GetMoves().OrderByDescending(x => scores[x]);
            var best = state.GetMoves().OrderByDescending(x => scores[x]).FirstOrDefault();

            if (OnStatus != null)
            {
                var status = new EngineStatus();
                var max = ordered.Max(x => scores[x]);
                foreach (var child in ordered)
                {
                    var eval = max == 0 ? 0 : scores[child] / max;
                    status.Add(child, Math.Round(eval, 2), Constants.TileNames[child], eval);
                }
                status.Sort();
                OnStatus?.Invoke(this, status);
            }

            return new SearchResult(best);
        }
    }
}
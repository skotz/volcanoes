using System;
using System.Collections.Generic;
using System.Linq;
using Volcano.Game;

namespace Volcano.Search
{
    internal class BeelineRolloutStrategy : IRolloutStrategy
    {
        private double _rate;
        private int _count;
        private TotalPathFinder _pathFinder;
        private Dictionary<long, TotalPathFinderResult> _cache;

        public BeelineRolloutStrategy(double rate, int count)
        {
            _rate = rate;
            _count = count;
            _pathFinder = new TotalPathFinder();
            _cache = new Dictionary<long, TotalPathFinderResult>();
        }

        public bool UseStrategy()
        {
            return Random.Shared.NextDouble() < _rate;
        }

        public List<int> GetAllMoves(Board state)
        {
            var hash = state.GetFullHash();
            _cache.TryGetValue(hash, out var paths);

            if (paths == null)
            {
                paths = _pathFinder.GetAllPaths(state);
                _cache.TryAdd(hash, paths);
            }

            var scores = new double[80];

            var bestPath = paths.MyPaths.OrderBy(x => x.Distance).FirstOrDefault();
            var bestOppPath = paths.OpponentPaths.OrderBy(x => x.Distance).FirstOrDefault();

            if (bestPath != null)
            {
                var filled = bestPath.Path.Count(x => state.Tiles[x] != 0);
                var empty = bestPath.Path.Count(x => state.Tiles[x] == 0);
                var progress = (double)filled / (filled + empty);

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

                    scores[tile] += state.Tiles[tile] == 0 ? progress : 0.01;
                    scores[tile] -= neighbors / 6.0;
                }
            }

            if (bestOppPath != null)
            {
                var filled = bestOppPath.Path.Count(x => state.Tiles[x] != 0);
                var empty = bestOppPath.Path.Count(x => state.Tiles[x] == 0);
                var progress = (double)filled / (filled + empty);

                foreach (var tile in bestOppPath.Path)
                {
                    scores[tile] += state.Tiles[tile] == 0 ? progress : 0.01;
                }
            }

            return state
                .GetMoves()
                .OrderByDescending(x => scores[x])
                .ToList();
        }

        public int GetNextMove(Board state)
        {
            var candidates = GetAllMoves(state).Take(_count).ToList();

            if (candidates.Count == 0)
            {
                return -1;
            }
            else
            {
                return candidates[Random.Shared.Next(candidates.Count)];
            }
        }
    }
}
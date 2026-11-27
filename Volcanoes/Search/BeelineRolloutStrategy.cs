using System;
using System.Collections.Generic;
using System.Linq;
using Volcano.Game;

namespace Volcano.Search
{
    internal class BeelineRolloutStrategy : IRolloutStrategy
    {
        private double _rate;
        private TotalPathFinder _pathFinder;
        private Dictionary<long, TotalPathFinderResult> _cache;

        public BeelineRolloutStrategy()
        {
            _rate = 0.75;
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

            var candidates = new List<int>();
            var done = new bool[80];
            var p1 = state.Player == Player.One;

            var bestPath = paths.MyPaths.OrderBy(x => x.Distance).FirstOrDefault();
            var bestOppPath = paths.OpponentPaths.OrderBy(x => x.Distance).FirstOrDefault();

            if (bestPath != null)
            {
                foreach (var tile in bestPath.Path)
                {
                    if (!done[tile])
                    {
                        if (state.Tiles[tile] == 0 || (p1 && state.Tiles[tile] > 0 && state.Tiles[tile] < 4) || (!p1 && state.Tiles[tile] < 0 && state.Tiles[tile] > -4))
                        {
                            candidates.Add(tile);
                            done[tile] = true;
                        }
                    }
                }
            }

            if (bestOppPath != null)
            {
                foreach (var tile in bestOppPath.Path)
                {
                    if (!done[tile])
                    {
                        if (state.Tiles[tile] == 0 || (p1 && state.Tiles[tile] > 0 && state.Tiles[tile] < 4) || (!p1 && state.Tiles[tile] < 0 && state.Tiles[tile] > -4))
                        {
                            candidates.Add(tile);
                            done[tile] = true;
                        }
                    }
                }
            }

            return candidates;
        }

        public int GetNextMove(Board state)
        {
            var candidates = GetAllMoves(state);

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
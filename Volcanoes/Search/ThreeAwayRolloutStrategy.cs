using System;
using System.Collections.Generic;
using System.Linq;
using Volcano.Game;

namespace Volcano.Search
{
    internal class ThreeAwayRolloutStrategy : IRolloutStrategy
    {
        public ThreeAwayRolloutStrategy()
        {
        }

        public bool UseStrategy()
        {
            return Random.Shared.NextDouble() < 0.5;
        }

        public List<int> GetAllMoves(Board state)
        {
            var all = state.GetMoves();

            var scores = new int[80];
            for (var i = 0; i < 80; i++)
            {
                foreach (var m in Constants.ThreeAway[i])
                {
                    // my tiles
                    if (state.Tiles[m] > 0 && state.Player == Player.One)
                    {
                        scores[m]++;
                    }
                    else if (state.Tiles[m] < 0 && state.Player == Player.Two)
                    {
                        scores[m]++;
                    }

                    // opponent tiles
                    if (state.Tiles[m] < 0 && state.Player == Player.One)
                    {
                        scores[m]++;
                    }
                    else if (state.Tiles[m] > 0 && state.Player == Player.Two)
                    {
                        scores[m]++;
                    }
                }
            }

            var candidates = all
                .Where(x => scores[x] > 0)
                .OrderByDescending(x => scores[x])
                .ToList();

            return candidates.Count > 0 ? candidates : all;
        }

        public int GetNextMove(Board state)
        {
            var candidates = GetAllMoves(state);
            return candidates[Random.Shared.Next(candidates.Count)];
        }
    }
}
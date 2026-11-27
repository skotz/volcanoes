using System;
using System.Collections.Generic;
using Volcano.Game;

namespace Volcano.Search
{
    internal class PointNeighborRolloutStrategy : IRolloutStrategy
    {
        public PointNeighborRolloutStrategy()
        {
        }

        public bool UseStrategy()
        {
            return true;
        }

        public List<int> GetAllMoves(Board state)
        {
            var all = state.GetMoves();
            var candidates = new List<int>();
            var done = new bool[80];

            for (var i = 0; i < 80; i++)
            {
                if ((state.Tiles[i] < 0 && state.Player == Player.One) || (state.Tiles[i] > 0 && state.Player == Player.Two))
                {
                    foreach (var n in Constants.PointNeighbors[i])
                    {
                        if (!done[n] && state.Tiles[n] == 0)
                        {
                            candidates.Add(n);
                            done[n] = true;
                        }
                    }
                }
            }

            return candidates.Count > 0 ? candidates : all;
        }

        public int GetNextMove(Board state)
        {
            var candidates = GetAllMoves(state);
            return candidates[Random.Shared.Next(candidates.Count)];
        }
    }
}
using System;
using System.Collections.Generic;
using Volcano.Game;

namespace Volcano.Search
{
    internal class NearbyRolloutStrategy : IRolloutStrategy
    {
        private NearbyMoveFilter _filter = new NearbyMoveFilter();

        public List<int> GetAllMoves(Board state)
        {
            return _filter.GetMoves(state);
        }

        public int GetNextMove(Board state)
        {
            var moves = _filter.GetMoves(state);
            return moves[Random.Shared.Next(moves.Count)];
        }

        public bool UseStrategy()
        {
            return true;
        }
    }
}
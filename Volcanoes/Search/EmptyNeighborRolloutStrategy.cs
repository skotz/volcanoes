using System;
using System.Collections.Generic;
using Volcano.Game;

namespace Volcano.Search
{
    internal class EmptyNeighborRolloutStrategy : IRolloutStrategy
    {
        public EmptyNeighborRolloutStrategy()
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
            foreach (var move in all)
            {
                if (state.Tiles[move] == 0)
                {
                    foreach (var neighbor in Constants.AdjacentIndexes[move])
                    {
                        if (state.Tiles[neighbor] != 0)
                        {
                            candidates.Add(move);
                            break;
                        }
                    }
                }
                else if (state.Tiles[move] == 3 * (state.Player == Player.One ? 1 : -1))
                {
                    if (state.GetMoveTypeForTurn(state.Turn + 1) != MoveType.AllGrow)
                    {
                        candidates.Add(move);
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
using System.Collections.Generic;
using Volcano.Game;

namespace Volcano.Search
{
    internal interface IRolloutStrategy
    {
        bool UseStrategy();
        int GetNextMove(Board state);
        List<int> GetAllMoves(Board state);
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Volcano.Game;

namespace Volcano.Search
{
    internal interface IMoveFilter
    {
        List<int> GetMoves(Board state);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Search
{
    class UnrestrictedPathFinder : PathFinder
    {
        protected override bool IsTraversableTile(Board state, Player player, int tileIndex)
        {
            return true;
        }
    }
}

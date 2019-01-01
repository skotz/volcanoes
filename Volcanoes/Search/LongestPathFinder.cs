using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Search
{
    class LongestPathFinder : PathFinder
    {
        protected override bool IsTraversableTile(Board state, Player player, int tileIndex)
        {
            // Ignore tiles that aren't the player's
            return (state.Tiles[tileIndex] > 0 && player == Player.One) || (state.Tiles[tileIndex] < 0 && player == Player.Two);
        }
    }
}

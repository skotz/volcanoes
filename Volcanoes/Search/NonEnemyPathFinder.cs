using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Search
{
    class NonEnemyPathFinder : PathFinder
    {
        protected override bool IsTraversableTile(Board state, Player player, int tileIndex)
        {
            // Ignore tiles that aren't the player's or empty
            if (state.Tiles[tileIndex].Owner != player && state.Tiles[tileIndex].Owner != Player.Empty)
            {
                return false;
            }

            return true;
        }
    }
}

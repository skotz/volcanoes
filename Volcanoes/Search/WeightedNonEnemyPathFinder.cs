using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Search
{
    class WeightedNonEnemyPathFinder : PathFinder
    {
        protected override bool IsTraversableTile(Board state, Player player, int tileIndex)
        {
            // Ignore enemy tiles
            if (state.Tiles[tileIndex].Owner != player && state.Tiles[tileIndex].Owner != Player.Empty)
            {
                return false;
            }

            return true;
        }

        protected override int GetDistance(Board state, int first, int second)
        {
            // Friendly volcano tiles are a free pass
            if (state.Tiles[first].Value > VolcanoGame.Settings.MaxMagmaChamberLevel && state.Tiles[second].Value > VolcanoGame.Settings.MaxMagmaChamberLevel)
            {
                return 0;
            }
            
            // Friendly magma chambers are cheap
            if (state.Tiles[first].Value >= 1 && state.Tiles[second].Value >= 1)
            {
                return 0;
            }

            // Empty tiles are expensive
            return 1;
        }
    }
}

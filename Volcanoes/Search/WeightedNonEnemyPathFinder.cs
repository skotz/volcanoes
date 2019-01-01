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
            if (((state.Tiles[tileIndex] > 0 && player != Player.One) || (state.Tiles[tileIndex] < 0 && player != Player.Two)) && state.Tiles[tileIndex] != 0)
            {
                return false;
            }

            return true;
        }

        protected override int GetDistance(Board state, int first, int second)
        {
            // Friendly volcano tiles are a free pass
            if (Math.Abs(state.Tiles[first]) > VolcanoGame.Settings.MaxMagmaChamberLevel && Math.Abs(state.Tiles[second]) > VolcanoGame.Settings.MaxMagmaChamberLevel)
            {
                return 0;
            }
            
            // Friendly magma chambers are cheap
            if (Math.Abs(state.Tiles[first]) >= 1 && Math.Abs(state.Tiles[second]) >= 1)
            {
                return 0;
            }

            // Empty tiles are expensive
            return 1;
        }
    }
}

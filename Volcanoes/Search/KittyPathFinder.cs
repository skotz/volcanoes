using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Search
{
    class KittyPathFinder : PathFinder
    {
        private bool _playerOnly;

        public KittyPathFinder(bool playerOnly)
        {
            _playerOnly = playerOnly;
        }

        protected override bool IsTraversableTile(Board state, Player player, int tileIndex)
        {
            return !_playerOnly || state.Tiles[tileIndex].Owner == player;
        }

        protected override int[] GetNeighborTiles(Board state, int tileIndex)
        {
            return Constants.KittyCornerTiles[tileIndex];
        }
    }
}

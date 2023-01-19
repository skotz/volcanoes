using Volcano.Game;

namespace Volcano.Search
{
    internal class BeelinePathFinder : PathFinder
    {
        protected override bool IsTraversableTile(Board state, Player player, int tileIndex)
        {
            if (state.Tiles[tileIndex] == 0)
            {
                return true;
            }
            else if (state.Tiles[tileIndex] > 0 && player == Player.One)
            {
                return true;
            }
            else if (state.Tiles[tileIndex] < 0 && player == Player.Two)
            {
                return true;
            }

            return false;
        }

        protected override int GetDistance(Board state, int first, int second)
        {
            if (state.Tiles[second] == 0)
            {
                return 100;
            }
            else
            {
                return 1;
            }
        }
    }
}
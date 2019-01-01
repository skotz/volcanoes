using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Game
{
    class Tile
    {
        public Player Owner;
        public int Value;
        
        public Tile()
        {
        }

        public Tile(Tile copy)
        {
            Owner = copy.Owner;
            Value = copy.Value;
        }

        public static Player GetPlayer(int value)
        {
            if (value > 0)
            {
                return Player.One;
            }
            else if (value < 0)
            {
                return Player.Two;
            }
            else
            {
                return Player.Empty;
            }
        }
    }
}

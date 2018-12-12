using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Game
{
    class Tile
    {
        public Player Owner { get; set; }
        public int Value { get; set; }
        public int Index { get; set; }

        public string Name
        {
            get
            {
                return Constants.TileNames[Index];
            }
        }

        public int[] AdjacentIndexes
        {
            get
            {
                return Constants.ConnectingTiles[Index];
            }
        }

        public int Antipodes
        {
            get
            {
                return Constants.Antipodes[Index];
            }
        }

        public Tile()
        {
        }

        public Tile(Tile copy)
        {
            Owner = copy.Owner;
            Value = copy.Value;
            Index = copy.Index;
        }        
    }
}

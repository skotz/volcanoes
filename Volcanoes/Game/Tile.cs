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
        public int Index;

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

        public int[] KittyCornerIndexes
        {
            get
            {
                return Constants.KittyCornerTiles[Index];
            }
        }

        public int Antipode
        {
            get
            {
                return Constants.Antipodes[Index];
            }
        }

        public TileType Type
        {
            get
            {
                if (Value <= 0)
                {
                    return TileType.Empty;
                }
                else if (Value <= VolcanoGame.Settings.MaxMagmaChamberLevel)
                {
                    return TileType.MagmaChamber;
                }
                else
                {
                    return TileType.Volcano;
                }
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

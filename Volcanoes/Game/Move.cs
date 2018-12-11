using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Game
{
    class Move
    {
        public int TileIndex { get; set; }
        public MoveType MoveType { get; set; }
        
        public Move(int index, MoveType type)
        {
            TileIndex = index;
            MoveType = type;
        }

        public static bool operator ==(Move a, Move b)
        {
            return a.TileIndex == b.TileIndex && a.MoveType == b.MoveType;
        }
        
        public static bool operator !=(Move a, Move b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (Move)obj;

            return TileIndex == other.TileIndex && MoveType == other.MoveType;
        }

        public override int GetHashCode()
        {
            return TileIndex.GetHashCode() ^ MoveType.GetHashCode();
        }
    }
}

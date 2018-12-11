using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Interface
{
    class GameGraphicsSettings
    {
        public int TileSize { get; set; }
        public int TileSpacing { get; set; }

        public int TileWidth { get { return TileSize; } }
        public int TileHeight { get { return (int)(TileWidth * Math.Sqrt(3) / 2); } }
        public int TileHorizontalSpacing { get { return (int)(TileSpacing * Math.Sqrt(3) / 2); } }

        public int FontSize { get; set; }

        public Color BlueColor = Color.Blue;

        public Color OrangeColor = Color.Orange;

        public GameGraphicsSettings(int tileSize, int tileSpacing, int fontSize)
        {
            TileSize = tileSize;
            TileSpacing = tileSpacing;
            FontSize = fontSize;
        }
    }
}

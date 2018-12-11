using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Interface
{
    class GameTile
    {
        public Point Location { get; set; }
        public bool Upright { get; set; }
        public Rectangle BoundingBox { get; set; }
        public GraphicsPath Path { get; set; }
    }
}

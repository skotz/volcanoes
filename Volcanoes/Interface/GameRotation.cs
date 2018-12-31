using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Interface
{
    class GameRotation
    {
        public Point Location { get; set; }
        public Rectangle BoundingBox { get; set; }
        public GraphicsPath Path { get; set; }
        public int[][] RotationLoops { get; set; }
        public bool Clockwise { get; set; }
        public Bitmap Image { get; set; }

        public Rectangle SmallerBox
        {
            get
            {
                int pixels = 2;
                return new Rectangle(BoundingBox.X + pixels, BoundingBox.Y + pixels, Math.Max(2, BoundingBox.Width - pixels * 2), Math.Max(2, BoundingBox.Height - pixels * 2));
            }
        }
        
        public bool IsWithinCircle(Point point)
        {
            int radius = BoundingBox.Width / 2;
            Point center = new Point(Location.X + radius, Location.Y + radius);
            double distance = Math.Sqrt(Math.Pow(point.X - center.X, 2) + Math.Pow(point.Y - center.Y, 2));

            return distance <= radius;
        }
    }
}

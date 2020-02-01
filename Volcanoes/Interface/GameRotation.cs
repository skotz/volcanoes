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
        public PointF Location { get; set; }
        public RectangleF BoundingBox { get; set; }
        public Rectangle BoundingBoxFull
        {
            get
            {
                return new Rectangle((int)BoundingBox.Location.X, (int)BoundingBox.Location.Y, (int)BoundingBox.Size.Width, (int)BoundingBox.Size.Height);
            }
        }
        public GraphicsPath Path { get; set; }
        public int[][] RotationLoops { get; set; }
        public bool Clockwise { get; set; }
        public Bitmap Image { get; set; }

        public RectangleF SmallerBox
        {
            get
            {
                int pixels = 2;
                return new RectangleF(BoundingBox.X + pixels, BoundingBox.Y + pixels, Math.Max(2, BoundingBox.Width - pixels * 2), Math.Max(2, BoundingBox.Height - pixels * 2));
            }
        }
        
        public bool IsWithinCircle(Point point)
        {
            var radius = BoundingBox.Width / 2;
            var center = new PointF(Location.X + radius, Location.Y + radius);
            double distance = Math.Sqrt(Math.Pow(point.X - center.X, 2) + Math.Pow(point.Y - center.Y, 2));

            return distance <= radius;
        }
    }
}

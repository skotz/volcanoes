using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Volcano.Game;

namespace Volcano.Interface
{
    class GameGraphics
    {
        private Panel _panel;
        private GameGraphicsSettings _settings;
        private List<GameTile> _tiles;

        public GameGraphics(Panel panel, GameGraphicsSettings settings)
        {
            _panel = panel;
            _settings = settings;

            InitializeTiles();
        }

        private void InitializeTiles()
        {
            _tiles = new List<GameTile>();

            for (int outer = 0; outer < 20; outer++)
            {
                int outerRow = outer / 5;
                int outerCol = outer % 5;

                for (int inner = 0; inner < 4; inner++)
                {
                    // Details of outer triangle
                    int x = outerCol * (_settings.TileWidth * 2 + _settings.TileHorizontalSpacing * 4) + _settings.TileSpacing;
                    int y = outerRow * (_settings.TileHeight * 2 + _settings.TileSpacing * 2 + _settings.TileHorizontalSpacing) + _settings.TileHorizontalSpacing;
                    bool upright = outerRow % 2 == 0;
                    string name = (outer + 1).ToString();
                    if (outerRow >= 2)
                    {
                        x += _settings.TileWidth + _settings.TileHorizontalSpacing * 2;
                        y -= _settings.TileHeight * 2 + _settings.TileSpacing * 2;
                    }

                    // Adjust to settings of inner triangle
                    if (upright)
                    {
                        switch (inner)
                        {
                            case 0:
                                x += _settings.TileWidth / 2 + _settings.TileHorizontalSpacing;
                                y += _settings.TileHeight + _settings.TileSpacing;
                                upright = !upright;
                                name += "A";
                                break;
                            case 1:
                                x += _settings.TileWidth / 2 + _settings.TileHorizontalSpacing;
                                name += "B";
                                break;
                            case 2:
                                x += _settings.TileWidth + _settings.TileHorizontalSpacing * 2;
                                y += _settings.TileHeight + _settings.TileSpacing + _settings.TileHorizontalSpacing;
                                name += "C";
                                break;
                            case 3:
                                y += _settings.TileHeight + _settings.TileSpacing + _settings.TileHorizontalSpacing;
                                name += "D";
                                break;
                        }
                    }
                    else
                    {
                        switch (inner)
                        {
                            case 0:
                                x += _settings.TileWidth / 2 + _settings.TileHorizontalSpacing;
                                y += _settings.TileHorizontalSpacing;
                                upright = !upright;
                                name += "A";
                                break;
                            case 1:
                                x += _settings.TileWidth / 2 + _settings.TileHorizontalSpacing;
                                y += _settings.TileHeight + _settings.TileSpacing + _settings.TileHorizontalSpacing;
                                name += "B";
                                break;
                            case 2:
                                name += "C";
                                break;
                            case 3:
                                x += _settings.TileWidth + _settings.TileHorizontalSpacing * 2;
                                name += "D";
                                break;
                        }
                    }
                    
                    PointF[] points;
                    if (upright)
                    {
                        points = new PointF[] {
                            new PointF(x + _settings.TileWidth / 2, y),
                            new PointF(x, y + _settings.TileHeight),
                            new PointF(x + _settings.TileWidth, y + _settings.TileHeight)
                        };
                    }
                    else
                    {
                        points = new PointF[] {
                            new PointF(x + _settings.TileWidth / 2, y + _settings.TileHeight),
                            new PointF(x, y),
                            new PointF(x + _settings.TileWidth, y)
                        };
                    }

                    var path = new GraphicsPath();
                    path.AddLines(points);

                    _tiles.Add(new GameTile
                    {
                        Location = new Point(x, y),
                        Name = name,
                        Upright = upright,
                        BoundingBox = new Rectangle(x, y, _settings.TileWidth, _settings.TileHeight),
                        Path = path
                    });
                }
            }
        }

        public void Draw(Board gameState)
        {
            PointF mouseLocation = _panel.PointToClient(Cursor.Position);
            int hoverTile = GetIndexFromLocation(mouseLocation);

            using (Bitmap b = new Bitmap(_panel.Width, _panel.Height))
            using (Graphics g = Graphics.FromImage(b))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                g.Clear(Color.White);

                for (int i = 0; i < 80; i++)
                {
                    DrawTile(g, i, hoverTile);
                    DrawTileText(g, i, gameState.Tiles[i].Index.ToString());
                }

                using (Graphics game = _panel.CreateGraphics())
                {
                    game.DrawImage(b, 0, 0, _panel.Width, _panel.Height);
                }
            }
        }

        private void DrawTile(Graphics g, int index, int hoverIndex)
        {
            Brush brush = Brushes.Red;
            if (index == hoverIndex)
            {
                brush = Brushes.Blue;
            }

            // TODO: get this data in a better way
            if (hoverIndex >= 0)
            {
                try
                {
                    if (Constants.ConnectingTiles[hoverIndex].Any(x => x == index))
                    {
                        brush = Brushes.LightBlue;
                    }
                }
                catch { }
            }

            g.FillPath(brush, _tiles[index].Path);
        }

        private int GetIndexFromLocation(PointF location)
        {
            for (int i = 0; i < 80; i++)
            {
                if (_tiles[i].Path.IsVisible(location))
                {
                    return i;
                }
            }

            return -1;
        }

        private void DrawTileText(Graphics g, int index, string text)
        {
            int triangleAdjust = (_tiles[index].Upright ? 1 : -1) * _settings.TileHeight / 6;
            Font font = new Font("Tahoma", _settings.FontSize, FontStyle.Bold);
            SizeF size = g.MeasureString(text, font);
            Brush brush = new SolidBrush(Color.FromArgb(128, 255, 255, 255));
            RectangleF location = new RectangleF(new PointF(_tiles[index].BoundingBox.X + _tiles[index].BoundingBox.Width / 2 - size.Width / 2, _tiles[index].BoundingBox.Y + _tiles[index].BoundingBox.Height / 2 - size.Height / 2 + triangleAdjust), size);

            g.DrawString(text, font, brush, location);
        }
    }
}

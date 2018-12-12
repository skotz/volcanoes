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
                                break;
                            case 1:
                                x += _settings.TileWidth / 2 + _settings.TileHorizontalSpacing;
                                break;
                            case 2:
                                x += _settings.TileWidth + _settings.TileHorizontalSpacing * 2;
                                y += _settings.TileHeight + _settings.TileSpacing + _settings.TileHorizontalSpacing;
                                break;
                            case 3:
                                y += _settings.TileHeight + _settings.TileSpacing + _settings.TileHorizontalSpacing;
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
                                break;
                            case 1:
                                x += _settings.TileWidth / 2 + _settings.TileHorizontalSpacing;
                                y += _settings.TileHeight + _settings.TileSpacing + _settings.TileHorizontalSpacing;
                                break;
                            case 2:
                                break;
                            case 3:
                                x += _settings.TileWidth + _settings.TileHorizontalSpacing * 2;
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
                        Upright = upright,
                        BoundingBox = new Rectangle(x, y, _settings.TileWidth, _settings.TileHeight),
                        Path = path
                    });
                }
            }
        }
        
        public void Draw(Board gameState, Point mouseLocation)
        {
            int hoverTile = GetTileIndex(mouseLocation);

            using (Bitmap b = new Bitmap(_panel.Width, _panel.Height))
            using (Graphics g = Graphics.FromImage(b))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                g.Clear(Color.White);

                for (int i = 0; i < 80; i++)
                {
                    DrawTile(g, gameState, i, hoverTile);

                    if (gameState.Tiles[i].Owner == Player.Empty)
                    {
                        DrawTileText(g, i, gameState.Tiles[i].Name);
                    }
                    else
                    {
                        DrawTileText(g, i, gameState.Tiles[i].Value.ToString());
                    }
                }

                // TODO: draw points between triangle groups

                Color playerColor = gameState.Player == Player.Blue ? _settings.BlueColor : _settings.OrangeColor;
                g.DrawString("Turn " + gameState.Turn, new Font("Tahoma", 12f, FontStyle.Bold), new SolidBrush(playerColor), new Point(0, 0));

                using (Graphics game = _panel.CreateGraphics())
                {
                    game.DrawImage(b, 0, 0, _panel.Width, _panel.Height);
                }
            }
        }

        private void DrawTile(Graphics g, Board gameState, int index, int hoverIndex)
        {
            Color tileColor = Color.Gray;

            if (gameState.Tiles[index].Owner == Player.Blue)
            {
                tileColor = _settings.BlueColor;
            }
            else if (gameState.Tiles[index].Owner == Player.Orange)
            {
                tileColor = _settings.OrangeColor;
            }

            if (hoverIndex >= 0)
            {
                // Tile under the mouse pointer
                if (index == hoverIndex)
                {
                    tileColor = Color.FromArgb(128, tileColor);
                }
            }

            Brush brush = new SolidBrush(tileColor);

            g.FillPath(brush, _tiles[index].Path);

            if (hoverIndex >= 0)
            {
                // Tiles directly adjacent to the tile under the mouse pointer
                if (Constants.ConnectingTiles[hoverIndex].Any(x => x == index))
                {
                    Pen pen = new Pen(Color.Black, _settings.TileHorizontalSpacing);
                    g.DrawPolygon(pen, _tiles[index].Path.PathPoints);
                }

                // Tile on the opposite side of the board
                if (gameState.Tiles[hoverIndex].Antipodes == index)
                {
                    Pen pen = new Pen(Color.Lime, _settings.TileHorizontalSpacing);
                    g.DrawPolygon(pen, _tiles[index].Path.PathPoints);
                }
            }
        }

        public int GetTileIndex(Point location)
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

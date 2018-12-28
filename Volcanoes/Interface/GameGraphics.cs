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
        private List<GameTile> _tiles;

        public GameGraphicsSettings GraphicsSettings { get; set; }

        public GameGraphics(Panel panel, GameGraphicsSettings settings)
        {
            _panel = panel;
            GraphicsSettings = settings;

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
                    int x = outerCol * (GraphicsSettings.TileWidth * 2 + GraphicsSettings.TileHorizontalSpacing * 4) + GraphicsSettings.TileSpacing + GraphicsSettings.BoardSpacing;
                    int y = outerRow * (GraphicsSettings.TileHeight * 2 + GraphicsSettings.TileSpacing * 2 + GraphicsSettings.TileHorizontalSpacing) + GraphicsSettings.TileHorizontalSpacing + GraphicsSettings.BoardSpacing;
                    bool upright = outerRow % 2 == 0;
                    if (outerRow >= 2)
                    {
                        x += GraphicsSettings.TileWidth + GraphicsSettings.TileHorizontalSpacing * 2;
                        y -= GraphicsSettings.TileHeight * 2 + GraphicsSettings.TileSpacing * 2;
                    }

                    // Adjust to settings of inner triangle
                    if (upright)
                    {
                        switch (inner)
                        {
                            case 0:
                                x += GraphicsSettings.TileWidth / 2 + GraphicsSettings.TileHorizontalSpacing;
                                y += GraphicsSettings.TileHeight + GraphicsSettings.TileSpacing;
                                upright = !upright;
                                break;
                            case 1:
                                x += GraphicsSettings.TileWidth / 2 + GraphicsSettings.TileHorizontalSpacing;
                                break;
                            case 2:
                                x += GraphicsSettings.TileWidth + GraphicsSettings.TileHorizontalSpacing * 2;
                                y += GraphicsSettings.TileHeight + GraphicsSettings.TileSpacing + GraphicsSettings.TileHorizontalSpacing;
                                break;
                            case 3:
                                y += GraphicsSettings.TileHeight + GraphicsSettings.TileSpacing + GraphicsSettings.TileHorizontalSpacing;
                                break;
                        }
                    }
                    else
                    {
                        switch (inner)
                        {
                            case 0:
                                x += GraphicsSettings.TileWidth / 2 + GraphicsSettings.TileHorizontalSpacing;
                                y += GraphicsSettings.TileHorizontalSpacing;
                                upright = !upright;
                                break;
                            case 1:
                                x += GraphicsSettings.TileWidth / 2 + GraphicsSettings.TileHorizontalSpacing;
                                y += GraphicsSettings.TileHeight + GraphicsSettings.TileSpacing + GraphicsSettings.TileHorizontalSpacing;
                                break;
                            case 2:
                                break;
                            case 3:
                                x += GraphicsSettings.TileWidth + GraphicsSettings.TileHorizontalSpacing * 2;
                                break;
                        }
                    }

                    PointF[] points;
                    if (upright)
                    {
                        points = new PointF[] {
                            new PointF(x + GraphicsSettings.TileWidth / 2, y),
                            new PointF(x, y + GraphicsSettings.TileHeight),
                            new PointF(x + GraphicsSettings.TileWidth, y + GraphicsSettings.TileHeight)
                        };
                    }
                    else
                    {
                        points = new PointF[] {
                            new PointF(x + GraphicsSettings.TileWidth / 2, y + GraphicsSettings.TileHeight),
                            new PointF(x, y),
                            new PointF(x + GraphicsSettings.TileWidth, y)
                        };
                    }

                    var path = new GraphicsPath();
                    path.AddLines(points);

                    _tiles.Add(new GameTile
                    {
                        Location = new Point(x, y),
                        Upright = upright,
                        BoundingBox = new Rectangle(x, y, GraphicsSettings.TileWidth, GraphicsSettings.TileHeight),
                        Path = path
                    });
                }
            }
        }

        public void Resize()
        {
            GraphicsSettings.UpdateBestTileWidth(_panel.Size);
            InitializeTiles();
        }

        public void Draw(VolcanoGame game, Point mouseLocation, int moveNumber, bool highlightLastMove)
        {
            Resize();

            if (GraphicsSettings.IdealPanelWidth <= 100 || GraphicsSettings.IdealPanelHeight <= 20)
            {
                using (Graphics g3 = _panel.CreateGraphics())
                {
                    g3.Clear(SystemColors.ControlDark);
                    g3.DrawString(":)", new Font("Tahoma", 8f, FontStyle.Regular), Brushes.White, new Point(1, 1));
                }

                return;
            }

            bool reviewMode = game.MoveHistory.Count > 0 && moveNumber != game.MoveHistory.Count - 1;

            int hoverTile = GetTileIndex(mouseLocation);
            Board gameState = reviewMode ? game.GetPreviousState(moveNumber) : game.CurrentState;

            int lastPlayIndex = game.MoveHistory.Count >= moveNumber && moveNumber - 1 >= 0 ? game.MoveHistory[moveNumber - 1] : -1;

            Color background = reviewMode ? GraphicsSettings.ReviewBackgroundColor : GraphicsSettings.BackgroundColor;

            using (Bitmap b = new Bitmap(GraphicsSettings.IdealPanelWidth, GraphicsSettings.IdealPanelHeight))
            using (Graphics g = Graphics.FromImage(b))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;

                g.Clear(background);

                for (int i = 0; i < 80; i++)
                {
                    DrawTile(g, gameState, i, hoverTile, lastPlayIndex, highlightLastMove);

                    if (gameState.Tiles[i].Owner == Player.Empty)
                    {
                        if (GraphicsSettings.ShowTileNames)
                        {
                            DrawTileCenterText(g, i, gameState.Tiles[i].Name);
                        }
                        else if (GraphicsSettings.ShowTileIndexes)
                        {
                            DrawTileMainText(g, i, gameState.Tiles[i].Index.ToString());
                        }

                        //DrawTileMainText(g, i, Constants.FastestPaths[i].Length.ToString());
                    }
                    else
                    {
                        int value = gameState.Tiles[i].Value;

                        if (gameState.Tiles[i].Type == TileType.MagmaChamber)
                        {
                            DrawTileSubText(g, i, "Chamber");
                        }
                        else
                        {
                            value -= VolcanoGame.Settings.MaxMagmaChamberLevel;
                            DrawTileSubText(g, i, "Volcano");
                        }

                        DrawTileMainText(g, i, value.ToString());
                    }
                }

                using (Bitmap b2 = new Bitmap(_panel.Width, _panel.Height))
                using (Graphics g2 = Graphics.FromImage(b2))
                using (Graphics g3 = _panel.CreateGraphics())
                {
                    g2.Clear(background);
                    g2.DrawImage(b, (_panel.Width - GraphicsSettings.IdealPanelWidth) / 2, (_panel.Height - GraphicsSettings.IdealPanelHeight) / 2, GraphicsSettings.IdealPanelWidth, GraphicsSettings.IdealPanelHeight);

                    Color playerColor = gameState.Player == Player.One ? GraphicsSettings.PlayerOneVolcanoTileColor : GraphicsSettings.PlayerTwoVolcanoTileColor;
                    g2.DrawString("Turn " + gameState.Turn, new Font("Tahoma", 12f, FontStyle.Bold), new SolidBrush(playerColor), new Point(5, 5));
                    if (gameState.State == GameState.GameOver)
                    {
                        playerColor = gameState.Winner == Player.One ? GraphicsSettings.PlayerOneVolcanoTileColor : GraphicsSettings.PlayerTwoVolcanoTileColor;
                        string gameOver = "Game Over!";
                        Font f = new Font("Tahoma", 12f, FontStyle.Bold);
                        SizeF size = g.MeasureString(gameOver, f);
                        g2.DrawString(gameOver, f, new SolidBrush(playerColor), new Point(_panel.Width - (int)size.Width - 5, 5));
                    }
                    g2.DrawString(game.NodesPerSecond.ToString() + " NPS", new Font("Tahoma", 12f, FontStyle.Bold), Brushes.Gray, new Point(5, _panel.Height - 25));

                    // Double buffering
                    g3.DrawImage(b2, 0, 0, _panel.Width, _panel.Height);
                }
            }
        }

        private void DrawTile(Graphics g, Board gameState, int index, int hoverIndex, int lastPlayIndex, bool highlightLastMove)
        {
            Color tileColor = GraphicsSettings.EmptyTileColor;

            if (gameState.Tiles[index].Owner == Player.One)
            {
                tileColor = gameState.Tiles[index].Type == TileType.Volcano ? GraphicsSettings.PlayerOneVolcanoTileColor : GraphicsSettings.PlayerOneMagmaChamberTileColor;
            }
            else if (gameState.Tiles[index].Owner == Player.Two)
            {
                tileColor = gameState.Tiles[index].Type == TileType.Volcano ? GraphicsSettings.PlayerTwoVolcanoTileColor : GraphicsSettings.PlayerTwoMagmaChamberTileColor;
            }

            // Winning path
            if (gameState.WinningPath.Count > 0 && !gameState.WinningPath.Contains(index))
            {
                tileColor = Color.FromArgb(64, tileColor);
            }

            //// Tile on the shortest unhindered path to the antipode
            //if (hoverIndex >= 0 && Constants.FastestPaths[hoverIndex].Any(x => x == index))
            //{
            //    tileColor = Color.LightBlue;
            //}

            Brush brush = new SolidBrush(tileColor);

            g.FillPath(brush, _tiles[index].Path);

            //// Recently erupted volcano
            //if (gameState.Eruptions.Contains(index))
            //{
            //    Pen pen = new Pen(_settings.RecentEruptionTileBorderColor, _settings.TileHorizontalSpacing);
            //    g.DrawPolygon(pen, _tiles[index].Path.PathPoints);
            //}

            if (highlightLastMove)
            {
                // Tile most recently played on
                if (lastPlayIndex == index || (lastPlayIndex == 80 && gameState.Tiles[index].Value > 0))
                {
                    Pen pen = new Pen(GraphicsSettings.LastPlayedTileBorderColor, GraphicsSettings.TileHorizontalSpacing);
                    g.DrawPolygon(pen, _tiles[index].Path.PathPoints);
                }
            }

            if (hoverIndex >= 0)
            {
                // Tile under the mouse pointer
                if (index == hoverIndex)
                {
                    Pen pen = new Pen(GraphicsSettings.HoverTileBorderColor, GraphicsSettings.TileHorizontalSpacing);
                    g.DrawPolygon(pen, _tiles[index].Path.PathPoints);
                }

                // Tiles directly adjacent to the tile under the mouse pointer
                if (Constants.ConnectingTiles[hoverIndex].Any(x => x == index))
                {
                    Pen pen = new Pen(GraphicsSettings.HoverAdjacentTileBorderColor, GraphicsSettings.TileHorizontalSpacing);
                    g.DrawPolygon(pen, _tiles[index].Path.PathPoints);
                }

                //// Tiles kitty corner to the tile under the pointer
                //if (Constants.KittyCornerTiles[hoverIndex].Any(x => x == index))
                //{
                //    Pen pen = new Pen(Color.Red, _settings.TileHorizontalSpacing);
                //    g.DrawPolygon(pen, _tiles[index].Path.PathPoints);
                //}

                // Tile on the opposite side of the board
                if (gameState.Tiles[hoverIndex].Antipode == index)
                {
                    Pen pen = new Pen(GraphicsSettings.HoverAntipodeTileBorderColor, GraphicsSettings.TileHorizontalSpacing);
                    g.DrawPolygon(pen, _tiles[index].Path.PathPoints);
                }
            }
        }

        private Point GetOffsetPoint(int x, int y)
        {
            return new Point(x - (_panel.Width - GraphicsSettings.IdealPanelWidth) / 2, y - (_panel.Height - GraphicsSettings.IdealPanelHeight) / 2);
        }

        private Point GetOffsetPoint(Point point)
        {
            return GetOffsetPoint(point.X, point.Y);
        }

        public int GetTileIndex(Point location)
        {
            Point offset = GetOffsetPoint(location);

            for (int i = 0; i < 80; i++)
            {
                if (_tiles[i].Path.IsVisible(offset))
                {
                    return i;
                }
            }

            return -1;
        }

        private void DrawTileCenterText(Graphics g, int index, string text)
        {
            DrawTileText(g, index, text, 4, GraphicsSettings.MainFontSize, true);
        }

        private void DrawTileMainText(Graphics g, int index, string text)
        {
            DrawTileText(g, index, text, 600, GraphicsSettings.MainFontSize, true);
        }

        private void DrawTileSubText(Graphics g, int index, string text)
        {
            DrawTileText(g, index, text, 3, GraphicsSettings.SubTextFontSize, false);
        }

        private void DrawTileText(Graphics g, int index, string text, int heightOffset, int fontSize, bool bold)
        {
            int triangleAdjust = (_tiles[index].Upright ? 1 : -1) * GraphicsSettings.TileHeight / heightOffset;
            Font font = new Font("Tahoma", fontSize, bold ? FontStyle.Bold : FontStyle.Regular);
            SizeF size = g.MeasureString(text, font);
            Brush brush = new SolidBrush(Color.FromArgb(128, 255, 255, 255));
            RectangleF location = new RectangleF(new PointF(_tiles[index].BoundingBox.X + _tiles[index].BoundingBox.Width / 2 - size.Width / 2, _tiles[index].BoundingBox.Y + _tiles[index].BoundingBox.Height / 2 - size.Height / 2 + triangleAdjust), size);

            g.DrawString(text, font, brush, location);
        }
    }
}

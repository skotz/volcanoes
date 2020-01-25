using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using Volcano.Game;

namespace Volcano.Interface
{
    internal class GameGraphics
    {
        private Panel _panel;
        private Size _size;
        private List<GameTile> _tiles;
        private List<GameRotation> _rotations;
        private Rectangle _clock;

        private int[] boardIndexFromTileIndex;

        public GameGraphicsSettings GraphicsSettings { get; set; }

        public GameGraphics(Panel panel, GameGraphicsSettings settings)
        {
            _panel = panel;
            _size = new Size(0, 0);
            GraphicsSettings = settings;

            InitializeTiles();
            InitializeRotations();
            InitializeRedirects();
            InitializeHud();
        }

        private void InitializeRedirects()
        {
            boardIndexFromTileIndex = new int[80];
            for (int i = 0; i < 80; i++)
            {
                boardIndexFromTileIndex[i] = i;
            }
        }

        private void InitializeRotations()
        {
            // TODO: simplify this mess
            _rotations = new List<GameRotation>();

            int radius = _tiles[0].BoundingBox.Width / 5;
            int xOffset = _tiles[0].BoundingBox.Width + GraphicsSettings.TileHorizontalSpacing;
            int yOffset = _tiles[0].BoundingBox.Height * 2 / 3;
            int yOffsetInverted = _tiles[0].BoundingBox.Height - yOffset;

            // Rotate around 2/7/26/41/23 and 71/50/33/55/74
            int[][] rotationLoops = new int[16][];
            rotationLoops[0] = new int[] { 71, 50, 33, 55, 74 };
            rotationLoops[1] = new int[] { 68, 48, 32, 52, 72 };
            rotationLoops[2] = new int[] { 69, 51, 34, 53, 75 };
            rotationLoops[3] = new int[] { 70, 49, 35, 54, 73 };
            rotationLoops[4] = new int[] { 67, 31, 14, 37, 77 };
            rotationLoops[5] = new int[] { 64, 28, 12, 36, 76 };
            rotationLoops[6] = new int[] { 65, 29, 15, 38, 78 };
            rotationLoops[7] = new int[] { 66, 30, 13, 39, 79 };
            rotationLoops[8] = new int[] { 61, 46, 10, 19, 59 };
            rotationLoops[9] = new int[] { 60, 44, 8, 16, 56 };
            rotationLoops[10] = new int[] { 62, 47, 11, 17, 57 };
            rotationLoops[11] = new int[] { 63, 45, 9, 18, 58 };
            rotationLoops[12] = new int[] { 43, 25, 6, 1, 22 };
            rotationLoops[13] = new int[] { 21, 42, 27, 5, 3 };
            rotationLoops[14] = new int[] { 40, 24, 4, 0, 20 };
            rotationLoops[15] = new int[] { 41, 26, 7, 2, 23 };

            int x = _tiles[74].Location.X - GraphicsSettings.TileHorizontalSpacing;
            int y = _tiles[74].Location.Y + yOffset;
            _rotations.Add(new GameRotation
            {
                Location = new Point(x - radius, y - radius),
                BoundingBox = new Rectangle(x - radius, y - radius, radius * 2, radius * 2),
                RotationLoops = rotationLoops,
                Image = Properties.Resources.rotate_counter_clockwise
            });
            x = _tiles[2].Location.X + xOffset;
            y = _tiles[2].Location.Y + yOffsetInverted;
            _rotations.Add(new GameRotation
            {
                Location = new Point(x - radius, y - radius),
                BoundingBox = new Rectangle(x - radius, y - radius, radius * 2, radius * 2),
                RotationLoops = rotationLoops,
                Image = Properties.Resources.rotate_clockwise
            });

            // Rotate around 1/5/9/13/17 and 61/65/69/73/77
            int[][] rotationLoopsPoles = new int[16][];
            rotationLoopsPoles[0] = new int[] { 17, 13, 9, 5, 1 };
            rotationLoopsPoles[1] = new int[] { 16, 12, 8, 4, 0 };
            rotationLoopsPoles[2] = new int[] { 19, 15, 11, 7, 3 };
            rotationLoopsPoles[3] = new int[] { 18, 14, 10, 6, 2 };
            rotationLoopsPoles[4] = new int[] { 38, 34, 30, 26, 22 };
            rotationLoopsPoles[5] = new int[] { 39, 35, 31, 27, 23 };
            rotationLoopsPoles[6] = new int[] { 36, 32, 28, 24, 20 };
            rotationLoopsPoles[7] = new int[] { 57, 53, 49, 45, 41 };
            rotationLoopsPoles[8] = new int[] { 37, 33, 29, 25, 21 };
            rotationLoopsPoles[9] = new int[] { 56, 52, 48, 44, 40 };
            rotationLoopsPoles[10] = new int[] { 58, 54, 50, 46, 42 };
            rotationLoopsPoles[11] = new int[] { 59, 55, 51, 47, 43 };
            rotationLoopsPoles[12] = new int[] { 79, 75, 71, 67, 63 };
            rotationLoopsPoles[13] = new int[] { 78, 74, 70, 66, 62 };
            rotationLoopsPoles[14] = new int[] { 76, 72, 68, 64, 60 };
            rotationLoopsPoles[15] = new int[] { 77, 73, 69, 65, 61 };

            // Right arrow (rotate at poles)
            x = _tiles[57].Location.X + _tiles[57].BoundingBox.Width + GraphicsSettings.TileHorizontalSpacing * 2;
            y = _tiles[57].Location.Y + yOffsetInverted;
            _rotations.Add(new GameRotation
            {
                Location = new Point(x - radius, y - radius),
                BoundingBox = new Rectangle(x - radius, y - radius, radius * 2, radius * 2),
                RotationLoops = rotationLoopsPoles,
                Image = Properties.Resources.rotate_right
            });

            // The rest of the rotation loops can be generated as a combination of the previous loops
            int[][] rotationLoops2 = new int[16][];
            for (int i = 0; i < 16; i++)
            {
                rotationLoops2[i] = new int[5];
                for (int r = 0; r < 5; r++)
                {
                    // Copy a pole rotation
                    rotationLoops2[i][4 - r] = rotationLoopsPoles[i][r];

                    // Find a rotation and apply that too
                    for (int j = 0; j < 16; j++)
                    {
                        for (int k = 0; k < 5; k++)
                        {
                            if (rotationLoops2[i][4 - r] == rotationLoops[j][k])
                            {
                                rotationLoops2[i][4 - r] = rotationLoops[j][(k - 1 + 5) % 5];
                                j = 16;
                                k = 5;
                            }
                        }
                    }
                }
            }

            x = _tiles[78].Location.X - GraphicsSettings.TileHorizontalSpacing;
            y = _tiles[78].Location.Y + yOffset;
            _rotations.Add(new GameRotation
            {
                Location = new Point(x - radius, y - radius),
                BoundingBox = new Rectangle(x - radius, y - radius, radius * 2, radius * 2),
                RotationLoops = rotationLoops2,
                Image = Properties.Resources.rotate_counter_clockwise
            });
            x = _tiles[6].Location.X + xOffset;
            y = _tiles[6].Location.Y + yOffsetInverted;
            _rotations.Add(new GameRotation
            {
                Location = new Point(x - radius, y - radius),
                BoundingBox = new Rectangle(x - radius, y - radius, radius * 2, radius * 2),
                RotationLoops = rotationLoops2,
                Image = Properties.Resources.rotate_clockwise
            });

            int[][] rotationLoop3 = new int[16][];
            for (int i = 0; i < 16; i++)
            {
                rotationLoop3[i] = new int[5];
                for (int r = 0; r < 5; r++)
                {
                    // Copy a pole rotation
                    rotationLoop3[i][4 - r] = rotationLoopsPoles[i][r];

                    // Find a rotation and apply that too
                    for (int j = 0; j < 16; j++)
                    {
                        for (int k = 0; k < 5; k++)
                        {
                            if (rotationLoop3[i][4 - r] == rotationLoops[j][k])
                            {
                                rotationLoop3[i][4 - r] = rotationLoops[j][(k - 1 + 5) % 5];
                                j = 16;
                                k = 5;
                            }
                        }
                    }

                    // Run the pole rotation again
                    for (int j = 0; j < 16; j++)
                    {
                        for (int k = 0; k < 5; k++)
                        {
                            if (rotationLoop3[i][4 - r] == rotationLoopsPoles[j][k])
                            {
                                rotationLoop3[i][4 - r] = rotationLoopsPoles[j][(k - 1 + 5) % 5];
                                j = 16;
                                k = 5;
                            }
                        }
                    }
                }
            }

            x = _tiles[62].Location.X - GraphicsSettings.TileHorizontalSpacing;
            y = _tiles[62].Location.Y + yOffset;
            _rotations.Add(new GameRotation
            {
                Location = new Point(x - radius, y - radius),
                BoundingBox = new Rectangle(x - radius, y - radius, radius * 2, radius * 2),
                RotationLoops = rotationLoop3,
                Image = Properties.Resources.rotate_counter_clockwise
            });
            x = _tiles[10].Location.X + xOffset;
            y = _tiles[10].Location.Y + yOffsetInverted;
            _rotations.Add(new GameRotation
            {
                Location = new Point(x - radius, y - radius),
                BoundingBox = new Rectangle(x - radius, y - radius, radius * 2, radius * 2),
                RotationLoops = rotationLoop3,
                Image = Properties.Resources.rotate_clockwise
            });

            int[][] rotationLoop4 = new int[16][];
            for (int i = 0; i < 16; i++)
            {
                rotationLoop4[i] = new int[5];
                for (int r = 0; r < 5; r++)
                {
                    // Copy a pole rotation
                    rotationLoop4[i][4 - r] = rotationLoopsPoles[i][r];

                    // Find a rotation and apply that too
                    for (int j = 0; j < 16; j++)
                    {
                        for (int k = 0; k < 5; k++)
                        {
                            if (rotationLoop4[i][4 - r] == rotationLoops[j][k])
                            {
                                rotationLoop4[i][4 - r] = rotationLoops[j][(k - 1 + 5) % 5];
                                j = 16;
                                k = 5;
                            }
                        }
                    }

                    // Run the pole rotation two more times
                    for (int c = 0; c < 2; c++)
                    {
                        for (int j = 0; j < 16; j++)
                        {
                            for (int k = 0; k < 5; k++)
                            {
                                if (rotationLoop4[i][4 - r] == rotationLoopsPoles[j][k])
                                {
                                    rotationLoop4[i][4 - r] = rotationLoopsPoles[j][(k - 1 + 5) % 5];
                                    j = 16;
                                    k = 5;
                                }
                            }
                        }
                    }
                }
            }

            x = _tiles[66].Location.X - GraphicsSettings.TileHorizontalSpacing;
            y = _tiles[66].Location.Y + yOffset;
            _rotations.Add(new GameRotation
            {
                Location = new Point(x - radius, y - radius),
                BoundingBox = new Rectangle(x - radius, y - radius, radius * 2, radius * 2),
                RotationLoops = rotationLoop4,
                Image = Properties.Resources.rotate_counter_clockwise
            });
            x = _tiles[14].Location.X + xOffset;
            y = _tiles[14].Location.Y + yOffsetInverted;
            _rotations.Add(new GameRotation
            {
                Location = new Point(x - radius, y - radius),
                BoundingBox = new Rectangle(x - radius, y - radius, radius * 2, radius * 2),
                RotationLoops = rotationLoop4,
                Image = Properties.Resources.rotate_clockwise
            });

            int[][] rotationLoop5 = new int[16][];
            for (int i = 0; i < 16; i++)
            {
                rotationLoop5[i] = new int[5];
                for (int r = 0; r < 5; r++)
                {
                    // Copy a pole rotation
                    rotationLoop5[i][4 - r] = rotationLoopsPoles[i][r];

                    // Find a rotation and apply that too
                    for (int j = 0; j < 16; j++)
                    {
                        for (int k = 0; k < 5; k++)
                        {
                            if (rotationLoop5[i][4 - r] == rotationLoops[j][k])
                            {
                                rotationLoop5[i][4 - r] = rotationLoops[j][(k - 1 + 5) % 5];
                                j = 16;
                                k = 5;
                            }
                        }
                    }

                    // Run the pole rotation three more times
                    for (int c = 0; c < 3; c++)
                    {
                        for (int j = 0; j < 16; j++)
                        {
                            for (int k = 0; k < 5; k++)
                            {
                                if (rotationLoop5[i][4 - r] == rotationLoopsPoles[j][k])
                                {
                                    rotationLoop5[i][4 - r] = rotationLoopsPoles[j][(k - 1 + 5) % 5];
                                    j = 16;
                                    k = 5;
                                }
                            }
                        }
                    }
                }
            }

            x = _tiles[70].Location.X - GraphicsSettings.TileHorizontalSpacing;
            y = _tiles[70].Location.Y + yOffset;
            _rotations.Add(new GameRotation
            {
                Location = new Point(x - radius, y - radius),
                BoundingBox = new Rectangle(x - radius, y - radius, radius * 2, radius * 2),
                RotationLoops = rotationLoop5,
                Image = Properties.Resources.rotate_counter_clockwise
            });
            x = _tiles[18].Location.X + xOffset;
            y = _tiles[18].Location.Y + yOffsetInverted;
            _rotations.Add(new GameRotation
            {
                Location = new Point(x - radius, y - radius),
                BoundingBox = new Rectangle(x - radius, y - radius, radius * 2, radius * 2),
                RotationLoops = rotationLoop5,
                Image = Properties.Resources.rotate_clockwise
            });
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

        private void InitializeHud()
        {
            // Turn clock
            var leftMost = _tiles.Min(x => x.Location.X);
            var bottomMost = _tiles.Max(x => x.Location.Y + x.BoundingBox.Height);
            var width = _tiles[0].BoundingBox.Width;
            _clock = new Rectangle(leftMost, bottomMost - width, width, width);
        }

        private void RotateBoard(int[][] rotationLoops)
        {
            int[] redirects = new int[80];

            for (int i = 0; i < 16; i++)
            {
                for (int r = 0; r < 5; r++)
                {
                    redirects[rotationLoops[i][r]] = boardIndexFromTileIndex[rotationLoops[i][(r + 1) % 5]];
                }
            }

            for (int i = 0; i < 80; i++)
            {
                redirects[i] = (redirects[i] + 80) % 80;
            }

            boardIndexFromTileIndex = redirects;
        }

        public void Resize()
        {
            if (_size != _panel.Size)
            {
                GraphicsSettings.UpdateBestTileWidth(_panel.Size);
                InitializeTiles();
                InitializeRotations();
                InitializeHud();

                _size = _panel.Size;
            }
        }

        public void Click(Point mouseLocation)
        {
            Point mouse = GetOffsetPoint(mouseLocation);

            if (GraphicsSettings.ShowRotationButtons)
            {
                // See if they clicked a rotation button
                for (int i = 0; i < _rotations.Count; i++)
                {
                    if (_rotations[i].IsWithinCircle(mouse))
                    {
                        RotateBoard(_rotations[i].RotationLoops);
                    }
                }
            }
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

            bool reviewMode = game.MoveHistory.Count > 0 && moveNumber != game.MoveHistory.Count;

            int hoverTile = GetTileIndex(mouseLocation);
            Board gameState = reviewMode ? game.GetPreviousState(moveNumber) : game.CurrentState;

            Point mouse = GetOffsetPoint(mouseLocation);

            int lastPlayIndex = GetTileIndexFromBoardIndex(game.MoveHistory.Count >= moveNumber && moveNumber - 1 >= 0 ? game.MoveHistory[moveNumber - 1] : -1);

            Color background = reviewMode ? GraphicsSettings.ReviewBackgroundColor : GraphicsSettings.BackgroundColor;

            using (Bitmap b = new Bitmap(GraphicsSettings.IdealPanelWidth, GraphicsSettings.IdealPanelHeight))
            using (Graphics g = Graphics.FromImage(b))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                g.Clear(background);

                // Draw rotation buttons
                if (GraphicsSettings.ShowRotationButtons)
                {
                    for (int i = 0; i < _rotations.Count; i++)
                    {
                        float opacity = 0.25f;
                        if (_rotations[i].IsWithinCircle(mouse))
                        {
                            opacity = 1f;
                        }

                        ColorMatrix matrix = new ColorMatrix();
                        matrix.Matrix33 = opacity;
                        ImageAttributes attributes = new ImageAttributes();
                        attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                        g.DrawImage(_rotations[i].Image, _rotations[i].BoundingBox, 0, 0, _rotations[i].Image.Width, _rotations[i].Image.Height, GraphicsUnit.Pixel, attributes);
                    }
                }

                // Draw tiles
                for (int i = 0; i < 80; i++)
                {
                    DrawTile(g, gameState, i, hoverTile, lastPlayIndex, highlightLastMove);

                    if (GraphicsSettings.ShowTileNames)
                    {
                        DrawTileSubText(g, i, Constants.TileNames[boardIndexFromTileIndex[i]]);
                    }

                    if (gameState.Tiles[boardIndexFromTileIndex[i]] == 0)
                    {
                        if (GraphicsSettings.ShowTileIndexes)
                        {
                            DrawTileMainText(g, i, boardIndexFromTileIndex[i].ToString());
                        }

                        //DrawTileMainText(g, i, Constants.FastestPaths[i].Length.ToString());
                    }
                    else
                    {
                        int value = Math.Abs(gameState.Tiles[boardIndexFromTileIndex[i]]);

                        if (!GraphicsSettings.ShowTileNames)
                        {
                            if (Math.Abs(gameState.Tiles[boardIndexFromTileIndex[i]]) <= VolcanoGame.Settings.MaxMagmaChamberLevel)
                            {
                                DrawTileSubText(g, i, "Chamber");
                            }
                            else
                            {
                                value -= VolcanoGame.Settings.MaxMagmaChamberLevel;
                                DrawTileSubText(g, i, "Volcano");
                            }
                        }

                        DrawTileMainText(g, i, value.ToString());
                    }
                }

                // Draw turn clock
                if (_clock.Width > 0)
                {
                    var lastToMove = moveNumber % 6;

                    var centerCover = new Rectangle(_clock.X + _clock.Width / 3, _clock.Y + _clock.Width / 3, _clock.Width / 3, _clock.Width / 3);
                    var angle = 360f / 6;
                    var start = -90f - angle / 2;

                    var player1brush = new SolidBrush(GraphicsSettings.PlayerOneVolcanoTileColor);
                    var player2brush = new SolidBrush(GraphicsSettings.PlayerTwoVolcanoTileColor);
                    var growthBrush = new SolidBrush(GraphicsSettings.EmptyTileColor);
                    var backgroundBrush = new SolidBrush(GraphicsSettings.BackgroundColor);
                    var playerToMovePen = new Pen(GraphicsSettings.LastPlayedTileBorderColor, 5f);

                    g.FillPie(player1brush, _clock, start, angle);
                    g.FillPie(player2brush, _clock, start + angle, angle);
                    g.FillPie(growthBrush, _clock, start + angle * 2, angle);
                    g.FillPie(player2brush, _clock, start + angle * 3, angle);
                    g.FillPie(player1brush, _clock, start + angle * 4, angle);
                    g.FillPie(growthBrush, _clock, start + angle * 5, angle);

                    g.DrawPie(playerToMovePen, _clock, start + angle * lastToMove, angle);

                    g.FillEllipse(backgroundBrush, centerCover);
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

        private void DrawTile(Graphics g, Board gameState, int index, int hoverTile, int lastPlayTile, bool highlightLastMove)
        {
            Color tileColor = GraphicsSettings.EmptyTileColor;

            if (gameState.Tiles[boardIndexFromTileIndex[index]] > 0)
            {
                tileColor = Math.Abs(gameState.Tiles[boardIndexFromTileIndex[index]]) > VolcanoGame.Settings.MaxMagmaChamberLevel ? GraphicsSettings.PlayerOneVolcanoTileColor : GraphicsSettings.PlayerOneMagmaChamberTileColor;
            }
            else if (gameState.Tiles[boardIndexFromTileIndex[index]] < 0)
            {
                tileColor = Math.Abs(gameState.Tiles[boardIndexFromTileIndex[index]]) > VolcanoGame.Settings.MaxMagmaChamberLevel ? GraphicsSettings.PlayerTwoVolcanoTileColor : GraphicsSettings.PlayerTwoMagmaChamberTileColor;
            }

            // Winning path
            if (gameState.WinningPath.Count > 0 && !gameState.WinningPath.Contains(boardIndexFromTileIndex[index]))
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
                if (lastPlayTile == index || (lastPlayTile == 80 && Math.Abs(gameState.Tiles[boardIndexFromTileIndex[index]]) > 0))
                {
                    Pen pen = new Pen(GraphicsSettings.LastPlayedTileBorderColor, GraphicsSettings.TileHorizontalSpacing);
                    g.DrawPolygon(pen, _tiles[index].Path.PathPoints);
                }
            }

            if (hoverTile >= 0)
            {
                // Tile under the mouse pointer
                if (index == hoverTile)
                {
                    Pen pen = new Pen(GraphicsSettings.HoverTileBorderColor, GraphicsSettings.TileHorizontalSpacing);
                    g.DrawPolygon(pen, _tiles[index].Path.PathPoints);
                }

                // Tiles directly adjacent to the tile under the mouse pointer
                if (Constants.AdjacentIndexes[hoverTile].Any(x => x == index))
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
                if (Constants.Antipodes[hoverTile] == index)
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

        public int GetBoardIndex(Point location)
        {
            int tileIndex = GetTileIndex(location);
            if (tileIndex >= 0)
            {
                return boardIndexFromTileIndex[tileIndex];
            }
            return tileIndex;
        }

        private int GetTileIndex(Point location)
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

        private int GetTileIndexFromBoardIndex(int index)
        {
            // Look up the redirected tile index from the hover tile index
            for (int r = 0; r < 80; r++)
            {
                if (boardIndexFromTileIndex[r] == index)
                {
                    return r;
                }
            }

            return index;
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
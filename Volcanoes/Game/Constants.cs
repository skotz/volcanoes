using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Search;

namespace Volcano.Game
{
    class Constants
    {
        public const int MaxVolcanoLevel = 8;

        public const int MaxMagmaChamberLevel = 4;

        private static Lazy<int[][]> _connectingTiles = new Lazy<int[][]>(GetConnectingTiles);

        private static Lazy<string[]> _tileNames = new Lazy<string[]>(GetTileNames);

        private static Lazy<int[]> _antipodes = new Lazy<int[]>(GetAntipodes);

        private static Lazy<int[][]> _kittyCornerTiles = new Lazy<int[][]>(GetKittyCornerTiles);

        private static Lazy<int[][]> _fastestPaths = new Lazy<int[][]>(GetFastestPaths);

        private static Lazy<Dictionary<string, int>> _reverseNameToIndex = new Lazy<Dictionary<string, int>>(GetTileIndexes);

        /// <summary>
        /// An array mapping a source tile index to it's three connecting triangle indexes.
        /// E.G., ConnectingTiles[55] = { 33, 52, 74 } since tile 55 connects to tiles 33, 51, and 74.
        /// </summary>
        public static int[][] ConnectingTiles { get { return _connectingTiles.Value; } }

        /// <summary>
        /// An array mapping a source tile index to it's tile name.
        /// E.G., TileNames[5] = "2A"
        /// </summary>
        public static string[] TileNames { get { return _tileNames.Value; } }

        /// <summary>
        /// An array mapping a source tile to its antipodes (tile directly opposite on the 3D board shape).
        /// </summary>
        public static int[] Antipodes { get { return _antipodes.Value; } }

        /// <summary>
        /// An array mapping a source tile index to it's three or four kitty corner tile indexes.
        /// NOTE: Currently there are only mappings for "A" tiles (not B, C, or D)
        /// </summary>
        public static int[][] KittyCornerTiles { get { return _kittyCornerTiles.Value; } }

        /// <summary>
        /// An array mapping a source tile index to a path to it's antipode.
        /// </summary>
        public static int[][] FastestPaths { get { return _fastestPaths.Value; } }

        public static Dictionary<string, int> TileIndexes { get { return _reverseNameToIndex.Value; } }

        private static int[][] GetConnectingTiles()
        {
            int[][] connections = new int[80][];

            // First row
            //    .
            //   /B\
            //  / A \   x5
            // /D   C\
            // -------
            for (int outer = 0; outer < 5; outer++)
            {
                // A
                int i = outer * 4;
                connections[i] = new int[3] { i + 1, i + 2, i + 3 };

                // B
                i++;
                connections[i] = new int[3] { (i - 1 + 20) % 20, (i + 4) % 20, (i - 4 + 20) % 20 };

                // C
                i++;
                connections[i] = new int[3] { (i - 2 + 20) % 20, i + 21, (i + 5) % 20 };

                // D
                i++;
                connections[i] = new int[3] { (i - 3 + 20) % 20, i + 19, (i - 5 + 20) % 20 };
            }

            // Second row
            // -------
            // \C   D/
            //  \ A /   x5
            //   \B/
            //    .
            for (int outer = 5; outer < 10; outer++)
            {
                // A
                int i = outer * 4;
                connections[i] = new int[3] { i + 1, i + 2, i + 3 };

                // B
                i++;
                connections[i] = new int[3] { i - 1, i + 22, i + 17 };
                connections[i][2] = i == 21 ? 58 : connections[i][2];

                // C
                i++;
                connections[i] = new int[3] { i - 2, i - 19, i + 15 };
                connections[i][2] = i == 22 ? 57 : connections[i][2];

                // D
                i++;
                connections[i] = new int[3] { i - 3, i + 18, i - 21 };
            }

            // Third row
            //    .
            //   /B\
            //  / A \   x5
            // /D   C\
            // -------
            for (int outer = 10; outer < 15; outer++)
            {
                // A
                int i = outer * 4;
                connections[i] = new int[3] { i + 1, i + 2, i + 3 };

                // B
                i++;
                connections[i] = new int[3] { i - 1, i - 18, i - 15 };
                connections[i][2] = i == 57 ? 22 : connections[i][2];

                // C
                i++;
                connections[i] = new int[3] { i - 2, i - 17, i + 21 };
                connections[i][1] = i == 58 ? 21 : connections[i][1];

                // D
                i++;
                connections[i] = new int[3] { i - 3, i + 19, i - 22 };
            }

            // Fourth row
            // -------
            // \C   D/
            //  \ A /   x5
            //   \B/
            //    .
            for (int outer = 15; outer < 20; outer++)
            {
                // A
                int i = outer * 4;
                connections[i] = new int[3] { i + 1, i + 2, i + 3 };

                // B
                i++;
                connections[i] = new int[3] { i - 1, i + 4, i - 4 };
                connections[i][2] = i == 61 ? 77 : connections[i][2];
                connections[i][1] = i == 77 ? 61 : connections[i][1];

                // C
                i++;
                connections[i] = new int[3] { i - 2, i - 19, i - 3 };
                connections[i][2] = i == 62 ? 79 : connections[i][2];

                // D
                i++;
                connections[i] = new int[3] { i - 3, i + 3, i - 21 };
                connections[i][1] = i == 79 ? 62 : connections[i][1];
            }

            return connections;
        }

        private static string[] GetTileNames()
        {
            string[] names = new string[80];

            for (int outer = 0; outer < 20; outer++)
            {
                int outerRow = outer / 5;
                int outerCol = outer % 5;

                for (int inner = 0; inner < 4; inner++)
                {
                    int index = outer * 4 + inner;
                    names[index] = (outer + 1).ToString();

                    switch (inner)
                    {
                        case 0:
                            names[index] += "A";
                            break;
                        case 1:
                            names[index] += "B";
                            break;
                        case 2:
                            names[index] += "C";
                            break;
                        case 3:
                            names[index] += "D";
                            break;
                    }
                }
            }

            return names;
        }

        private static int[] GetAntipodes()
        {
            int[] antipodes = new int[80];

            for (int i = 0; i < 12; i++)
            {
                int a = 68;
                a = i % 4 == 2 ? 69 : a;
                a = i % 4 == 3 ? 67 : a;

                antipodes[i] = i + a;
                antipodes[i + a] = i;
            }

            for (int i = 12; i < 20; i++)
            {
                int a = 48;
                a = i % 4 == 2 ? 49 : a;
                a = i % 4 == 3 ? 47 : a;

                antipodes[i] = i + a;
                antipodes[i + a] = i;
            }

            for (int i = 20; i < 32; i++)
            {
                int a = 28;
                a = i % 4 == 2 ? 29 : a;
                a = i % 4 == 3 ? 27 : a;

                antipodes[i] = i + a;
                antipodes[i + a] = i;
            }

            for (int i = 32; i < 40; i++)
            {
                int a = 8;
                a = i % 4 == 2 ? 9 : a;
                a = i % 4 == 3 ? 7 : a;

                antipodes[i] = i + a;
                antipodes[i + a] = i;
            }

            return antipodes;
        }

        private static int[][] GetKittyCornerTiles()
        {
            int[][] corners = new int[80][];

            for (int outer = 0; outer < 5; outer++)
            {
                // A
                int i = outer * 4;
                corners[i] = new int[3] { (i + 4) % 20, i + 20, (i - 4 + 20) % 20 };

                // TODO: B, C, D
                i++;
                corners[i] = new int[3] { i, i, i };
                i++;
                corners[i] = new int[3] { i, i, i };
                i++;
                corners[i] = new int[3] { i, i, i };
            }

            for (int outer = 5; outer < 10; outer++)
            {
                // A
                int i = outer * 4;
                corners[i] = new int[3] { i - 20, i + 20, i + 16 };
                corners[i][2] = i == 20 ? 56 : corners[i][2];

                // TODO: B, C, D
                i++;
                corners[i] = new int[3] { i, i, i };
                i++;
                corners[i] = new int[3] { i, i, i };
                i++;
                corners[i] = new int[3] { i, i, i };
            }

            for (int outer = 10; outer < 15; outer++)
            {
                // A
                int i = outer * 4;
                corners[i] = new int[3] { i - 20, i - 16, i + 20 };
                corners[i][1] = i == 56 ? 20 : corners[i][1];

                // TODO: B, C, D
                i++;
                corners[i] = new int[3] { i, i, i };
                i++;
                corners[i] = new int[3] { i, i, i };
                i++;
                corners[i] = new int[3] { i, i, i };
            }

            for (int outer = 15; outer < 20; outer++)
            {
                // A
                int i = outer * 4;
                corners[i] = new int[3] { i - 20, i + 4, i - 4 };
                corners[i][1] = i == 76 ? 60 : corners[i][1];
                corners[i][2] = i == 60 ? 76 : corners[i][2];

                // TODO: B, C, D
                i++;
                corners[i] = new int[3] { i, i, i };
                i++;
                corners[i] = new int[3] { i, i, i };
                i++;
                corners[i] = new int[3] { i, i, i };
            }

            return corners;
        }

        private static int[][] GetFastestPaths()
        {
            PathFinder pathFinder = new UnrestrictedPathFinder();
            int[][] paths = new int[80][];
            Board board = new Board();

            for (int i = 0; i < 80; i++)
            {
                paths[i] = pathFinder.FindPath(board, i, Antipodes[i]).Path.ToArray();
            }

            return paths;
        }

        private static Dictionary<string, int> GetTileIndexes()
        {
            Dictionary<string, int> indexes = new Dictionary<string, int>();

            for (int i = 0; i < 80; i++)
            {
                indexes.Add(TileNames[i], i);
            }

            return indexes;
        }
    }
}

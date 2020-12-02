using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Volcano.Search;

namespace Volcano.Game
{
    internal class Constants
    {
        public const int AllGrowMove = 80;

        /// <summary>
        /// An array mapping a source tile index to it's three connecting triangle indexes.
        /// E.G., ConnectingTiles[55] = { 33, 52, 74 } since tile 55 connects to tiles 33, 51, and 74.
        /// </summary>
        public static int[][] AdjacentIndexes = GetConnectingTiles();

        /// <summary>
        /// An array mapping a source tile index to it's tile name.
        /// E.G., TileNames[5] = "N02"
        /// </summary>
        public static string[] TileNames = GetTileNames();

        /// <summary>
        /// FOR BACKWARDS COMPATIBILITY
        /// An array mapping a source tile index to it's tile name.
        /// E.G., TileNames[5] = "2A"
        /// </summary>
        public static string[] OldTileNames = GetOldTileNames();

        /// <summary>
        /// An array mapping a source tile to its antipodes (tile directly opposite on the 3D board shape).
        /// </summary>
        public static int[] Antipodes = GetAntipodes();

        /// <summary>
        /// An array mapping a source tile index to it's three or four kitty corner tile indexes.
        /// NOTE: Currently there are only mappings for "A" tiles (not B, C, or D)
        /// </summary>
        public static int[][] KittyCornerTiles = GetKittyCornerTiles();

        /// <summary>
        /// An array mapping a source tile index to a path to it's antipode.
        /// </summary>
        public static int[][] FastestPaths = GetFastestPaths();

        /// <summary>
        /// An dictionary mapping a tile name back to it's index.
        /// Supports both old and new tile names!
        /// </summary>
        public static Dictionary<string, int> TileIndexes = GetTileIndexes();

        /// <summary>
        /// An array mapping each tile to a list of tiles 3 moves away
        /// </summary>
        public static int[][] ThreeAway = GetThreeAway();

        /// <summary>
        /// Random numbers to use for zobrist style hashes
        /// </summary>
        public static long[,] ZobristKeys = GetZobristKeys();

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
            return new string[] {
                "N07", "N01", "N08", "N06", "N10", "N02", "N11", "N09", "N13", "N03", "N14", "N12", "N16", "N04", "N17", "N15", "N19", "N05", "N20", "N18", "N22", "S32", "N21", "N23", "N26", "S36", "N25", "N27", "N30", "S40", "N29", "N31", "N34", "S24", "N33", "N35", "N38", "S28", "N37", "N39",
                "S34", "N24", "S35", "S33", "S38", "N28", "S39", "S37", "S22", "N32", "S23", "S21", "S26", "N36", "S27", "S25", "S30", "N40", "S31", "S29", "S16", "S04", "S15", "S17", "S19", "S05", "S18", "S20", "S07", "S01", "S06", "S08", "S10", "S02", "S09", "S11", "S13", "S03", "S12", "S14",
                "G",
            };
        }

        private static string[] GetOldTileNames()
        {
            string[] names = new string[81];

            for (int outer = 0; outer < 20; outer++)
            {
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

            // For growth
            names[80] = "G";

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

            if (paths.Any(x => x.Length != 12))
            {
                // To prevent me from messing up the path finding algorithm...
                throw new Exception("Path finder distances are incorrect!");
            }

            return paths;
        }

        private static Dictionary<string, int> GetTileIndexes()
        {
            Dictionary<string, int> indexes = new Dictionary<string, int>();

            // The 80th index is a pseudo tile for the growth phase
            for (int i = 0; i < 81; i++)
            {
                indexes.Add(TileNames[i], i);
            }

            // Support legacy tile names
            for (int i = 0; i < 81; i++)
            {
                if (indexes.ContainsKey(OldTileNames[i]))
                {
                    if (indexes[OldTileNames[i]] != i)
                    {
                        throw new Exception("Tile mapping duplicate!");
                    }
                }
                else
                {
                    indexes.Add(OldTileNames[i], i);
                }
            }

            return indexes;
        }

        private static int[][] GetThreeAway()
        {
            var tiles = new int[80][];
            var connections = GetConnectingTiles();

            for (int i = 0; i < 80; i++)
            {
                var done = new List<int>();
                var queue = new List<int>();
                queue.Add(i);

                for (int d = 0; d < 3; d++)
                {
                    for (int x = queue.Count - 1; x >= 0; x--)
                    {
                        foreach (var con in connections[queue[x]])
                        {
                            if (!done.Contains(con))
                            {
                                queue.Add(con);
                            }
                        }

                        done.Add(queue[x]);
                        queue.RemoveAt(x);
                    }
                }

                tiles[i] = queue.Distinct().ToArray();
            }

            return tiles;
        }

        private static long[,] GetZobristKeys()
        {
            var rand = new Random();
            var tiles = new long[80, 100];

            for (int i = 0; i < 80; i++)
            {
                for (int x = 0; x < 100; x++)
                {
                    var buffer = new byte[sizeof(long)];
                    rand.NextBytes(buffer);
                    tiles[i, x] = BitConverter.ToInt64(buffer, 0);
                }
            }

            return tiles;
        }
    }
}
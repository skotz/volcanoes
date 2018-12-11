using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Game
{
    class Constants
    {
        public const int VolcanoEruptionValue = 8;

        private static Lazy<int[][]> _connectingTiles = new Lazy<int[][]>(LoadConnectingTiles);

        /// <summary>
        /// An array mapping a source tile index to it's three connecting triangle indexes.
        /// E.G., ConnectingTiles[55] = { 33, 52, 74 } since tile 55 connects to tiles 33, 51, and 74.
        /// </summary>
        public static int[][] ConnectingTiles { get { return _connectingTiles.Value; } }

        private static int[][] LoadConnectingTiles()
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
    }
}

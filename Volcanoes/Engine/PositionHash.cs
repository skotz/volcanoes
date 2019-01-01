using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Engine
{
    class PositionHash
    {
        private RNGCryptoServiceProvider random;
        private Hashtable hashes;

        private int[][][] tilesMasks;

        public PositionHash()
        {
            random = new RNGCryptoServiceProvider();
            hashes = new Hashtable();

            tilesMasks = new int[80][][];
            for (int tile = 0; tile < 80; tile++)
            {
                tilesMasks[tile] = new int[3][];
                for (int player = 0; player < 3; player++)
                {
                    tilesMasks[tile][player] = new int[VolcanoGame.Settings.MaxVolcanoLevel];
                    for (int value = 0; value < VolcanoGame.Settings.MaxVolcanoLevel; value++)
                    {
                        tilesMasks[tile][player][value] = GetRandom();
                    }
                }
            }
        }

        public int? Get(Board board)
        {
            int hash = GetHash(board);
            return (int?)hashes[hash];
        }

        public void Set(Board board, int evaluation)
        {
            int hash = GetHash(board);
            hashes[hash] = evaluation;
        }

        private int GetHash(Board board)
        {
            int hash = 0;

            // Get a hash of the board
            if (hash == 0)
            {
                for (int tile = 0; tile < 80; tile++)
                {
                    int player = board.Tiles[tile] == 0 ? 0 : (board.Tiles[tile] > 0 ? 1 : 2);
                    int value = Math.Abs(board.Tiles[tile]);

                    hash ^= tilesMasks[tile][player][value];
                }
            }

            return hash;
        }

        private int GetRandom()
        {
            byte[] buffer = new byte[sizeof(int)];
            random.GetBytes(buffer);
            return BitConverter.ToInt32(buffer, 0);
        }
    }
}

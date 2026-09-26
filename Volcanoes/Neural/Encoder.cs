using System.Collections.Generic;
using TorchSharp;
using Volcano.Game;

namespace Volcano.Neural
{
    internal class Encoder
    {
        /// <summary>
        /// 4 for enemy pieces -4 through -1
        /// 1 for empty tiles 0
        /// 4 for my pieces 1 through 4
        /// 1 for having two moves
        /// </summary>
        public const int Planes = 10;

        /// <summary>
        /// shape [batch, cell, channel]
        /// </summary>
        public torch.Tensor Encode(IReadOnlyList<Board> states, torch.Device device)
        {
            var batchSize = states.Count;
            var cells = 80;
            var data = new float[batchSize * Planes * cells];

            for (var b = 0; b < batchSize; b++)
            {
                var state = states[b];
                var board = state.Tiles;
                var baseIndex = b * Planes * cells;

                for (var i = 0; i < cells; i++)
                {
                    // adjust [-4, +4] to [0, 8]
                    var channel = board[i] + 4;
                    data[baseIndex + i * Planes + channel] = 1f;
                }

                // if this is the first of two moves
                if (state.GetMoveTypeForTurn(state.Turn + 1) == MoveType.AllGrow)
                {
                    for (var i = 0; i < cells; i++)
                    {
                        var channel = Planes - 1;
                        data[baseIndex + i * Planes + channel] = 1f;
                    }
                }
            }

            return torch.tensor(data, new long[] { batchSize, cells, Planes }).to(device);
        }

        /// <summary>
        /// [1, 80, 10]
        /// </summary>
        public torch.Tensor Encode(Board state, torch.Device device) => Encode(new[] { state }, device);
    }
}
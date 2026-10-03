using System.Collections.Generic;
using TorchSharp;
using Volcano.Game;

namespace Volcano.Neural
{
    internal class Encoder
    {
        /// <summary>
        /// 1 for each tile value, normalized to [-1, 1]
        /// 1 for having two moves
        /// </summary>
        public const int channels = 2;

        /// <summary>
        /// shape [batch, channel, cell]
        /// </summary>
        public torch.Tensor Encode(IReadOnlyList<Board> states, torch.Device device)
        {
            var batchSize = states.Count;
            var cells = 80;
            var data = new float[batchSize * channels * cells];

            for (var b = 0; b < batchSize; b++)
            {
                var state = states[b];
                var board = state.Tiles;
                var baseIndex = b * channels * cells;

                // encode every tile by normalizing [-4,4] to [-1,1]
                for (var i = 0; i < cells; i++)
                {
                    var channel = 0;
                    data[baseIndex + channel * 80 + i] = board[i] / 4.0f;
                }

                // if this is the first of two moves
                if (state.GetMoveTypeForTurn(state.Turn + 1) == MoveType.AllGrow)
                {
                    var channel = channels - 1;
                    for (var i = 0; i < cells; i++)
                    {
                        data[baseIndex + channel * 80 + i] = 1f;
                    }
                }
            }

            return torch.tensor(data, new long[] { batchSize, channels, cells }).to(device);
        }

        /// <summary>
        /// [1, 10, 80]
        /// </summary>
        public torch.Tensor Encode(Board state, torch.Device device) => Encode(new[] { state }, device);
    }
}
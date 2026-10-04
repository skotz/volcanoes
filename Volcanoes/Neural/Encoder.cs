using System.Collections.Generic;
using TorchSharp;
using Volcano.Game;

namespace Volcano.Neural
{
    internal class Encoder
    {
        /// <summary>
        /// 9 one-hot channels for each value in [-4,4]
        /// 27 one-hot channels for neighbor tiles (9 per tile value * 3 neighbors)
        /// 1 channel for move type
        /// 1 channel for antipode dense encoding
        /// </summary>
        public const int channels = 10;

        private string[][] tileMapping = [
            [ "N06", "N01", "N09", "N02", "N12", "N03", "N15", "N04", "N18", "N05", ],
            [ "N21", "N07", "N25", "N10", "N29", "N13", "N33", "N16", "N37", "N19", ],
            [ "N22", "N08", "N26", "N11", "N30", "N14", "N34", "N17", "N38", "N20", ],
            [ "S32", "N23", "S36", "N27", "S40", "N31", "S24", "N35", "S28", "N39", ],
            [ "S33", "N24", "S37", "N28", "S21", "N32", "S25", "N36", "S29", "N40", ],
            [ "S15", "S34", "S18", "S38", "S06", "S22", "S09", "S26", "S12", "S30", ],
            [ "S16", "S35", "S19", "S39", "S07", "S23", "S10", "S27", "S13", "S31", ],
            [ "S04", "S17", "S05", "S20", "S01", "S08", "S02", "S11", "S03", "S14", ],
        ];

        private static (int, int)[] indexToMatrix;

        public Encoder()
        {
            InitializeTileMap();
        }

        /// <summary>
        /// shape [batch, channel, height, width]
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
                    var channel = board[i] + 4;
                    data[baseIndex + i * channels + channel] = 1.0f;
                }

                // if this is the first of two moves
                if (state.GetMoveTypeForTurn(state.Turn + 1) == MoveType.AllGrow)
                {
                    var channel = channels - 1;
                    for (var i = 0; i < cells; i++)
                    {
                        data[baseIndex + i * channels + channel] = 1f;
                    }
                }
            }

            return torch.tensor(data, new long[] { batchSize, cells, channels }).to(device);
        }

        private void InitializeTileMap()
        {
            if (indexToMatrix == null)
            {
                indexToMatrix = new (int, int)[80];
                for (var y = 0; y < tileMapping.Length; y++)
                {
                    for (var x = 0; x < tileMapping[y].Length; x++)
                    {
                        var index = Constants.TileIndexes[tileMapping[y][x]];
                        indexToMatrix[index] = (x, y);
                    }
                }
            }
        }

        /// <summary>
        /// [1, 10, 80]
        /// </summary>
        public torch.Tensor Encode(Board state, torch.Device device) => Encode(new[] { state }, device);
    }
}
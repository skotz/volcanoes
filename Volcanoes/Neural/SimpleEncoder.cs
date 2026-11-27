using System.Collections.Generic;
using TorchSharp;
using Volcano.Game;

namespace Volcano.Neural
{
    internal class SimpleEncoder : IBoardEncoder
    {
        /// <summary>
        /// 9 one-hot channels for each value in [-4,4]
        /// 1 channel for move type
        /// </summary>
        public const int channels = 10;

        public int InputChannels => channels;

        private const int height = 8;
        private const int width = 10;

        private readonly string[][] tileMapping = [
            [ "N07", "N01", "N10", "N02", "N13", "N03", "N16", "N04", "N19", "N05", ],
            [ "N06", "N08", "N09", "N11", "N12", "N14", "N15", "N17", "N18", "N20", ],
            [ "N22", "N23", "N26", "N27", "N30", "N31", "N34", "N35", "N38", "N39", ],
            [ "N21", "S32", "N25", "S36", "N29", "S40", "N33", "S24", "N37", "S28", ],
            [ "S34", "N24", "S38", "N28", "S22", "N32", "S26", "N36", "S30", "N40", ],
            [ "S33", "S35", "S37", "S39", "S21", "S23", "S25", "S27", "S29", "S31", ],
            [ "S16", "S17", "S19", "S20", "S07", "S08", "S10", "S11", "S13", "S14", ],
            [ "S15", "S04", "S18", "S05", "S06", "S01", "S09", "S02", "S12", "S03", ],
        ];

        private readonly (int, int)[] indexToMatrix = new (int, int)[80];

        public SimpleEncoder()
        {
            for (var y = 0; y < tileMapping.Length; y++)
            {
                for (var x = 0; x < tileMapping[y].Length; x++)
                {
                    var index = Constants.TileIndexes[tileMapping[y][x]];
                    indexToMatrix[index] = (x, y);
                }
            }
        }

        /// <summary>
        /// shape [batch, channel, height, width]
        /// </summary>
        public torch.Tensor Encode(IReadOnlyList<Board> states, torch.Device device)
        {
            var batchSize = states.Count;
            var planeSize = height * width;
            var data = new float[batchSize * channels * planeSize];

            for (var b = 0; b < batchSize; b++)
            {
                var state = states[b];
                var baseIndex = b * channels * planeSize;

                var allGrowNext = state.GetMoveTypeForTurn(state.Turn + 1) == MoveType.AllGrow;

                for (var tileIdx = 0; tileIdx < 80; tileIdx++)
                {
                    var (x, y) = indexToMatrix[tileIdx];
                    var linearIdx = y * width + x;

                    // Channel 0-8: One-hot encoding for tile value [-4, 4]
                    var valueChannel = state.Tiles[tileIdx] + 4;
                    data[baseIndex + valueChannel * planeSize + linearIdx] = 1.0f;

                    // Channel 9: Move type channel (all 1s if next turn is AllGrow)
                    if (allGrowNext)
                    {
                        data[baseIndex + 9 * planeSize + linearIdx] = 1.0f;
                    }
                }
            }

            return torch.tensor(data, [batchSize, channels, height, width]).to(device);
        }

        /// <summary>
        /// [1, 10, 8, 10]
        /// </summary>
        public torch.Tensor Encode(Board state, torch.Device device) => Encode([state], device);
    }
}
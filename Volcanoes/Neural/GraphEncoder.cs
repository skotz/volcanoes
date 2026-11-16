using System.Collections.Generic;
using TorchSharp;
using Volcano.Game;

namespace Volcano.Neural
{
    internal class GraphEncoder : IBoardEncoder
    {
        /// <summary>
        /// 9 one-hot channels for each value in [-4,4]
        /// 1 channel for move type (all nodes are 1 if next turn is AllGrow)
        /// </summary>
        public const int features = 10;

        public int InputChannels => features;

        public const int nodes = 80;

        /// <summary>
        /// shape [batch, node, feature]
        /// </summary>
        public torch.Tensor Encode(IReadOnlyList<Board> states, torch.Device device)
        {
            var batchSize = states.Count;
            var data = new float[batchSize * nodes * features];

            for (var b = 0; b < batchSize; b++)
            {
                var state = states[b];
                var board = state.Tiles;
                var allGrow = state.GetMoveTypeForTurn(state.Turn + 1) == MoveType.AllGrow;

                for (var tileIdx = 0; tileIdx < nodes; tileIdx++)
                {
                    var baseIndex = (b * nodes + tileIdx) * features;
                    var tileValue = board[tileIdx];
                    data[baseIndex + tileValue + 4] = 1.0f;

                    if (allGrow)
                    {
                        data[baseIndex + 9] = 1.0f;
                    }
                }
            }

            return torch.tensor(data, [batchSize, nodes, features]).to(device);
        }

        /// <summary>
        /// [1, 80, 10]
        /// </summary>
        public torch.Tensor Encode(Board state, torch.Device device) => Encode([state], device);
    }
}
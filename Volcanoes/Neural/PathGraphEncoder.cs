using System;
using System.Collections.Generic;
using System.Linq;
using TorchSharp;
using Volcano.Game;
using Volcano.Search;

namespace Volcano.Neural
{
    internal class PathGraphEncoder : IBoardEncoder
    {
        /// <summary>
        /// 9 one-hot channels for each value in [-4,4]
        /// 1 channel for move type (all nodes are 1 if next turn is AllGrow)
        /// 1 channel for the state after one forced growth, dense encoded
        /// 1 channel for the state after two forced growths, dense encoded
        /// 1 channel for the state after three forced growths, dense encoded
        /// 1 channel for the shortest path for the current player (1 on path tiles)
        /// 1 channel for the shortest path for the opponent (1 on path tiles)
        /// 1 channel for all current player paths, summed and scaled to [0,1]
        /// 1 channel for all opponent paths, summed and scaled to [0,1]
        /// 1 channel for triangle type (1 for equilateral, 0 otherwise)
        /// 1 channel for count of neighbors > 0, divided by 3
        /// 1 channel for count of neighbors < 0, divided by 3
        /// 1 channel for count of neighbors == 0, divided by 3
        /// 1 channel for the dense encoded antipode value
        /// 1 channel for count of empty tiles, divided by 80 (same for all nodes)
        /// 1 channel for turn, divided by 40 and capped at 40 (same for all nodes)
        /// </summary>
        public const int features = 24;

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

                var afterOneGrowth = new Board(state);
                afterOneGrowth.MakeMove(Constants.AllGrowMove, false, false);

                var afterTwoGrowths = new Board(afterOneGrowth);
                afterTwoGrowths.MakeMove(Constants.AllGrowMove, false, false);

                var afterThreeGrowths = new Board(afterTwoGrowths);
                afterThreeGrowths.MakeMove(Constants.AllGrowMove, false, false);

                var allGrow = state.GetMoveTypeForTurn(state.Turn + 1) == MoveType.AllGrow;

                var paths = new TotalPathFinder().GetAllPaths(state);
                var bestPath = paths.MyPaths.OrderBy(x => x.Distance).FirstOrDefault();
                var bestOppPath = paths.OpponentPaths.OrderBy(x => x.Distance).FirstOrDefault();

                var myAll = new double[nodes];
                foreach (var path in paths.MyPaths)
                {
                    foreach (var tile in path.Path)
                    {
                        myAll[tile] += 1;
                    }
                }

                var oppAll = new double[nodes];
                foreach (var path in paths.OpponentPaths)
                {
                    foreach (var tile in path.Path)
                    {
                        oppAll[tile] += 1;
                    }
                }

                ScaleToUnit(myAll);
                ScaleToUnit(oppAll);

                var myBest = new double[nodes];
                foreach (var tile in bestPath?.Path ?? new List<int>())
                {
                    myBest[tile] = 1;
                }

                var oppBest = new double[nodes];
                foreach (var tile in bestOppPath?.Path ?? new List<int>())
                {
                    oppBest[tile] = 1;
                }

                var equilateral = new bool[nodes];
                foreach (var tile in Constants.EquilateralIndexes)
                {
                    equilateral[tile] = true;
                }

                var emptyFraction = board.Count(t => t == 0) / (float)nodes;
                var turnFraction = Math.Min(state.Turn, 40) / 40.0f;

                for (var tileIdx = 0; tileIdx < nodes; tileIdx++)
                {
                    var baseIndex = (b * nodes + tileIdx) * features;
                    var tileValue = board[tileIdx];
                    data[baseIndex + tileValue + 4] = 1.0f;

                    if (allGrow)
                    {
                        data[baseIndex + 9] = 1.0f;
                    }

                    data[baseIndex + 10] = afterOneGrowth.Tiles[tileIdx] / 4.0f;
                    data[baseIndex + 11] = afterTwoGrowths.Tiles[tileIdx] / 4.0f;
                    data[baseIndex + 12] = afterThreeGrowths.Tiles[tileIdx] / 4.0f;
                    data[baseIndex + 13] = (float)myBest[tileIdx];
                    data[baseIndex + 14] = (float)oppBest[tileIdx];
                    data[baseIndex + 15] = (float)myAll[tileIdx];
                    data[baseIndex + 16] = (float)oppAll[tileIdx];
                    data[baseIndex + 17] = equilateral[tileIdx] ? 1.0f : 0.0f;

                    var positive = 0;
                    var negative = 0;
                    var zero = 0;
                    foreach (var neighbor in Constants.AdjacentIndexes[tileIdx])
                    {
                        if (board[neighbor] > 0)
                        {
                            positive++;
                        }
                        else if (board[neighbor] < 0)
                        {
                            negative++;
                        }
                        else
                        {
                            zero++;
                        }
                    }

                    data[baseIndex + 18] = positive / 3.0f;
                    data[baseIndex + 19] = negative / 3.0f;
                    data[baseIndex + 20] = zero / 3.0f;
                    data[baseIndex + 21] = board[Constants.Antipodes[tileIdx]] / 4.0f;
                    data[baseIndex + 22] = emptyFraction;
                    data[baseIndex + 23] = turnFraction;
                }
            }

            return torch.tensor(data, [batchSize, nodes, features]).to(device);
        }

        private static void ScaleToUnit(double[] values)
        {
            var max = values.Max();
            var min = values.Min();
            var range = max - min;

            for (var i = 0; i < values.Length; i++)
            {
                values[i] = range > 0 ? (values[i] - min) / range : 0;
            }
        }

        /// <summary>
        /// [1, 80, 16]
        /// </summary>
        public torch.Tensor Encode(Board state, torch.Device device) => Encode([state], device);
    }
}
using System.Text;
using Volcano.Game;

namespace Volcano.Neural
{
    internal class BoardSample : ISample
    {
        public double[,,] Inputs { get; set; }

        public double[,,] Outputs { get; set; }

        public ISample Clone()
        {
            return new BoardSample
            {
                Inputs = Inputs?.Clone() as double[,,],
                Outputs = Outputs?.Clone() as double[,,]
            };
        }

        public BoardSample()
        {
        }

        public BoardSample(Board board)
        {
            var factor = board.Player == Player.Two ? -1.0 : 1.0;

            Inputs = new double[80 + 80 * 3, 1, 1];
            for (int i = 0; i < 80; i += 4)
            {
                Inputs[i, 0, 0] = factor * board.Tiles[i] / VolcanoGame.Settings.MaxVolcanoLevel;

                var ni = 1;
                foreach (var neighbor in Constants.AdjacentIndexes[i])
                {
                    Inputs[i + ni, 0, 0] = factor * board.Tiles[neighbor] / VolcanoGame.Settings.MaxVolcanoLevel;
                    ni++;
                }
            }

            Outputs = new double[80, 1, 1];
        }

        public BoardSample(Board board, double[] scores)
            : this(board)
        {
            for (int i = 0; i < scores.Length; i++)
            {
                Outputs[i, 0, 0] = scores[i];
            }
        }

        public double[] OutputToArray()
        {
            var tiles = new double[80];

            for (int i = 0; i < 80; i++)
            {
                tiles[i] = Outputs[i, 0, 0];
            }

            return tiles;
        }

        public int OutputToIndex()
        {
            var max = double.MinValue;
            var index = -1;

            for (int i = 0; i < Outputs.GetLength(0); i++)
            {
                if (Outputs[i, 0, 0] > max)
                {
                    max = Outputs[i, 0, 0];
                    index = i;
                }
            }

            return index;
        }

        public string OutputsToString()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < Outputs.GetLength(0); i++)
            {
                sb.Append(Outputs[i, 0, 0]);

                if (i < Outputs.GetLength(0) - 1)
                {
                    sb.Append(", ");
                }
            }

            return sb.ToString();
        }
    }
}
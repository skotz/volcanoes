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
            Inputs = new double[80, 1, 1];
            for (int i = 0; i < 80; i++)
            {
                Inputs[i, 0, 0] = (double)board.Tiles[i] * (board.Player == Player.One ? 1 : -1) / VolcanoGame.Settings.MaxVolcanoLevel;
            }

            Outputs = new double[80, 1, 1];
        }

        public BoardSample(Board board, double[] scores)
        {
            Inputs = new double[80, 1, 1];
            for (int i = 0; i < 80; i++)
            {
                Inputs[i, 0, 0] = (double)board.Tiles[i] * (board.Player == Player.One ? 1 : -1) / VolcanoGame.Settings.MaxVolcanoLevel;
            }

            Outputs = new double[scores.Length, 1, 1];
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
using System;

namespace Volcano.Neural
{
    public class SquaredErrorLoss : ILoss
    {
        public double[,,] Gradients(double[,,] output, double[,,] expected)
        {
            var loss = new double[output.Length, 1, 1];

            for (int i = 0; i < output.Length; i++)
            {
                // dL/d[i] = 2 * (o[i] - e[i]) / 2 = o[i] - e[i]
                loss[i, 0, 0] = output[i, 0, 0] - expected[i, 0, 0];
            }

            return loss;
        }

        public double Total(double[,,] output, double[,,] expected)
        {
            var loss = 0.0;

            for (int i = 0; i < output.Length; i++)
            {
                loss += Math.Pow(output[i, 0, 0] - expected[i, 0, 0], 2) / 2;
            }

            return loss;
        }
    }
}
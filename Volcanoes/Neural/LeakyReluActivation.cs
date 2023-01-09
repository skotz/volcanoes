using System;

namespace Volcano.Neural
{
    public class LeakyReLuActivation : IActivation
    {
        private double _factor = 0.3;

        public double Run(double value)
        {
            return Math.Max(_factor * value, value);
        }

        public double Derivative(double value)
        {
            return value > 0 ? 1 : _factor;
        }

        public double StandardDeviation(int inputs, int outputs)
        {
            // "He" initialization
            return Math.Sqrt(4.0 / (inputs + outputs));
        }
    }
}
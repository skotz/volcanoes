using System;

namespace Volcano.Neural
{
    public class GaussianRandom
    {
        private Random _random;
        private double _mean;
        private double _stdDev;

        public GaussianRandom(double mean, double standardDeviation)
        {
            _random = new Random();
            _mean = mean;
            _stdDev = standardDeviation;
        }

        public double NextDouble()
        {
            // https://stackoverflow.com/a/218600
            var u1 = 1.0 - _random.NextDouble();
            var u2 = 1.0 - _random.NextDouble();
            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            return _mean + _stdDev * randStdNormal;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace Volcano.Neural
{
    internal static class Sampling
    {
        /// <summary>Standard normal via Box-Muller.</summary>
        private static double NextNormal(Random rng)
        {
            double u1 = 1.0 - rng.NextDouble();
            double u2 = rng.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }

        /// <summary>Gamma(shape, 1) via Marsaglia-Tsang.</summary>
        private static double NextGamma(Random rng, double shape)
        {
            if (shape < 1.0)
            {
                // Boost a sub-1 shape into the a >= 1 regime, then scale back.
                double u = rng.NextDouble();
                return NextGamma(rng, shape + 1.0) * Math.Pow(u, 1.0 / shape);
            }

            double d = shape - 1.0 / 3.0;
            double c = 1.0 / Math.Sqrt(9.0 * d);
            while (true)
            {
                double x = NextNormal(rng);
                double v = 1.0 + c * x;
                if (v <= 0) continue;
                v = v * v * v;
                double u = rng.NextDouble();
                if (Math.Log(u) < 0.5 * x * x + d - d * v + d * Math.Log(v))
                {
                    return d * v;
                }
            }
        }

        /// <summary>A symmetric Dirichlet(alpha, ..., alpha) sample of length <paramref name="n"/>.</summary>
        public static float[] Dirichlet(Random rng, int n, double alpha)
        {
            float[] sample = new float[n];
            double sum = 0;
            for (int i = 0; i < n; i++)
            {
                double g = NextGamma(rng, alpha);
                sample[i] = (float)g;
                sum += g;
            }
            if (sum <= 0) sum = 1;
            for (int i = 0; i < n; i++) sample[i] /= (float)sum;
            return sample;
        }

        /// <summary>Samples an index from unnormalised non-negative weights.</summary>
        public static int SampleFromWeights(Random rng, double[] weights)
        {
            double total = 0;
            for (int i = 0; i < weights.Length; i++) total += weights[i];

            if (total <= 0)
            {
                // Unreachable while NumSearches >= 1 (the root always accrues visits). Failing
                // loudly beats silently returning an arbitrary, possibly illegal, cell.
                throw new InvalidOperationException("SampleFromWeights: all weights are zero.");
            }

            double target = rng.NextDouble() * total;
            double running = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                running += weights[i];
                // Strictly greater, and only for a positive weight: with >= a zero-weight entry
                // could be returned when target lands exactly on a cumulative boundary (e.g.
                // target == 0), which would mean playing an illegal move.
                if (weights[i] > 0 && running > target) return i;
            }
            // Floating-point round-off can leave `running` a hair under `target`; fall back to
            // the last action that actually has weight.
            for (int i = weights.Length - 1; i >= 0; i--)
            {
                if (weights[i] > 0) return i;
            }
            return 0;
        }

        /// <summary>Picks a uniformly random index among those marked legal.</summary>
        public static int SampleUniformLegal(Random rng, bool[] validMoves, int validCount)
        {
            int pick = rng.Next(validCount);
            for (int i = 0; i < validMoves.Length; i++)
            {
                if (!validMoves[i]) continue;
                if (pick == 0) return i;
                pick--;
            }
            throw new InvalidOperationException(
                $"SampleUniformLegal: validCount ({validCount}) disagrees with the mask.");
        }
    }
}
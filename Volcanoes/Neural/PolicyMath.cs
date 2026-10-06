namespace Volcano.Neural
{
    internal static class PolicyMath
    {
        /// <summary>Zeroes illegal moves and renormalises in place. Falls back to uniform-over-legal.</summary>
        public static void MaskAndNormalize(float[] policy, bool[] validMoves)
        {
            float sum = 0;
            for (int i = 0; i < policy.Length; i++)
            {
                if (!validMoves[i]) policy[i] = 0f;
                sum += policy[i];
            }

            if (sum > 0)
            {
                for (int i = 0; i < policy.Length; i++) policy[i] /= sum;
                return;
            }

            // The network put all its mass on illegal moves: spread evenly over the legal ones.
            int legal = 0;
            for (int i = 0; i < validMoves.Length; i++) if (validMoves[i]) legal++;
            if (legal == 0) return;
            for (int i = 0; i < policy.Length; i++) policy[i] = validMoves[i] ? 1f / legal : 0f;
        }

        /// <summary>Blends Dirichlet exploration noise into a root policy.</summary>
        public static void AddDirichletNoise(float[] policy, float[] noise, double epsilon)
        {
            var ni = 0;
            for (int i = 0; i < policy.Length; i++)
            {
                if (policy[i] != 0)
                {
                    policy[i] = (float)((1 - epsilon) * policy[i] + epsilon * noise[ni++]);
                }
            }
        }
    }
}
using System;
using System.IO;

namespace Volcano.Engine.Neural
{
    public class NeuralNetwork
    {
        private Random random = new Random();

        // Layer 1: 726 → 128
        public double[,] w1;

        private double[] b1;
        private double[,] w1_grad;
        private double[] b1_grad;

        // Layer 2: 128 → 80
        public double[,] w2;

        private double[] b2;
        private double[,] w2_grad;
        private double[] b2_grad;

        // Cached activations for backprop
        private double[] a0; // input

        private double[] z1; // pre-activation
        private double[] a1; // activation
        private double[] z2; // output layer (linear)

        private double learningRate;
        private const double L2_REGULARIZATION = 0; // 0.0001;

        public NeuralNetwork(double learningRate = 0.001)
        {
            this.learningRate = learningRate;
            InitializeWeights();
        }

        private void InitializeWeights()
        {
            // He initialization for ReLU
            HeInit(out w1, out b1, 726, 128);
            HeInit(out w2, out b2, 128, 80);

            w1_grad = new double[726, 128];
            b1_grad = new double[128];
            w2_grad = new double[128, 80];
            b2_grad = new double[80];

            a0 = new double[726];
            z1 = new double[128];
            a1 = new double[128];
            z2 = new double[80];
        }

        private void HeInit(out double[,] w, out double[] b, int inputSize, int outputSize)
        {
            w = new double[inputSize, outputSize];
            b = new double[outputSize];

            double stdDev = Math.Sqrt(2.0 / inputSize);

            for (int i = 0; i < inputSize; i++)
            {
                for (int j = 0; j < outputSize; j++)
                {
                    w[i, j] = GaussianRandom() * stdDev;
                }
            }

            for (int j = 0; j < outputSize; j++)
            {
                b[j] = 0.0;
            }
        }

        private double GaussianRandom()
        {
            double u1 = random.NextDouble();
            double u2 = random.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }

        public double[] Forward(double[] input)
        {
            Array.Copy(input, a0, Math.Min(input.Length, a0.Length));

            // Layer 1: 726 → 128 (ReLU)
            MatrixVectorMultiply(a0, w1, b1, z1);
            ApplyReLU(z1, a1);

            // Layer 2: 128 → 80 (Linear output)
            MatrixVectorMultiply(a1, w2, b2, z2);

            double[] output = new double[80];
            Array.Copy(z2, output, 80);
            return output;
        }

        public void Backward(double[] loss, double learningRateScale = 1.0)
        {
            // loss is already the gradient dL/dz2 (output layer)
            double[] dz2 = loss;

            // Gradient for w2 and b2
            Array.Clear(w2_grad, 0, w2_grad.Length);
            Array.Clear(b2_grad, 0, b2_grad.Length);

            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 80; j++)
                {
                    w2_grad[i, j] = a1[i] * dz2[j];
                }
            }

            for (int j = 0; j < 80; j++)
            {
                b2_grad[j] = dz2[j];
            }

            // Backprop to layer 1
            double[] da1 = new double[128];
            for (int i = 0; i < 128; i++)
            {
                da1[i] = 0.0;
                for (int j = 0; j < 80; j++)
                {
                    da1[i] += w2[i, j] * dz2[j];
                }
            }

            // ReLU derivative
            double[] dz1 = new double[128];
            for (int i = 0; i < 128; i++)
            {
                dz1[i] = z1[i] > 0 ? 1 : 0.1; // z1[i] > 0 ? da1[i] : 0.0;
            }

            // Gradient for w1 and b1
            Array.Clear(w1_grad, 0, w1_grad.Length);
            Array.Clear(b1_grad, 0, b1_grad.Length);

            for (int i = 0; i < 726; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    w1_grad[i, j] = a0[i] * dz1[j];
                }
            }

            for (int j = 0; j < 128; j++)
            {
                b1_grad[j] = dz1[j];
            }

            // Update weights
            UpdateWeights(w1, w1_grad, b1, b1_grad, learningRateScale);
            UpdateWeights(w2, w2_grad, b2, b2_grad, learningRateScale);
        }

        // Computes local gradients using the internal cached activations from the last Forward pass
        public void ComputeGradients(double[] lossGradient, out double[,] outW1Grad, out double[] outB1Grad, out double[,] outW2Grad, out double[] outB2Grad)
        {
            double[] dz2 = lossGradient;
            outW2Grad = new double[128, 80];
            outB1Grad = new double[128];
            outW1Grad = new double[726, 128];
            outB2Grad = (double[])dz2.Clone();

            // Layer 2 gradients
            for (int i = 0; i < 128; i++)
                for (int j = 0; j < 80; j++)
                    outW2Grad[i, j] = a1[i] * dz2[j];

            // Backprop to Layer 1
            double[] da1 = new double[128];
            for (int i = 0; i < 128; i++)
                for (int j = 0; j < 80; j++)
                    da1[i] += w2[i, j] * dz2[j];

            // Fixed LeakyReLU derivative logic (0.1 multiplier)
            double[] dz1 = new double[128];
            for (int i = 0; i < 128; i++)
                dz1[i] = z1[i] > 0 ? da1[i] : da1[i] * 0.1;

            // Layer 1 gradients
            for (int i = 0; i < 726; i++)
                for (int j = 0; j < 128; j++)
                    outW1Grad[i, j] = a0[i] * dz1[j];

            for (int j = 0; j < 128; j++)
                outB1Grad[j] = dz1[j];
        }

        // Applies accumulated batch gradients with optional scaling
        public void ApplyMiniBatchUpdates(double[,] aggregatedW1G, double[] aggregatedB1G, double[,] aggregatedW2G, double[] aggregatedB2G, double batchScale)
        {
            double effectiveRate = learningRate * batchScale;
            const double GRAD_CLIP = 1.0;

            // Remember to set L2_REGULARIZATION to 0.0 at the top of your class!

            // Update Layer 1
            for (int i = 0; i < w1.GetLength(0); i++)
            {
                for (int j = 0; j < w1.GetLength(1); j++)
                {
                    double clippedGrad = Math.Max(-GRAD_CLIP, Math.Min(GRAD_CLIP, aggregatedW1G[i, j]));
                    double l2Grad = 2 * L2_REGULARIZATION * w1[i, j];
                    w1[i, j] -= effectiveRate * (clippedGrad + l2Grad);
                }
            }
            for (int j = 0; j < b1.Length; j++)
            {
                double clippedGrad = Math.Max(-GRAD_CLIP, Math.Min(GRAD_CLIP, aggregatedB1G[j]));
                b1[j] -= effectiveRate * clippedGrad;
            }

            // Update Layer 2
            for (int i = 0; i < w2.GetLength(0); i++)
            {
                for (int j = 0; j < w2.GetLength(1); j++)
                {
                    double clippedGrad = Math.Max(-GRAD_CLIP, Math.Min(GRAD_CLIP, aggregatedW2G[i, j]));
                    double l2Grad = 2 * L2_REGULARIZATION * w2[i, j];
                    w2[i, j] -= effectiveRate * (clippedGrad + l2Grad);
                }
            }
            for (int j = 0; j < b2.Length; j++)
            {
                double clippedGrad = Math.Max(-GRAD_CLIP, Math.Min(GRAD_CLIP, aggregatedB2G[j]));
                b2[j] -= effectiveRate * clippedGrad;
            }
        }

        private void UpdateWeights(double[,] w, double[,] w_grad, double[] b, double[] b_grad, double learningRateScale)
        {
            double effectiveRate = learningRate * learningRateScale;
            const double GRAD_CLIP = 1.0; // Clip gradients to [-1.0, 1.0]

            for (int i = 0; i < w.GetLength(0); i++)
            {
                for (int j = 0; j < w.GetLength(1); j++)
                {
                    // Gradient clipping
                    double clippedGrad = Math.Max(-GRAD_CLIP, Math.Min(GRAD_CLIP, w_grad[i, j]));

                    // L2 regularization gradient
                    double l2Grad = 2 * L2_REGULARIZATION * w[i, j];
                    w[i, j] -= effectiveRate * (clippedGrad + l2Grad);
                }
            }

            for (int j = 0; j < b.Length; j++)
            {
                // Gradient clipping for biases
                double clippedGrad = Math.Max(-GRAD_CLIP, Math.Min(GRAD_CLIP, b_grad[j]));
                b[j] -= effectiveRate * clippedGrad;
            }
        }

        private void MatrixVectorMultiply(double[] v, double[,] m, double[] b, double[] result)
        {
            for (int j = 0; j < m.GetLength(1); j++)
            {
                result[j] = b[j];
                for (int i = 0; i < v.Length; i++)
                {
                    result[j] += v[i] * m[i, j];
                }
            }
        }

        private void ApplyReLU(double[] input, double[] output)
        {
            for (int i = 0; i < input.Length; i++)
            {
                // leaky relu
                output[i] = Math.Max(0.1 * input[i], input[i]); // Math.Max(0.0, input[i]);
            }
        }

        public void Save(string filePath)
        {
            // Retry logic for file locks
            int maxRetries = 10;
            int retryCount = 0;
            const int initialDelayMs = 100;

            while (retryCount < maxRetries)
            {
                try
                {
                    using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
                    {
                        SaveMatrix(writer, w1);
                        SaveArray(writer, b1);
                        SaveMatrix(writer, w2);
                        SaveArray(writer, b2);
                    }
                    return;
                }
                catch (IOException) when (retryCount < maxRetries - 1)
                {
                    retryCount++;
                    int delayMs = initialDelayMs * (int)Math.Pow(2, retryCount - 1);
                    System.Threading.Thread.Sleep(delayMs);
                }
            }

            // Final attempt without catch
            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                SaveMatrix(writer, w1);
                SaveArray(writer, b1);
                SaveMatrix(writer, w2);
                SaveArray(writer, b2);
            }
        }

        public void Load(string filePath)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                LoadMatrix(reader, w1);
                LoadArray(reader, b1);
                LoadMatrix(reader, w2);
                LoadArray(reader, b2);
            }
        }

        public void CopyWeightsFrom(NeuralNetwork other)
        {
            // Copy weights and biases from other network
            Array.Copy(other.w1, this.w1, other.w1.Length);
            Array.Copy(other.b1, this.b1, other.b1.Length);
            Array.Copy(other.w2, this.w2, other.w2.Length);
            Array.Copy(other.b2, this.b2, other.b2.Length);
        }

        private void SaveMatrix(BinaryWriter writer, double[,] matrix)
        {
            writer.Write(matrix.GetLength(0));
            writer.Write(matrix.GetLength(1));
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    writer.Write(matrix[i, j]);
                }
            }
        }

        private void LoadMatrix(BinaryReader reader, double[,] matrix)
        {
            int rows = reader.ReadInt32();
            int cols = reader.ReadInt32();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = reader.ReadDouble();
                }
            }
        }

        private void SaveArray(BinaryWriter writer, double[] array)
        {
            writer.Write(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                writer.Write(array[i]);
            }
        }

        private void LoadArray(BinaryReader reader, double[] array)
        {
            int length = reader.ReadInt32();
            for (int i = 0; i < length; i++)
            {
                array[i] = reader.ReadDouble();
            }
        }
    }
}
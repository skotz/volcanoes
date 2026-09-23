using System;
using System.IO;
using Volcano.Game;

namespace Volcano.Engine.Neural
{
    public class NeuralNetwork
    {
        private Random random = new Random();

        private const double L2_REGULARIZATION = 0;
        private const double GRAD_CLIP = 1.0;
        private const double LEAKY_RELU_ALPHA = 0.1;
        private const double BN_MOMENTUM = 0.9;
        private const double BN_EPSILON = 1e-5;

        private const int NUM_TILES = 80;
        private const int NUM_PIECE_CHANNELS = 9;
        private const int NUM_TURN_CHANNELS = 4;
        private const int INPUT_CHANNELS = NUM_PIECE_CHANNELS + NUM_TURN_CHANNELS;
        private const int CONV_OUTPUT_CHANNELS = 32;
        private const int CONV_OUTPUT_SIZE = NUM_TILES * CONV_OUTPUT_CHANNELS;
        private const int DENSE1_SIZE = 128;
        private const int OUTPUT_SIZE = 80;

        private double learningRate;
        private bool trainingMode = true;

        private TriangularConvolution convLayer;
        private BatchNorm bn1;
        private BatchNorm bn2;

        private double[,] w1;
        private double[] b1;
        private double[,] w2;
        private double[] b2;

        private double[] a0;
        private double[] convOut;
        private double[] convFlat;
        private double[] bn1Out;
        private double[] z1;
        private double[] a1;
        private double[] bn2Out;
        private double[] z2;

        private double[,] w1_grad;
        private double[] b1_grad;
        private double[,] w2_grad;
        private double[] b2_grad;

        public NeuralNetwork(double learningRate = 0.001)
        {
            this.learningRate = learningRate;
            InitializeWeights();
        }

        private void InitializeWeights()
        {
            convLayer = new TriangularConvolution(INPUT_CHANNELS, CONV_OUTPUT_CHANNELS, NUM_TILES, random);

            bn1 = new BatchNorm(CONV_OUTPUT_SIZE, BN_MOMENTUM, BN_EPSILON);
            bn2 = new BatchNorm(DENSE1_SIZE, BN_MOMENTUM, BN_EPSILON);

            HeInit(out w1, out b1, CONV_OUTPUT_SIZE, DENSE1_SIZE);
            HeInit(out w2, out b2, DENSE1_SIZE, OUTPUT_SIZE);

            w1_grad = new double[CONV_OUTPUT_SIZE, DENSE1_SIZE];
            b1_grad = new double[DENSE1_SIZE];
            w2_grad = new double[DENSE1_SIZE, OUTPUT_SIZE];
            b2_grad = new double[OUTPUT_SIZE];

            a0 = new double[NUM_TILES * INPUT_CHANNELS];
            convOut = new double[CONV_OUTPUT_SIZE];
            convFlat = new double[CONV_OUTPUT_SIZE];
            bn1Out = new double[CONV_OUTPUT_SIZE];
            z1 = new double[DENSE1_SIZE];
            a1 = new double[DENSE1_SIZE];
            bn2Out = new double[DENSE1_SIZE];
            z2 = new double[OUTPUT_SIZE];
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

        public void SetTrainingMode(bool training)
        {
            trainingMode = training;
            bn1.SetTrainingMode(training);
            bn2.SetTrainingMode(training);
        }

        public double[] Forward(double[] input)
        {
            Array.Copy(input, a0, Math.Min(input.Length, a0.Length));

            convLayer.Forward(a0, convOut);
            Array.Copy(convOut, convFlat, CONV_OUTPUT_SIZE);

            MatrixVectorMultiply(convFlat, w1, b1, z1);
            ApplyLeakyReLU(z1, a1);

            MatrixVectorMultiply(a1, w2, b2, z2);

            double[] output = new double[OUTPUT_SIZE];
            Array.Copy(z2, output, OUTPUT_SIZE);
            return output;
        }

        public void ComputeGradients(double[] lossGradient, out double[,] outConvWGrad, out double[,] outW1Grad, out double[] outB1Grad, out double[,] outW2Grad, out double[] outB2Grad)
        {
            double[] dz2 = lossGradient;
            outW2Grad = new double[DENSE1_SIZE, OUTPUT_SIZE];
            outB2Grad = (double[])dz2.Clone();

            for (int i = 0; i < DENSE1_SIZE; i++)
                for (int j = 0; j < OUTPUT_SIZE; j++)
                    outW2Grad[i, j] = a1[i] * dz2[j];

            double[] dA1 = new double[DENSE1_SIZE];
            for (int i = 0; i < DENSE1_SIZE; i++)
            {
                dA1[i] = 0.0;
                for (int j = 0; j < OUTPUT_SIZE; j++)
                    dA1[i] += w2[i, j] * dz2[j];
            }

            double[] dZ1 = new double[DENSE1_SIZE];
            for (int i = 0; i < DENSE1_SIZE; i++)
                dZ1[i] = z1[i] > 0 ? dA1[i] : dA1[i] * LEAKY_RELU_ALPHA;

            outB1Grad = new double[DENSE1_SIZE];
            outW1Grad = new double[CONV_OUTPUT_SIZE, DENSE1_SIZE];
            for (int i = 0; i < CONV_OUTPUT_SIZE; i++)
                for (int j = 0; j < DENSE1_SIZE; j++)
                    outW1Grad[i, j] = convFlat[i] * dZ1[j];

            for (int j = 0; j < DENSE1_SIZE; j++)
                outB1Grad[j] = dZ1[j];

            double[] dConvFlat = new double[CONV_OUTPUT_SIZE];
            for (int i = 0; i < CONV_OUTPUT_SIZE; i++)
            {
                dConvFlat[i] = 0.0;
                for (int j = 0; j < DENSE1_SIZE; j++)
                    dConvFlat[i] += w1[i, j] * dZ1[j];
            }

            outConvWGrad = convLayer.Backward(a0, dConvFlat);
        }

        public void ApplyMiniBatchUpdates(double[,] aggregatedConvWG, double[,] aggregatedW1G, double[] aggregatedB1G, double[,] aggregatedW2G, double[] aggregatedB2G, double batchScale)
        {
            double effectiveRate = learningRate * batchScale;

            convLayer.ApplyWeightUpdates(aggregatedConvWG, effectiveRate);

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

        private void ApplyLeakyReLU(double[] input, double[] output)
        {
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = Math.Max(LEAKY_RELU_ALPHA * input[i], input[i]);
            }
        }

        public void Save(string filePath)
        {
            int maxRetries = 10;
            int retryCount = 0;
            const int initialDelayMs = 100;

            while (retryCount < maxRetries)
            {
                try
                {
                    using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
                    {
                        writer.Write(2);

                        convLayer.Save(writer);

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

            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                writer.Write(2);

                convLayer.Save(writer);

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
                int version = reader.ReadInt32();
                if (version >= 1)
                {
                    convLayer.Load(reader);
                }
                if (version >= 2)
                {

                }

                LoadMatrix(reader, w1);
                LoadArray(reader, b1);
                LoadMatrix(reader, w2);
                LoadArray(reader, b2);
            }
        }

        public void CopyWeightsFrom(NeuralNetwork other)
        {
            Array.Copy(other.w1, this.w1, other.w1.Length);
            Array.Copy(other.b1, this.b1, other.b1.Length);
            Array.Copy(other.w2, this.w2, other.w2.Length);
            Array.Copy(other.b2, this.b2, other.b2.Length);
            convLayer.CopyWeightsFrom(other.convLayer);
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

    public class TriangularConvolution
    {
        private int inputChannels;
        private int outputChannels;
        private int numTiles;
        private Random random;

        private double[,] weights;
        private double[] bias;
        private double[,] weightGradientAccum;

        public TriangularConvolution(int inputChannels, int outputChannels, int numTiles, Random random)
        {
            this.inputChannels = inputChannels;
            this.outputChannels = outputChannels;
            this.numTiles = numTiles;
            this.random = random;

            int aggregationSize = 4 * inputChannels;
            weights = new double[aggregationSize, outputChannels];
            bias = new double[numTiles * outputChannels];
            weightGradientAccum = new double[aggregationSize, outputChannels];

            double stdDev = Math.Sqrt(2.0 / aggregationSize);
            for (int i = 0; i < aggregationSize; i++)
            {
                for (int j = 0; j < outputChannels; j++)
                {
                    weights[i, j] = GaussianRandom() * stdDev;
                }
            }

            for (int i = 0; i < bias.Length; i++)
                bias[i] = 0.0;
        }

        private double GaussianRandom()
        {
            double u1 = random.NextDouble();
            double u2 = random.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }

        public void Forward(double[] input, double[] output)
        {
            Array.Clear(output, 0, output.Length);

            for (int tileIdx = 0; tileIdx < numTiles; tileIdx++)
            {
                int[] neighbors = Constants.AdjacentIndexes[tileIdx];

                for (int outCh = 0; outCh < outputChannels; outCh++)
                {
                    double sum = bias[tileIdx * outputChannels + outCh];

                    double accum = 0.0;
                    for (int ch = 0; ch < inputChannels; ch++)
                    {
                        accum += input[tileIdx * inputChannels + ch] * weights[ch, outCh];
                    }

                    for (int nIdx = 0; nIdx < 3; nIdx++)
                    {
                        int neighborTile = neighbors[nIdx];
                        for (int ch = 0; ch < inputChannels; ch++)
                        {
                            int weightRow = inputChannels + nIdx * inputChannels + ch;
                            accum += input[neighborTile * inputChannels + ch] * weights[weightRow, outCh];
                        }
                    }

                    sum += accum;
                    output[tileIdx * outputChannels + outCh] = sum;
                }
            }
        }

        public double[,] Backward(double[] input, double[] dOutput)
        {
            int aggregationSize = 4 * inputChannels;
            double[,] convWeightGrad = new double[aggregationSize, outputChannels];

            for (int tileIdx = 0; tileIdx < numTiles; tileIdx++)
            {
                int[] neighbors = Constants.AdjacentIndexes[tileIdx];

                for (int outCh = 0; outCh < outputChannels; outCh++)
                {
                    double grad = dOutput[tileIdx * outputChannels + outCh];

                    for (int ch = 0; ch < inputChannels; ch++)
                    {
                        convWeightGrad[ch, outCh] += input[tileIdx * inputChannels + ch] * grad;
                    }

                    for (int nIdx = 0; nIdx < 3; nIdx++)
                    {
                        int neighborTile = neighbors[nIdx];
                        for (int ch = 0; ch < inputChannels; ch++)
                        {
                            int weightRow = inputChannels + nIdx * inputChannels + ch;
                            convWeightGrad[weightRow, outCh] += input[neighborTile * inputChannels + ch] * grad;
                        }
                    }
                }
            }

            return convWeightGrad;
        }

        public void ApplyWeightUpdates(double[,] gradients, double effectiveRate)
        {
            const double GRAD_CLIP = 1.0;
            const double L2_REGULARIZATION = 0;

            for (int i = 0; i < weights.GetLength(0); i++)
            {
                for (int j = 0; j < weights.GetLength(1); j++)
                {
                    double clippedGrad = Math.Max(-GRAD_CLIP, Math.Min(GRAD_CLIP, gradients[i, j]));
                    double l2Grad = 2 * L2_REGULARIZATION * weights[i, j];
                    weights[i, j] -= effectiveRate * (clippedGrad + l2Grad);
                }
            }
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(weights.GetLength(0));
            writer.Write(weights.GetLength(1));
            for (int i = 0; i < weights.GetLength(0); i++)
            {
                for (int j = 0; j < weights.GetLength(1); j++)
                {
                    writer.Write(weights[i, j]);
                }
            }

            writer.Write(bias.Length);
            for (int i = 0; i < bias.Length; i++)
            {
                writer.Write(bias[i]);
            }
        }

        public void Load(BinaryReader reader)
        {
            int rows = reader.ReadInt32();
            int cols = reader.ReadInt32();
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    weights[i, j] = reader.ReadDouble();
                }
            }

            int biasLen = reader.ReadInt32();
            for (int i = 0; i < biasLen; i++)
            {
                bias[i] = reader.ReadDouble();
            }
        }

        public void CopyWeightsFrom(TriangularConvolution other)
        {
            Array.Copy(other.weights, this.weights, other.weights.Length);
            Array.Copy(other.bias, this.bias, other.bias.Length);
        }
    }

    public class BatchNorm
    {
        private int size;
        private double momentum;
        private double epsilon;
        private bool trainingMode;

        private double[] gamma;
        private double[] beta;
        private double[] runningMean;
        private double[] runningVar;
        private double[] batchMean;
        private double[] batchVar;
        private double[] normalized;

        private double[] gammaGrad;
        private double[] betaGrad;

        public BatchNorm(int size, double momentum, double epsilon)
        {
            this.size = size;
            this.momentum = momentum;
            this.epsilon = epsilon;
            this.trainingMode = true;

            gamma = new double[size];
            beta = new double[size];
            runningMean = new double[size];
            runningVar = new double[size];
            batchMean = new double[size];
            batchVar = new double[size];
            normalized = new double[size];
            gammaGrad = new double[size];
            betaGrad = new double[size];

            for (int i = 0; i < size; i++)
            {
                gamma[i] = 1.0;
                beta[i] = 0.0;
                runningMean[i] = 0.0;
                runningVar[i] = 1.0;
            }
        }

        public void SetTrainingMode(bool training)
        {
            trainingMode = training;
        }

        public void Forward(double[] input, double[] output, bool training)
        {
            trainingMode = training;

            if (training)
            {
                Array.Clear(batchMean, 0, size);
                for (int i = 0; i < size; i++)
                {
                    batchMean[i] = input[i];
                }

                for (int i = 0; i < size; i++)
                {
                    double diff = input[i] - batchMean[i];
                    batchVar[i] = diff * diff;
                }

                for (int i = 0; i < size; i++)
                {
                    double stdDev = Math.Sqrt(batchVar[i] + epsilon);
                    normalized[i] = (input[i] - batchMean[i]) / stdDev;
                    output[i] = gamma[i] * normalized[i] + beta[i];

                    runningMean[i] = momentum * runningMean[i] + (1 - momentum) * batchMean[i];
                    runningVar[i] = momentum * runningVar[i] + (1 - momentum) * batchVar[i];
                }
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    double stdDev = Math.Sqrt(runningVar[i] + epsilon);
                    output[i] = gamma[i] * (input[i] - runningMean[i]) / stdDev + beta[i];
                }
            }
        }

        public double[] Backward(double[] dOutput, bool training)
        {
            double[] dInput = new double[size];

            if (training)
            {
                Array.Clear(gammaGrad, 0, size);
                Array.Clear(betaGrad, 0, size);

                for (int i = 0; i < size; i++)
                {
                    gammaGrad[i] = dOutput[i] * normalized[i];
                    betaGrad[i] = dOutput[i];

                    double stdDev = Math.Sqrt(batchVar[i] + epsilon);
                    double dNormalized = dOutput[i] * gamma[i];
                    dInput[i] = dNormalized / stdDev;
                }
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    double stdDev = Math.Sqrt(runningVar[i] + epsilon);
                    dInput[i] = dOutput[i] * gamma[i] / stdDev;
                }
            }

            return dInput;
        }

        public void UpdateParameters(double effectiveRate)
        {
            for (int i = 0; i < size; i++)
            {
                gamma[i] -= effectiveRate * gammaGrad[i];
                beta[i] -= effectiveRate * betaGrad[i];
            }
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(size);
            for (int i = 0; i < size; i++) writer.Write(gamma[i]);
            for (int i = 0; i < size; i++) writer.Write(beta[i]);
            for (int i = 0; i < size; i++) writer.Write(runningMean[i]);
            for (int i = 0; i < size; i++) writer.Write(runningVar[i]);
        }

        public void Load(BinaryReader reader)
        {
            int savedSize = reader.ReadInt32();
            for (int i = 0; i < size && i < savedSize; i++) gamma[i] = reader.ReadDouble();
            for (int i = 0; i < size && i < savedSize; i++) beta[i] = reader.ReadDouble();
            for (int i = 0; i < size && i < savedSize; i++) runningMean[i] = reader.ReadDouble();
            for (int i = 0; i < size && i < savedSize; i++) runningVar[i] = reader.ReadDouble();
        }

        public void CopyFrom(BatchNorm other)
        {
            Array.Copy(other.gamma, this.gamma, size);
            Array.Copy(other.beta, this.beta, size);
            Array.Copy(other.runningMean, this.runningMean, size);
            Array.Copy(other.runningVar, this.runningVar, size);
        }
    }
}

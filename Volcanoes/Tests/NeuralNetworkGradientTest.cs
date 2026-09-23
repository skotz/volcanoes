using System;
using System.Collections.Generic;
using Volcano.Engine.Neural;
using Volcano.Game;

namespace Volcano.Engine.Tests
{
    public class NeuralNetworkGradientTest
    {
        public static void TestSparseGradientFlow()
        {
            Console.WriteLine("Testing sparse gradient propagation through triangular convolution...");

            var network = new NeuralNetwork(0.001);
            int inputSize = 80 * 13;

            double[] input = new double[inputSize];
            for (int i = 0; i < inputSize; i++)
            {
                input[i] = 0.1;
            }

            network.SetTrainingMode(true);
            double[] output = network.Forward(input);

            double[] sparseGradient = new double[80];
            sparseGradient[42] = 1.0;

            Console.WriteLine($"Output at action 42: {output[42]:F6}");
            Console.WriteLine($"Sparse gradient only at position 42: {sparseGradient[42]}");

            try
            {
                network.ComputeGradients(sparseGradient, 
                    out double[,] convWGrad, 
                    out double[,] w1Grad, 
                    out double[] b1Grad, 
                    out double[,] w2Grad, 
                    out double[] b2Grad);

                bool nonZeroConvGrad = false;
                for (int i = 0; i < convWGrad.GetLength(0); i++)
                {
                    for (int j = 0; j < convWGrad.GetLength(1); j++)
                    {
                        if (Math.Abs(convWGrad[i, j]) > 1e-10)
                        {
                            nonZeroConvGrad = true;
                            break;
                        }
                    }
                }

                bool nonZeroW1Grad = false;
                for (int i = 0; i < w1Grad.GetLength(0); i++)
                {
                    for (int j = 0; j < w1Grad.GetLength(1); j++)
                    {
                        if (Math.Abs(w1Grad[i, j]) > 1e-10)
                        {
                            nonZeroW1Grad = true;
                            break;
                        }
                    }
                }

                Console.WriteLine($"Conv weight gradients non-zero: {nonZeroConvGrad}");
                Console.WriteLine($"Dense1 weight gradients non-zero: {nonZeroW1Grad}");
                Console.WriteLine($"B1 gradient sum: {SumArray(b1Grad):F6}");
                Console.WriteLine($"B2 gradient at position 42: {b2Grad[42]:F6}");

                bool hasInfOrNan = false;
                for (int i = 0; i < convWGrad.GetLength(0); i++)
                {
                    for (int j = 0; j < convWGrad.GetLength(1); j++)
                    {
                        if (double.IsNaN(convWGrad[i, j]) || double.IsInfinity(convWGrad[i, j]))
                        {
                            hasInfOrNan = true;
                        }
                    }
                }

                if (!hasInfOrNan && nonZeroConvGrad && nonZeroW1Grad)
                {
                    Console.WriteLine("✓ Gradient flow test PASSED");
                }
                else
                {
                    Console.WriteLine("✗ Gradient flow test FAILED");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Exception during gradient computation: {ex.Message}");
            }
        }

        private static double SumArray(double[] arr)
        {
            double sum = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                sum += arr[i];
            }
            return sum;
        }
    }
}

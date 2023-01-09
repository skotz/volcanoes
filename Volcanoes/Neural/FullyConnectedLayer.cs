using System;
using System.IO;

namespace Volcano.Neural
{
    public class FullyConnectedLayer : ILayer
    {
        private GaussianRandom _random;
        private IActivation _activationFunction;
        private int _previousLayerSize;
        private int _numberOfNeurons;

        private double[,] _weights;
        private double[] _biases;
        private double[,,] _inputs;
        private double[,,] _activations;

        public FullyConnectedLayer(int previousLayerSize, int numberOfNeurons, IActivation activation)
        {
            _activationFunction = activation;
            _previousLayerSize = previousLayerSize;
            _numberOfNeurons = numberOfNeurons;

            var stdDev = _activationFunction.StandardDeviation(previousLayerSize, numberOfNeurons);
            _random = new GaussianRandom(0, stdDev);

            _weights = new double[numberOfNeurons, previousLayerSize];
            _biases = new double[numberOfNeurons];

            for (int n = 0; n < numberOfNeurons; n++)
            {
                for (int p = 0; p < previousLayerSize; p++)
                {
                    _weights[n, p] = _random.NextDouble();
                }
            }
        }

        public double[,,] FeedForward(double[,,] values)
        {
            // Save the inputs for backprop
            _inputs = values;
            _activations = new double[_numberOfNeurons, 1, 1];

            for (int n = 0; n < _numberOfNeurons; n++)
            {
                for (int p = 0; p < _previousLayerSize; p++)
                {
                    _activations[n, 0, 0] += _inputs[p, 0, 0] * _weights[n, p];
                }

                _activations[n, 0, 0] = _activationFunction.Run(_activations[n, 0, 0] + _biases[n]);
            }

            return _activations;
        }

        public double[,,] Backpropagate(double[,,] gradients, double learningRate)
        {
            var nextGradients = new double[_previousLayerSize, 1, 1];

            // Gradients coming in are with respect to the result of the activation function
            for (int n = 0; n < _numberOfNeurons; n++)
            {
                gradients[n, 0, 0] *= _activationFunction.Derivative(_activations[n, 0, 0]);

                // Clip the gradient to avoid diverging to infinity
                gradients[n, 0, 0] = ClipGradient(gradients[n, 0, 0]);
            }

            // Gradients with respect to each weight
            for (int n = 0; n < _numberOfNeurons; n++)
            {
                for (int p = 0; p < _previousLayerSize; p++)
                {
                    var weightGradientNP = gradients[n, 0, 0] * _inputs[p, 0, 0];
                    var inputGradientNP = gradients[n, 0, 0] * _weights[n, p];

                    // Update weights based on gradients
                    _weights[n, p] -= weightGradientNP * learningRate;

                    // Store the correct gradients to pass back to the previous layer
                    nextGradients[p, 0, 0] += inputGradientNP;
                }
            }

            // Gradients for the biases of each neuron
            for (int n = 0; n < _numberOfNeurons; n++)
            {
                // Update biases based on gradients
                _biases[n] -= gradients[n, 0, 0] * learningRate;
            }

            return nextGradients;
        }

        private double ClipGradient(double gradient)
        {
            return Math.Min(Math.Max(gradient, -1), 1);
        }

        public void Save(BinaryWriter writer)
        {
            for (int i = 0; i < _weights.GetLength(0); i++)
            {
                for (int j = 0; j < _weights.GetLength(1); j++)
                {
                    writer.Write(_weights[i, j]);
                }
            }

            for (int i = 0; i < _biases.GetLength(0); i++)
            {
                writer.Write(_biases[i]);
            }
        }

        public void Load(BinaryReader reader)
        {
            for (int i = 0; i < _weights.GetLength(0); i++)
            {
                for (int j = 0; j < _weights.GetLength(1); j++)
                {
                    _weights[i, j] = reader.ReadDouble();
                }
            }

            for (int i = 0; i < _biases.GetLength(0); i++)
            {
                _biases[i] = reader.ReadDouble();
            }
        }
    }
}
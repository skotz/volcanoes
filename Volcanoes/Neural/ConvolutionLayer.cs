using System;
using System.IO;

namespace Volcano.Neural
{
    public class ConvolutionLayer : ILayer
    {
        private GaussianRandom _random;
        private IActivation _activationFunction;

        private int _previousLayerWidth;
        private int _previousLayerHeight;
        private int _previousLayerDepth;

        private int _outputWidth;
        private int _outputHeight;

        private int _featureSize;
        private int _numberOfFeatures;
        private int _pad;
        private int _stride;

        private double[,,,] _kernels;
        private double[] _biases;
        private double[,,] _inputs;
        private double[,,] _activations;

        public ConvolutionLayer(int previousWidth, int previousHeight, int previousDepth, int featureSize, int numberOfFeatures, int stride, IActivation activation, int pad = 0)
        {
            _activationFunction = activation;

            _previousLayerWidth = previousWidth;
            _previousLayerHeight = previousHeight;
            _previousLayerDepth = previousDepth;

            _featureSize = featureSize;
            _numberOfFeatures = numberOfFeatures;

            // TODO
            _pad = pad;
            _stride = stride;

            if (_stride > 1)
            {
                _outputWidth = _previousLayerWidth / stride;
                _outputHeight = _previousLayerHeight / stride;
            }
            else
            {
                _outputWidth = _pad > 0 ? _previousLayerWidth : (_previousLayerWidth - _featureSize + 1);
                _outputHeight = _pad > 0 ? _previousLayerHeight : (_previousLayerHeight - _featureSize + 1);
            }

            var stdDev = _activationFunction.StandardDeviation(previousWidth, numberOfFeatures);
            _random = new GaussianRandom(0, stdDev);

            _kernels = new double[_numberOfFeatures, _featureSize, _featureSize, _previousLayerDepth];
            _biases = new double[_numberOfFeatures];

            for (int f = 0; f < _numberOfFeatures; f++)
            {
                for (int w = 0; w < _featureSize; w++)
                {
                    for (int h = 0; h < _featureSize; h++)
                    {
                        for (int d = 0; d < _previousLayerDepth; d++)
                        {
                            _kernels[f, w, h, d] = _random.NextDouble();
                        }
                    }
                }
            }
        }

        public double[,,] FeedForward(double[,,] values)
        {
            // Save the inputs for backprop
            _inputs = values;
            _activations = new double[_outputWidth, _outputHeight, _numberOfFeatures];

            for (var d = 0; d < _numberOfFeatures; d++)
            {
                // Previous layer output
                var y = -_pad;
                for (var ay = 0; ay < _outputHeight; y += _stride, ay++)
                {
                    var x = -_pad;
                    for (var ax = 0; ax < _outputWidth; x += _stride, ax++)
                    {
                        // Convolve with feature
                        var convolution = 0.0;
                        for (var fy = 0; fy < _featureSize; fy++)
                        {
                            var inputY = y + fy;
                            for (var fx = 0; fx < _featureSize; fx++)
                            {
                                var inputX = x + fx;
                                if (inputY >= 0 && inputY < _previousLayerHeight && inputX >= 0 && inputX < _previousLayerWidth)
                                {
                                    for (var fd = 0; fd < _previousLayerDepth; fd++)
                                    {
                                        convolution += _kernels[d, fx, fy, fd] * _inputs[inputX, inputY, fd];
                                    }
                                }
                            }
                        }

                        // Activate
                        _activations[ax, ay, d] = _activationFunction.Run(convolution + _biases[d]);
                    }
                }
            }

            return _activations;
        }

        public double[,,] Backpropagate(double[,,] gradients, double learningRate)
        {
            var nextGradients = new double[_previousLayerWidth, _previousLayerHeight, _previousLayerDepth];

            // Gradients coming in are with respect to the result of the activation function
            for (int w = 0; w < _outputWidth; w += _stride)
            {
                for (int h = 0; h < _outputHeight; h += _stride)
                {
                    for (int f = 0; f < _numberOfFeatures; f++)
                    {
                        gradients[w, h, f] *= _activationFunction.Derivative(_activations[w, h, f]);

                        // Clip the gradient to avoid diverging to infinity
                        gradients[w, h, f] = ClipGradient(gradients[w, h, f]);
                    }
                }
            }

            var filterGradients = new double[_numberOfFeatures, _featureSize, _featureSize, _previousLayerDepth];

            for (var d = 0; d < _numberOfFeatures; d++)
            {
                var y = -_pad;
                for (var ay = 0; ay < _outputHeight; y += _stride, ay++)
                {
                    var x = -_pad;
                    for (var ax = 0; ax < _outputWidth; x += _stride, ax++)
                    {
                        var gradient = gradients[ax, ay, d];
                        for (var fy = 0; fy < _featureSize; fy++)
                        {
                            var inputY = y + fy;
                            for (var fx = 0; fx < _featureSize; fx++)
                            {
                                var inputX = x + fx;
                                if (inputY >= 0 && inputY < _previousLayerHeight && inputX >= 0 && inputX < _previousLayerWidth)
                                {
                                    for (var fd = 0; fd < _previousLayerDepth; fd++)
                                    {
                                        filterGradients[d, fx, fy, fd] += _inputs[inputX, inputY, fd] * gradient;

                                        // Accumulate gradients to pass back to the previous layer
                                        nextGradients[inputX, inputY, fd] += _kernels[d, fx, fy, fd] * gradient;
                                    }
                                }
                            }
                        }

                        // Clip the gradient to avoid diverging to infinity
                        gradient = ClipGradient(gradient);

                        // Update biases
                        _biases[d] += gradient * learningRate;
                    }
                }
            }

            // Update features based on gradients
            for (var d = 0; d < _numberOfFeatures; d++)
            {
                for (var fy = 0; fy < _featureSize; fy++)
                {
                    for (var fx = 0; fx < _featureSize; fx++)
                    {
                        for (var fd = 0; fd < _previousLayerDepth; fd++)
                        {
                            _kernels[d, fx, fy, fd] -= filterGradients[d, fx, fy, fd] * learningRate;
                        }
                    }
                }
            }

            return nextGradients;
        }

        private double ClipGradient(double gradient)
        {
            return Math.Min(Math.Max(gradient, -1), 1);
        }

        public void Save(BinaryWriter writer)
        {
            for (int i = 0; i < _kernels.GetLength(0); i++)
            {
                for (int j = 0; j < _kernels.GetLength(1); j++)
                {
                    for (int k = 0; k < _kernels.GetLength(2); k++)
                    {
                        for (int l = 0; l < _kernels.GetLength(3); l++)
                        {
                            writer.Write(_kernels[i, j, k, l]);
                        }
                    }
                }
            }

            for (int i = 0; i < _biases.GetLength(0); i++)
            {
                writer.Write(_biases[i]);
            }
        }

        public void Load(BinaryReader reader)
        {
            for (int i = 0; i < _kernels.GetLength(0); i++)
            {
                for (int j = 0; j < _kernels.GetLength(1); j++)
                {
                    for (int k = 0; k < _kernels.GetLength(2); k++)
                    {
                        for (int l = 0; l < _kernels.GetLength(3); l++)
                        {
                            _kernels[i, j, k, l] = reader.ReadDouble();
                        }
                    }
                }
            }

            for (int i = 0; i < _biases.GetLength(0); i++)
            {
                _biases[i] = reader.ReadDouble();
            }
        }
    }
}
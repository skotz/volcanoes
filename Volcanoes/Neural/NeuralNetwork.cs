using System;
using System.Collections.Generic;
using System.IO;

namespace Volcano.Neural
{
    public class NeuralNetwork
    {
        private List<ILayer> _layers;
        private ILoss _lossFunction;
        private double _learningRate;

        public NeuralNetwork(ILoss lossFunction, double learningRate)
        {
            _layers = new List<ILayer>();
            _lossFunction = lossFunction;
            _learningRate = learningRate;
        }

        public void Add(ILayer layer)
        {
            _layers.Add(layer);
        }

        public ISample FeedForward(ISample data)
        {
            var result = data.Clone();
            var activations = result.Inputs;

            foreach (var layer in _layers)
            {
                activations = layer.FeedForward(activations);
            }

            result.Outputs = activations;

            return result;
        }

        public double Train(List<ISample> samples)
        {
            var totalLoss = 0.0;

            // Shuffle samples
            samples.Shuffle();

            foreach (var sample in samples)
            {
                var output = FeedForward(sample);

                // Output layer gradients
                var gradients = _lossFunction.Gradients(output.Outputs, sample.Outputs);

                // Backpropagation
                for (int i = _layers.Count - 1; i >= 0; i--)
                {
                    gradients = _layers[i].Backpropagate(gradients, _learningRate);
                }

                totalLoss += _lossFunction.Total(output.Outputs, sample.Outputs);
            }

            return totalLoss / samples.Count;
        }

        public double TestLoss(List<ISample> samples)
        {
            var totalLoss = 0.0;

            foreach (var sample in samples)
            {
                var output = FeedForward(sample);

                totalLoss += _lossFunction.Total(output.Outputs, sample.Outputs);
            }

            return totalLoss / samples.Count;
        }

        public double TestRate<T>(List<ISample> samples, Func<T, T, bool> isCorrect) where T : ISample
        {
            var correct = 0.0;

            foreach (var sample in samples)
            {
                var output = FeedForward(sample);

                if (isCorrect((T)output, (T)sample))
                {
                    correct++;
                }
            }

            return correct / samples.Count;
        }

        public void Save(Stream stream)
        {
            using (var w = new BinaryWriter(stream))
            {
                foreach (var layer in _layers)
                {
                    layer.Save(w);
                }
            }
        }

        public void Load(Stream stream)
        {
            using (var r = new BinaryReader(stream))
            {
                foreach (var layer in _layers)
                {
                    layer.Load(r);
                }
            }
        }
    }
}
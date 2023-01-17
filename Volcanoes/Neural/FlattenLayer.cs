using System.IO;

namespace Volcano.Neural
{
    public class FlattenLayer : ILayer
    {
        private int _width;
        private int _height;
        private int _depth;

        public FlattenLayer(int width, int height, int depth)
        {
            _width = width;
            _height = height;
            _depth = depth;
        }

        public double[,,] FeedForward(double[,,] values)
        {
            var output = new double[_width * _height * _depth, 1, 1];

            for (int w = 0; w < _width; w++)
            {
                for (int h = 0; h < _height; h++)
                {
                    for (int d = 0; d < _depth; d++)
                    {
                        output[w * _height * _depth + h * _depth + d, 0, 0] = values[w, h, d];
                    }
                }
            }

            return output;
        }

        public double[,,] Backpropagate(double[,,] gradients, double learningRate)
        {
            var output = new double[_width, _height, _depth];

            for (int w = 0; w < _width; w++)
            {
                for (int h = 0; h < _height; h++)
                {
                    for (int d = 0; d < _depth; d++)
                    {
                        output[w, h, d] = gradients[w * _height * _depth + h * _depth + d, 0, 0];
                    }
                }
            }

            return output;
        }

        public void Save(BinaryWriter writer)
        {
        }

        public void Load(BinaryReader reader)
        {
        }
    }
}
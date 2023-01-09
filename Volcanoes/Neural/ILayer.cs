using System.IO;

namespace Volcano.Neural
{
    public interface ILayer
    {
        double[,,] FeedForward(double[,,] values);

        double[,,] Backpropagate(double[,,] gradients, double learningRate);

        void Save(BinaryWriter writer);

        void Load(BinaryReader reader);
    }
}
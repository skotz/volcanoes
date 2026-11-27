using System.Collections.Generic;
using Volcano.Game;

namespace Volcano.Neural
{
    internal class TrainingData
    {
        public List<Board> States { get; } = new();
        public List<double[]> Probs { get; } = new();
        public List<float> Values { get; } = new();

        public int Count => States.Count;

        public void Add(Board state, double[] probs, float value)
        {
            States.Add(state);
            Probs.Add(probs);
            Values.Add(value);
        }

        public void Add(TrainingData other)
        {
            States.AddRange(other.States);
            Probs.AddRange(other.Probs);
            Values.AddRange(other.Values);
        }
    }
}
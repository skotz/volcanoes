using System;
using System.Collections.Generic;
using TorchSharp;
using static TorchSharp.torch;

namespace Volcano.Engine
{
    internal class Transition
    {
        public double[] State { get; set; }
        public int Action { get; set; }
        public double Reward { get; set; }
        public double[] NextState { get; set; }
        public bool Done { get; set; }
        public bool NextTurnIsOpponent { get; set; }

        public Transition(double[] state, int action, double reward, double[] nextState, bool done, bool nextTurnIsOpponent)
        {
            State = state;
            Action = action;
            Reward = reward;
            NextState = nextState;
            Done = done;
            NextTurnIsOpponent = nextTurnIsOpponent;
        }
    }

    internal class TorchSharpReplayBuffer
    {
        private readonly List<Transition> _buffer;
        private readonly int _maxSize;
        private int _position = 0;

        public int Count => _buffer.Count;

        public TorchSharpReplayBuffer(int maxSize)
        {
            _maxSize = maxSize;
            _buffer = new List<Transition>(maxSize);
        }

        public void Add(Transition transition)
        {
            if (_buffer.Count < _maxSize)
            {
                _buffer.Add(transition);
            }
            else
            {
                _buffer[_position] = transition;
            }

            _position = (_position + 1) % _maxSize;
        }

        public List<Transition> SampleMiniBatch(int batchSize)
        {
            if (_buffer.Count < batchSize)
            {
                throw new InvalidOperationException($"Buffer contains {_buffer.Count} transitions but batch size is {batchSize}");
            }

            var batch = new List<Transition>(batchSize);
            var random = new Random();
            var indices = new HashSet<int>();

            while (indices.Count < batchSize)
            {
                indices.Add(random.Next(_buffer.Count));
            }

            foreach (int idx in indices)
            {
                batch.Add(_buffer[idx]);
            }

            return batch;
        }

        public (torch.Tensor states, torch.Tensor actions, torch.Tensor rewards, 
                torch.Tensor nextStates, torch.Tensor dones) 
            GetBatch(List<Transition> transitions, torch.Device device)
        {
            int batchSize = transitions.Count;
            int stateSize = transitions[0].State.Length;

            var statesData = new float[batchSize * stateSize];
            var nextStatesData = new float[batchSize * stateSize];
            var actionsData = new long[batchSize];
            var rewardsData = new float[batchSize];
            var donesData = new float[batchSize];

            for (int i = 0; i < batchSize; i++)
            {
                var t = transitions[i];

                // Copy state and next state
                Array.Copy(Array.ConvertAll(t.State, x => (float)x), 0, statesData, i * stateSize, stateSize);
                Array.Copy(Array.ConvertAll(t.NextState, x => (float)x), 0, nextStatesData, i * stateSize, stateSize);

                actionsData[i] = t.Action;
                rewardsData[i] = (float)t.Reward;
                donesData[i] = t.Done ? 1.0f : 0.0f;
            }

            var states = tensor(statesData).reshape(batchSize, stateSize).to(device);
            var nextStates = tensor(nextStatesData).reshape(batchSize, stateSize).to(device);
            var actions = tensor(actionsData).to(device);
            var rewards = tensor(rewardsData).to(device);
            var dones = tensor(donesData).to(device);

            return (states, actions, rewards, nextStates, dones);
        }

        public void Clear()
        {
            _buffer.Clear();
            _position = 0;
        }
    }
}

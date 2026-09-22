using System;
using System.Collections.Generic;
using System.Linq;

namespace Volcano.Engine.Neural
{
    public class ReplayBuffer
    {
        private List<Transition> buffer;
        private int capacity;
        private Random random;

        public int Count => buffer.Count;

        public ReplayBuffer(int capacity = 5000)
        {
            this.capacity = capacity;
            this.buffer = new List<Transition>(capacity);
            this.random = new Random();
        }

        public void Add(Transition transition)
        {
            if (buffer.Count >= capacity)
            {
                buffer.RemoveAt(0);
            }
            buffer.Add(transition);
        }

        public List<Transition> SampleMiniBatch(int batchSize)
        {
            if (buffer.Count < batchSize)
            {
                return new List<Transition>(buffer);
            }

            List<Transition> batch = new List<Transition>(batchSize);
            HashSet<int> sampled = new HashSet<int>();

            while (batch.Count < batchSize)
            {
                int idx = random.Next(buffer.Count);
                if (!sampled.Contains(idx))
                {
                    sampled.Add(idx);
                    batch.Add(buffer[idx]);
                }
            }

            return batch;
        }

        public void Clear()
        {
            buffer.Clear();
        }
    }
}

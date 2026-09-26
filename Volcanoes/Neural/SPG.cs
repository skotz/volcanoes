using System.Collections.Generic;
using Volcano.Game;

namespace Volcano.Neural
{
    internal class SPG
    {
        public Board State { get; set; }
        public NNNode Root { get; set; }
        public NNNode Node { get; set; }
        public bool Terminated { get; set; }

        private readonly List<Board> statesMemory = new();
        private readonly List<double[]> probsMemory = new();
        private readonly List<int> playersMemory = new();

        public SPG(Game game)
        {
            State = game.GetInitialState();
            Root = null;
            Node = null;
            Terminated = false;
        }

        public void AddEntry(Board state, double[] probs, int player)
        {
            statesMemory.Add(state);
            probsMemory.Add(probs);
            playersMemory.Add(player);
        }

        public IReadOnlyList<Board> GetStates() => statesMemory;

        public IReadOnlyList<double[]> GetProbs() => probsMemory;

        public IReadOnlyList<int> GetPlayers() => playersMemory;
    }
}
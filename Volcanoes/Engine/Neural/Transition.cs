using System;

namespace Volcano.Engine.Neural
{
    public class Transition
    {
        public double[] State { get; set; }
        public int Action { get; set; }
        public double Reward { get; set; }
        public double[] NextState { get; set; }
        public bool Done { get; set; }

        public Transition(double[] state, int action, double reward, double[] nextState, bool done)
        {
            State = state;
            Action = action;
            Reward = reward;
            NextState = nextState;
            Done = done;
        }
    }
}

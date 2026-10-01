using System;
using System.Collections.Generic;
using Volcano.Game;

namespace Volcano.Neural
{
    internal class NNNode
    {
        private readonly GameRule game;
        private readonly AlphaZeroConfig config;
        private readonly NNNode parent;
        private readonly double prior;
        public double valueSum;
        private Board state;

        public int ActionTaken { get; }
        public List<NNNode> Children { get; } = new();
        public int VisitCount { get; private set; }

        // the absolute player of the state before making the desired move
        public Player AbsolutePlayer { get; }

        /// <summary>
        /// The position at this node, derived from the parent on first access and then cached.
        /// Expansion creates a child per legal action — 65 of them here — but a search only ever
        /// visits a handful, so materialising every child's board up front would allocate tens of
        /// gigabytes per move. Nodes are always reached through their parent, which has already
        /// been materialised, so this stays O(1).
        /// </summary>
        public Board State
        {
            get
            {
                if (state == null)
                {
                    var next = game.GetNextState(parent!.State, ActionTaken);
                    state = game.ChangePerspective(next, Player.One);
                }
                return state;
            }
        }

        /// <summary>Creates a search root from a known position.</summary>
        public NNNode(GameRule game, AlphaZeroConfig config, Board state, int visitCount = 0)
        {
            this.game = game;
            this.config = config;
            this.state = state;
            this.parent = null;
            this.ActionTaken = -1;
            this.prior = 0;
            this.VisitCount = visitCount;
            this.AbsolutePlayer = state.GetAbsolutePlayer(); // current state since it's not pulled from parent
        }

        /// <summary>Creates a child; its position is computed lazily from <paramref name="parent"/>.</summary>
        private NNNode(GameRule game, AlphaZeroConfig config, NNNode parent, int actionTaken, double prior)
        {
            this.game = game;
            this.config = config;
            this.state = null;
            this.parent = parent;
            this.ActionTaken = actionTaken;
            this.prior = prior;
            this.VisitCount = 0;
            this.AbsolutePlayer = parent.State.GetAbsolutePlayer(); // parent state since it's after parent move (i.e., it's current state)
        }

        public bool IsFullyExpanded() => Children.Count > 0;

        //public double GetUcb(NNNode child)
        //{
        //    double q = child.VisitCount == 0
        //        ? 0
        //        : ((child.valueSum / child.VisitCount) + 1) / 2;

        //    return q + config.C * child.prior * (Math.Sqrt(VisitCount) / (child.VisitCount + 1));
        //}

        public NNNode Select()
        {
            NNNode best = null;
            double bestUcb = double.MinValue;

            double ucbScalar = config.C * Math.Sqrt(VisitCount);

            for (int i = 0; i < Children.Count; i++)
            {
                double q = Children[i].VisitCount == 0
                    ? 0.5 // draw in [0,1] space
                    : ((Children[i].valueSum / Children[i].VisitCount) + 1.0) / 2.0;
                double u = ucbScalar * Children[i].prior / (Children[i].VisitCount + 1);
                double ucb = q + u;

                if (ucb > bestUcb)
                {
                    best = Children[i];
                    bestUcb = ucb;
                }
            }

            return best!;
        }

        /// <summary>Creates one child per action with non-zero prior. No boards are built here.</summary>
        public void Expand(float[] policy)
        {
            for (int action = 0; action < policy.Length; action++)
            {
                if (policy[action] <= 0) continue;
                Children.Add(new NNNode(game, config, this, action, policy[action]));
            }
        }

        public void Backpropagate(double value)
        {
            var negate = parent?.AbsolutePlayer != AbsolutePlayer ? -1 : 1;

            valueSum += value;
            VisitCount++;
            parent?.Backpropagate(value * negate);
        }
    }
}
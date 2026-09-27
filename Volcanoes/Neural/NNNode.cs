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
        private double valueSum;
        private Board state;

        public int ActionTaken { get; }
        public List<NNNode> Children { get; } = new();
        public int VisitCount { get; private set; }

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
                    Board next = game.GetNextState(parent!.State, ActionTaken);
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
        }

        public bool IsFullyExpanded() => Children.Count > 0;

        public double GetUcb(NNNode child)
        {
            double q = child.VisitCount == 0
                ? 0
                : 1 - ((child.valueSum / child.VisitCount) + 1) / 2;
            return q + config.C * child.prior * (Math.Sqrt(VisitCount) / (child.VisitCount + 1));
        }

        public NNNode Select()
        {
            NNNode best = null;
            double bestUcb = double.MinValue;
            foreach (NNNode child in Children)
            {
                double ucb = GetUcb(child);
                if (ucb > bestUcb)
                {
                    best = child;
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

        public void Backpropagate(Player winner)
        {
            var player = state.Flipped ? (state.Player == Player.One ? Player.Two : Player.One) : state.Player;
            var value = player == winner ? 1 : -1;
            valueSum += value;
            VisitCount++;
            parent?.Backpropagate(winner);
        }

        public void Backpropagate(double value)
        {
            valueSum += value;
            VisitCount++;
            var parentPlayer = parent?.state?.Flipped == true ? (parent?.state?.Player == Player.Two ? Player.One : Player.Two) : parent?.state?.Player;
            var currentPlayer = state?.Flipped == true ? (state?.Player == Player.Two ? Player.One : Player.Two) : state?.Player;
            var negate = parentPlayer != currentPlayer ? -1 : 1;
            parent?.Backpropagate(value * negate);
        }
    }
}
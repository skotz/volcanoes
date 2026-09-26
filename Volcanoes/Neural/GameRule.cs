using Volcano.Game;

namespace Volcano.Neural
{
    internal class GameRule
    {
        public const int Cells = 80;

        public int ActionSize => Cells;

        public Board GetInitialState() => new Board();

        public Board GetNextState(Board state, int action)
        {
            var next = new Board(state);
            next.MakeMove(action);
            return next;
        }

        public bool[] GetValidMoves(Board state)
        {
            var valid = new bool[ActionSize];
            var moves = state.GetMoves();

            foreach (var m in moves)
            {
                valid[m] = true;
            }

            return valid;
        }

        public int CountValidMoves(Board state)
        {
            return state.GetMoves().Count;
        }

        public bool CheckWinner(Board state)
        {
            return state.Winner != Player.Empty;
        }

        /// <summary>
        /// Whether the game has ended after <paramref name="action"/>.
        /// <paramref name="value"/> is 1 if that move won, 0 for a draw or an unfinished game.
        /// </summary>
        public bool GetTerminated(Board state, int action, out int value)
        {
            if (CheckWinner(state))
            {
                value = 1;
                return true;
            }
            if (CountValidMoves(state) == 0)
            {
                value = 0; // draw
                return true;
            }
            value = 0;
            return false;
        }

        public Player GetOpponent(Player player) => player == Player.One ? Player.Two : Player.One;

        public int GetOpponentValue(int value) => -value;

        /// <summary>
        /// Returns the position as seen by <paramref name="player"/>, i.e. with that player's
        /// stones as +1. Search always works in the "side to move is +1" frame.
        /// </summary>
        public Board ChangePerspective(Board state, Player player)
        {
            var next = new Board(state);

            if (next.Player != player)
            {
                next.Turn += 3;
                next.Player = player;
                next.Flipped = !next.Flipped;
                for (int i = 0; i < Cells; i++)
                {
                    next.Tiles[i] *= -1;
                }
            }
            //else if (next.Flipped)
            //{
            //    // second move
            //}

            return next;
        }

        public Board ChangePerspective(Board state)
        {
            return ChangePerspective(state, Player.One);
        }
    }
}
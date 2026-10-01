using System.Collections.Concurrent;
using Volcano.Game;

namespace Volcano.Neural
{
    internal class GameRule
    {
        public const int Cells = 80;

        public int ActionSize => Cells;

        public Board GetInitialState() => new Board();

        private static ConcurrentDictionary<long, Board> NextStateCache = new ConcurrentDictionary<long, Board>();

        public Board GetNextState(Board state, int action)
        {
            var hash = GetBoardHash(state, action);
            if (NextStateCache.TryGetValue(hash, out var cached))
            {
                return cached;
            }

            var next = new Board(state);
            next.MakeMove(action);

            NextStateCache.TryAdd(hash, next);

            return next;
        }

        private long GetBoardHash(Board board, int action)
        {
            var hash = 0L;

            hash ^= Constants.AdditionalZobristKeys[0, board.Turn];
            hash ^= Constants.AdditionalZobristKeys[1, (int)board.Player];
            hash ^= Constants.AdditionalZobristKeys[2, board.Flipped ? 1 : 0];
            hash ^= Constants.AdditionalZobristKeys[3, (int)board.Winner];
            hash ^= Constants.AdditionalZobristKeys[4, action];

            for (int i = 0; i < 80; i++)
            {
                hash ^= Constants.ZobristKeys[i, board.Tiles[i] + 4];
            }

            return hash;
        }

        public bool[] GetValidMoves(Board state)
        {
            var valid = new bool[ActionSize];
            var moves = state.GetMoves();

            //if (state.Turn == 1)
            //{
            //    // on the first move only return one of each type (one edge, one center) since the rest are technically translated duplicates
            //    moves = new List<int> { 0, 1 };
            //}

            foreach (var m in moves)
            {
                valid[m] = true;
            }

            return valid;
        }

        //public int CountValidMoves(Board state)
        //{
        //    return state.GetMoves().Count;
        //}

        //public bool CheckWinner(Board state)
        //{
        //    return state.Winner != Player.Empty;
        //}

        /// <summary>
        /// Whether the game has ended after <paramref name="action"/>.
        /// <paramref name="winner"/> is 1 if that move won, 0 for a draw or an unfinished game.
        /// </summary>
        public bool GetTerminated(Board state, int action, out Player winner)
        {
            //if (state.Winner == Player.Draw)
            //{
            //    winner = Player.Draw;
            //    return true;
            //}
            //else
            if (state.Winner != Player.Empty)
            {
                winner = state.Winner; // state.Flipped ? (state.Winner == Player.Two ? Player.One : Player.Two) : state.Winner;
                return true;
            }
            //else if (CountValidMoves(state) == 0)
            //{
            //    // should be handled in the previous cases
            //    absoluteWinner = Player.Draw;
            //    return true;
            //}
            winner = Player.Empty;
            return false;
        }

        /// <summary>
        /// Returns the position as seen by <paramref name="player"/>, i.e. with that player's
        /// stones as +1. Search always works in the "side to move is +1" frame.
        /// </summary>
        public Board ChangePerspective(Board state, Player player)
        {
            var next = new Board(state);

            if (next.Player == Player.Two)
            {
                next.Turn += next.Flipped ? -3 : 3;
                next.Player = Player.One;
                next.Flipped = !next.Flipped;
                for (int i = 0; i < Cells; i++)
                {
                    next.Tiles[i] *= -1;
                }
            }

            return next;
        }
    }
}
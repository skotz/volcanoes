using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Game
{
    class Board
    {
        public List<Tile> Tiles { get; private set; }
        public Player Player { get; private set; }
        public int Turn { get; private set; }

        public Board()
        {
            Turn = 1;
            Player = Player.Blue;
            Tiles = new List<Tile>();

            for (int outer = 0; outer < 20; outer++)
            {
                int outerRow = outer / 5;
                int outerCol = outer % 5;

                for (int inner = 0; inner < 4; inner++)
                {
                    int index = outer * 4 + inner;
                    string name = (outer + 1).ToString();

                    switch (inner)
                    {
                        case 0:
                            name += "A";
                            break;
                        case 1:
                            name += "B";
                            break;
                        case 2:
                            name += "C";
                            break;
                        case 3:
                            name += "D";
                            break;
                    }

                    Tiles.Add(new Tile
                    {
                        Index = index,
                        Owner = Player.Empty,
                        Value = 0,
                        Name = name
                    });
                }
            }
        }

        public Board(Board copy)
        {
            Tiles = copy.Tiles.Select(x => new Tile(x)).ToList();
            Player = copy.Player;
            Turn = copy.Turn;
        }

        /// <summary>
        /// Make a given move on the board. 
        /// </summary>
        /// <param name="move"></param>
        public void MakeMove(Move move)
        {
            if (IsValidMove(move))
            {
                Tiles[move.TileIndex].Owner = Player;
                Tiles[move.TileIndex].Value += 1;

                // process eruptions
                // check for win
                // note: maintain list of connected tiles to make win check trivial?
            }
        }

        /// <summary>
        /// Get a list of all valid moves for the current player on the current board state.
        /// </summary>
        /// <returns></returns>
        public List<Move> GetMoves()
        {
            List<Move> moves = new List<Move>();

            // TODO: order moves for alpha/beta pruning by 1) growing existing tiles, 2) claiming adjacent tiles, and then 3) claiming remaining tiles

            for (int i = 0; i < 80; i++)
            {
                // Grow existing tiles
                if (Tiles[i].Owner == Player && Tiles[i].Value < Constants.VolcanoEruptionValue)
                {
                    moves.Add(new Move(i, GetMoveTypeForTurn(i)));
                }

                // Claim new tiles
                if (Tiles[i].Owner == Player.Empty)
                {
                    moves.Add(new Move(i, GetMoveTypeForTurn(i)));
                }
            }

            return moves;
        }

        public bool IsValidMove(Move move)
        {
            return GetMoves().Any(x => x == move);
        }
        
        /// <summary>
        /// Get the opponent for a given player.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        private Player GetOpponent(Player current)
        {
            return current == Player.Blue ? Player.Orange : Player.Blue;
        }

        /// <summary>
        /// Which player should move on a given turn number.
        /// </summary>
        /// <param name="turn"></param>
        /// <returns></returns>
        private Player GetPlayerForTurn(int turn)
        {
            return (turn - 1) % 6 <= 2 ? Player.Blue : Player.Orange;
        }

        /// <summary>
        /// Get the type of move a specific turn requires.
        /// </summary>
        /// <param name="turn"></param>
        /// <returns></returns>
        private MoveType GetMoveTypeForTurn(int turn)
        {
            return (turn - 1) % 3 == 1 ? MoveType.AllGrow : MoveType.SingleGrow;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Game
{
    class Board
    {
        // TODO: tile state can be maintained in a simple list of 80 ints where positive numbers are for blue and negative numbers are for orange
        public List<Tile> Tiles { get; private set; }
        public Player Player { get; private set; }
        public int Turn { get; private set; }

        public Board()
        {
            Turn = 1;
            Player = Player.Blue;
            Tiles = new List<Tile>();

            for (int i = 0; i < 80; i++)
            {
                Tiles.Add(new Tile
                {
                    Index = i,
                    Owner = Player.Empty,
                    Value = 0
                });
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
                if (move.MoveType == MoveType.AllGrow)
                {
                    for (int i = 0; i < 80; i++)
                    {
                        if (Tiles[i].Value != 0)
                        {
                            Tiles[i].Value++;
                        }
                    }
                }
                else if (move.MoveType == MoveType.SingleGrow)
                {
                    Tiles[move.TileIndex].Owner = Player;
                    Tiles[move.TileIndex].Value += 1;
                }

                ProcessEruptions();

                Player winner = GetWinner();

                if (winner != Player.Empty)
                {
                    // TODO
                    throw new Exception("WINNER!");
                }

                Turn++;
                Player = GetPlayerForTurn(Turn);

                if (GetMoveTypeForTurn(Turn) == MoveType.AllGrow)
                {
                    MakeMove(new Move(-1, MoveType.AllGrow));
                }
            }
        }

        private void ProcessEruptions()
        {
            bool done = false;
            while (!done)
            {
                done = true;
                for (int i = 0; i < 80; i++)
                {
                    if (Tiles[i].Value >= Constants.VolcanoEruptionValue)
                    {
                        for (int adjacent = 0; adjacent < 3; adjacent++)
                        {
                            Tiles[i].Value = 1;
                            done = false;

                            // Blank tile
                            if (Tiles[Constants.ConnectingTiles[i][adjacent]].Owner == Player.Empty)
                            {
                                Tiles[Constants.ConnectingTiles[i][adjacent]].Owner = Tiles[i].Owner;
                                Tiles[Constants.ConnectingTiles[i][adjacent]].Value = 1;
                            }

                            // Same owner
                            else if (Tiles[Constants.ConnectingTiles[i][adjacent]].Owner == Tiles[i].Owner)
                            {
                                Tiles[Constants.ConnectingTiles[i][adjacent]].Value++;
                            }

                            // Enemy owner
                            else if (Tiles[Constants.ConnectingTiles[i][adjacent]].Owner != Tiles[i].Owner)
                            {
                                Tiles[Constants.ConnectingTiles[i][adjacent]].Value = 1;
                            }
                        }
                    }
                }
            }
        }

        private Player GetWinner()
        {
            Player winner = Player.Empty;

            // TODO: do some path searching to find winners

            return winner;
        }

        /// <summary>
        /// Get a list of all valid moves for the current player on the current board state.
        /// </summary>
        /// <returns></returns>
        public List<Move> GetMoves()
        {
            List<Move> moves = new List<Move>();

            // TODO: order moves for alpha/beta pruning by 1) growing existing tiles, 2) claiming adjacent tiles, and then 3) claiming remaining tiles

            if (GetMoveTypeForTurn(Turn) == MoveType.AllGrow)
            {
                moves.Add(new Move(-1, MoveType.AllGrow));
            }
            else
            {
                for (int i = 0; i < 80; i++)
                {
                    // Grow existing tiles
                    if (Tiles[i].Owner == Player && Tiles[i].Value < Constants.VolcanoEruptionValue)
                    {
                        moves.Add(new Move(i, MoveType.SingleGrow));
                    }

                    // Claim new tiles
                    if (Tiles[i].Owner == Player.Empty)
                    {
                        moves.Add(new Move(i, MoveType.SingleGrow));
                    }
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

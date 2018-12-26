using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Engine;
using Volcano.Search;

namespace Volcano.Game
{
    class Board
    {
        // TODO: tile state can be maintained in a simple list of 80 ints where positive numbers are for blue and negative numbers are for orange
        public List<Tile> Tiles { get; private set; }
        public Player Player { get; private set; }
        public int Turn { get; private set; }

        public Player Winner { get; private set; }
        public List<int> WinningPath { get; private set; }

        public List<int> Eruptions { get; private set; }

        private static PathFinder pathFinder = new PathFinder();

        public GameState State
        {
            get
            {
                return Winner == Player.Empty ? GameState.InProgress : GameState.GameOver;
            }
        }

        public Board()
        {
            Turn = 1;
            Player = Player.One;
            Tiles = new List<Tile>();
            Winner = Player.Empty;
            WinningPath = new List<int>();
            Eruptions = new List<int>();

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
            Winner = copy.Winner;
            WinningPath = copy.WinningPath;
            Eruptions = new List<int>();
        }

        /// <summary>
        /// Make a given move on the board. 
        /// </summary>
        /// <param name="move"></param>
        public bool MakeMove(Move move)
        {
            return MakeMove(move, true, true);
        }

        /// <summary>
        /// Make a given move on the board. 
        /// </summary>
        /// <param name="move"></param>
        public bool MakeMove(Move move, bool checkForWin, bool autoGrow)
        {
            if (move == null)
            {
                return false;
            }

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

            if (checkForWin)
            {
                SearchForWin();
            }

            if (Winner == Player.Empty)
            {
                Turn++;
                Player = GetPlayerForTurn(Turn);

                if (autoGrow && GetMoveTypeForTurn(Turn) == MoveType.AllGrow)
                {
                    MakeMove(new Move(-1, MoveType.AllGrow));
                    return true;
                }
            }

            return false;
        }

        private void ProcessEruptions()
        {
            bool done = false;
            while (!done)
            {
                // Phase one: get a list of deltas from eruptions
                int[] oneDeltas = new int[80];
                int[] twoDeltas = new int[80];
                for (int i = 0; i < 80; i++)
                {
                    if (Tiles[i].Value >= VolcanoGame.Settings.MaxVolcanoLevel)
                    {
                        Eruptions.Add(i);
                        for (int adjacent = 0; adjacent < 3; adjacent++)
                        {
                            // Downgrade to a level one volcano
                            Tiles[i].Value = VolcanoGame.Settings.MaxMagmaChamberLevel + 1;

                            // Blank tile
                            if (Tiles[Constants.ConnectingTiles[i][adjacent]].Owner == Player.Empty)
                            {
                                if (Tiles[i].Owner == Player.One)
                                {
                                    oneDeltas[Constants.ConnectingTiles[i][adjacent]] += VolcanoGame.Settings.EruptOverflowEmptyTileAmount;
                                }
                                else
                                {
                                    twoDeltas[Constants.ConnectingTiles[i][adjacent]] += VolcanoGame.Settings.EruptOverflowEmptyTileAmount;
                                }
                            }

                            // Same owner
                            else if (Tiles[Constants.ConnectingTiles[i][adjacent]].Owner == Tiles[i].Owner)
                            {
                                if (Tiles[i].Owner == Player.One)
                                {
                                    oneDeltas[Constants.ConnectingTiles[i][adjacent]] += VolcanoGame.Settings.EruptOverflowFriendlyTileAmount;
                                }
                                else
                                {
                                    twoDeltas[Constants.ConnectingTiles[i][adjacent]] += VolcanoGame.Settings.EruptOverflowFriendlyTileAmount;
                                }
                            }

                            // Enemy owner
                            else if (Tiles[Constants.ConnectingTiles[i][adjacent]].Owner != Tiles[i].Owner && Tiles[Constants.ConnectingTiles[i][adjacent]].Value > 0)
                            {
                                if (Tiles[i].Owner == Player.One)
                                {
                                    twoDeltas[Constants.ConnectingTiles[i][adjacent]] += VolcanoGame.Settings.EruptOverflowEnemyTileAmount;
                                }
                                else
                                {
                                    oneDeltas[Constants.ConnectingTiles[i][adjacent]] += VolcanoGame.Settings.EruptOverflowEnemyTileAmount;
                                }
                            }
                        }
                    }
                }

                // Phase two: process deltas
                done = true;
                for (int i = 0; i < 80; i++)
                {
                    if (oneDeltas[i] != 0 || twoDeltas[i] != 0)
                    {
                        if (Tiles[i].Value == 0)
                        {
                            // When the tile is empty it's possible to get multiple positive deltas so we need them to cancel out
                            Tiles[i].Value += Math.Abs(oneDeltas[i] - twoDeltas[i]);

                            // The player with the higher delta will own the tile
                            if (Tiles[i].Value != 0)
                            {
                                Tiles[i].Owner = oneDeltas[i] > twoDeltas[i] ? Player.One : Player.Two;
                            }
                        }
                        else
                        {
                            // Someone already owns this tile, so the deltas can be takes as-is
                            Tiles[i].Value += oneDeltas[i] + twoDeltas[i];

                            // If we subtracted this tile enough to get a negative number, then switch the owner to the opposite player
                            if (Tiles[i].Value < 0)
                            {
                                if (VolcanoGame.Settings.EruptOverflowAllowCapture)
                                {
                                    // Capture the tile
                                    Tiles[i].Value = -Tiles[i].Value;
                                    Tiles[i].Owner = Tiles[i].Owner == Player.One ? Player.Two : Player.One;
                                }
                                else
                                {
                                    // Clear the tile
                                    Tiles[i].Value = 0;
                                    Tiles[i].Owner = Player.Empty;
                                }
                            }
                        }

                        // If after the changes the tile is clear, remove any owner
                        if (Tiles[i].Value == 0)
                        {
                            Tiles[i].Owner = Player.Empty;
                        }

                        // Did this change trigger a chain reaction?
                        if (Tiles[i].Value >= VolcanoGame.Settings.MaxVolcanoLevel)
                        {
                            done = false;
                        }
                    }
                }
            }
        }

        private void SearchForWin()
        {
            for (int i = 0; i < 80; i++)
            {
                if (Tiles[i].Owner != Player.Empty && Tiles[Tiles[i].Antipode].Owner == Tiles[i].Owner && Tiles[i].Value > VolcanoGame.Settings.MaxMagmaChamberLevel)
                {
                    List<int> path = pathFinder.FindPath(this, i, Tiles[i].Antipode).Path;

                    if (path.Count > 0)
                    {
                        Winner = Tiles[i].Owner;
                        WinningPath = path;
                    }
                }
            }
        }

        /// <summary>
        /// Get a list of all valid moves for the current player on the current board state.
        /// </summary>
        /// <returns></returns>
        public List<Move> GetMoves()
        {
            return GetMoves(true, true, true, VolcanoGame.Settings.MaxVolcanoLevel);
        }

        /// <summary>
        /// Get a list of all valid moves for the current player on the current board state.
        /// </summary>
        /// <returns></returns>
        public List<Move> GetMoves(bool growthMoves, bool expandMoves, bool captureMoves, int maxGrowthValue)
        {
            List<Move> moves = new List<Move>();
            Player opponent = Player == Player.One ? Player.Two : Player.One;
            
            if (GetMoveTypeForTurn(Turn) == MoveType.AllGrow)
            {
                moves.Add(new Move(-1, MoveType.AllGrow));
            }
            else
            {
                for (int i = 0; i < 80; i++)
                {
                    // Grow existing tiles
                    if (growthMoves && Tiles[i].Owner == Player && Tiles[i].Value < maxGrowthValue)
                    {
                        moves.Add(new Move(i, MoveType.SingleGrow));
                    }

                    // Claim new tiles
                    if (expandMoves && Tiles[i].Owner == Player.Empty)
                    {
                        moves.Add(new Move(i, MoveType.SingleGrow));
                    }

                    // Capture enemy tiles
                    if (captureMoves && VolcanoGame.Settings.AllowMagmaChamberCaptures && Tiles[i].Owner == opponent && Tiles[i].Value <= VolcanoGame.Settings.MaxMagmaChamberLevel)
                    {
                        moves.Add(new Move(i, MoveType.SingleGrow));
                    }
                    if (captureMoves && VolcanoGame.Settings.AllowVolcanoCaptures && Tiles[i].Owner == opponent && Tiles[i].Value > VolcanoGame.Settings.MaxMagmaChamberLevel)
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
            return current == Player.One ? Player.Two : Player.One;
        }

        /// <summary>
        /// Which player should move on a given turn number.
        /// </summary>
        /// <param name="turn"></param>
        /// <returns></returns>
        private Player GetPlayerForTurn(int turn)
        {
            switch ((turn - 1) % 6)
            {
                case 0:
                case 4:
                case 5:
                    return Player.One;
                case 1:
                case 2:
                case 3:
                    return Player.Two;
                default:
                    return Player.Empty;
            }
        }

        /// <summary>
        /// Get the type of move a specific turn requires.
        /// </summary>
        /// <param name="turn"></param>
        /// <returns></returns>
        private MoveType GetMoveTypeForTurn(int turn)
        {
            switch ((turn - 1) % 6)
            {
                case 2:
                case 5:
                    return MoveType.AllGrow;
                case 0:
                case 1:
                case 3:
                case 4:
                default:
                    return MoveType.SingleGrow;
            }
        }
    }
}

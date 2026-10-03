using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Volcano.Search;

namespace Volcano.Game
{
    internal class Board
    {
        public int[] Tiles;
        public bool[] Dormant;
        public Player Player;
        public int Turn;

        public Player Winner;
        public List<int> WinningPathPlayerOne;
        public List<int> WinningPathPlayerTwo;

        public bool LastMoveIncreasedTile;

        public string Transcript;

        private static PathFinder pathFinder = new PathFinder();

        public bool allowHash;
        public bool fastWinSearch;

        // whether the board tiles have been inverted for perspective
        public bool Flipped;

        public ConcurrentDictionary<long, Player> winHashes;

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
            Tiles = new int[80];
            Dormant = new bool[80];
            Winner = Player.Empty;
            WinningPathPlayerOne = new List<int>();
            WinningPathPlayerTwo = new List<int>();
            Flipped = false;
        }

        public Board(Board copy)
        {
            Tiles = new int[80];
            Array.Copy(copy.Tiles, Tiles, 80);
            Dormant = new bool[80];
            Array.Copy(copy.Dormant, Dormant, 80);
            Player = copy.Player;
            Turn = copy.Turn;
            Winner = copy.Winner;
            WinningPathPlayerOne = copy.WinningPathPlayerOne;
            WinningPathPlayerTwo = copy.WinningPathPlayerTwo;
            Flipped = copy.Flipped;

            winHashes = copy.winHashes;
            fastWinSearch = copy.fastWinSearch;
        }

        /// <summary>
        /// Make a given move on the board.
        /// </summary>
        /// <param name="move"></param>
        public bool MakeMove(int move)
        {
            return MakeMove(move, true, true);
        }

        /// <summary>
        /// Make a given move on the board.
        /// </summary>
        /// <param name="move"></param>
        public bool MakeMove(int move, bool checkForWin, bool autoGrow)
        {
            Queue<int> eruptions = new Queue<int>();

            if (move == Constants.AllGrowMove)
            {
                LastMoveIncreasedTile = false;

                for (int i = 0; i < 80; i++)
                {
                    if (Tiles[i] != 0)
                    {
                        if (!VolcanoGame.Settings.AllowDormantVolcanoes || !Dormant[i])
                        {
                            Tiles[i] = Tiles[i] > 0 ? Tiles[i] + 1 : Tiles[i] - 1;
                            if (Math.Abs(Tiles[i]) >= VolcanoGame.Settings.MaxVolcanoLevel)
                            {
                                eruptions.Enqueue(i);
                            }
                        }
                    }
                }
            }
            else
            {
                LastMoveIncreasedTile = Tiles[move] != 0;

                Tiles[move] = Player == Player.One ? Tiles[move] + 1 : Tiles[move] - 1;
                if (Math.Abs(Tiles[move]) >= VolcanoGame.Settings.MaxVolcanoLevel)
                {
                    eruptions.Enqueue(move);
                }
            }

            if (eruptions.Count > 0)
            {
                ProcessEruptions(eruptions);
            }

            if (Winner == Player.Empty && checkForWin)
            {
                SearchForWin();
            }

            if (Winner == Player.Empty)
            {
                Turn++;
                Player = GetPlayerForTurn(Turn);

                if (autoGrow && GetMoveTypeForTurn(Turn) == MoveType.AllGrow)
                {
                    MakeMove(Constants.AllGrowMove);
                    return true;
                }
            }

            return false;
        }

        private void ProcessEruptions(Queue<int> eruptions)
        {
            int phases = 100;
            Queue<int> deltaIndexes = new Queue<int>();
            while (eruptions.Count > 0 && phases-- > 0)
            {
                // Phase one: get a list of deltas from eruptions
                int[] deltas = new int[80];
                while (eruptions.Count > 0)
                {
                    int i = eruptions.Dequeue();

                    if (Dormant[i])
                    {
                        continue;
                    }

                    if (VolcanoGame.Settings.AllowDormantVolcanoes)
                    {
                        Dormant[i] = true;
                    }
                    else
                    {
                        // Downgrade to a level one volcano
                        Tiles[i] = Tiles[i] > 0 ? VolcanoGame.Settings.MaxMagmaChamberLevel + 1 : -(VolcanoGame.Settings.MaxMagmaChamberLevel + 1);
                    }

                    foreach (int adjacent in Constants.AdjacentIndexes[i])
                    {
                        deltaIndexes.Enqueue(adjacent);

                        // Blank tile
                        if (Tiles[adjacent] == 0)
                        {
                            if (Tiles[i] > 0)
                            {
                                deltas[adjacent] += VolcanoGame.Settings.EruptOverflowEmptyTileAmount;
                            }
                            else
                            {
                                deltas[adjacent] -= VolcanoGame.Settings.EruptOverflowEmptyTileAmount;
                            }
                        }

                        // Same owner
                        else if ((Tiles[adjacent] > 0 && Tiles[i] > 0) || (Tiles[adjacent] < 0 && Tiles[i] < 0))
                        {
                            if (Tiles[i] > 0)
                            {
                                deltas[adjacent] += VolcanoGame.Settings.EruptOverflowFriendlyTileAmount;
                            }
                            else
                            {
                                deltas[adjacent] -= VolcanoGame.Settings.EruptOverflowFriendlyTileAmount;
                            }
                        }

                        // Enemy owner
                        else if ((Tiles[adjacent] > 0 && Tiles[i] < 0) || (Tiles[adjacent] < 0 && Tiles[i] > 0))
                        {
                            if (Tiles[i] > 0)
                            {
                                deltas[adjacent] -= VolcanoGame.Settings.EruptOverflowEnemyTileAmount;
                            }
                            else
                            {
                                deltas[adjacent] += VolcanoGame.Settings.EruptOverflowEnemyTileAmount;
                            }
                        }
                    }
                }

                // Phase two: process deltas
                while (deltaIndexes.Count > 0)
                {
                    int i = deltaIndexes.Dequeue();
                    if (deltas[i] != 0)
                    {
                        bool playerOne = Tiles[i] > 0;
                        bool playerTwo = Tiles[i] < 0;

                        // Someone already owns this tile, so the deltas can be taken as-is
                        Tiles[i] += deltas[i];

                        // If we changed the value so much that it switched sides
                        if (((Tiles[i] < 0 && playerOne) || (Tiles[i] > 0 && playerTwo)) && !VolcanoGame.Settings.EruptOverflowAllowCapture)
                        {
                            // Clear the tile
                            Tiles[i] = 0;
                            Dormant[i] = false;
                        }

                        // Did this change trigger a chain reaction?
                        if (Math.Abs(Tiles[i]) >= VolcanoGame.Settings.MaxVolcanoLevel)
                        {
                            eruptions.Enqueue(i);
                        }

                        // So we don't process it a second time
                        deltas[i] = 0;

                        if (VolcanoGame.Settings.AllowDormantVolcanoes)
                        {
                            if (Math.Abs(Tiles[i]) >= VolcanoGame.Settings.MaxVolcanoLevel)
                            {
                                Tiles[i] = Tiles[i] > 0 ? VolcanoGame.Settings.MaxVolcanoLevel : -VolcanoGame.Settings.MaxVolcanoLevel;
                            }
                            else
                            {
                                Dormant[i] = false;
                            }
                        }
                    }
                }
            }

            // If we're caught in an infinite loop of volcano eruptions, call the game a draw
            if (phases <= 0)
            {
                Winner = Player.Draw;
                WinningPathPlayerOne = new List<int>();
                WinningPathPlayerTwo = new List<int>();
            }
        }

        private void SearchForWin()
        {
            Winner = Player.Empty;

            var hash = 0L;
            if (allowHash)
            {
                hash = GetHash();
                if (winHashes.ContainsKey(hash))
                {
                    Winner = winHashes[hash];
                    return;
                }
            }

            // Training only
            if (StaticWinCache.Enabled)
            {
                hash = GetHash();
                if (StaticWinCache.Winners.TryGetValue(hash, out var winner))
                {
                    Winner = winner;
                    return;
                }
            }

            //if (Tiles[42] > 0)
            //{
            //    Winner = Player.One;
            //    return;
            //}
            //else if (Tiles[42] < 0)
            //{
            //    Winner = Player.Two;
            //    return;
            //}

            // We only need to cover the first 40 tiles since their antipodes cover the last 40
            for (int i = 0; i < 40; i++)
            {
                if (Tiles[i] != 0 &&
                    ((Tiles[Constants.Antipodes[i]] > 0 && Tiles[i] > 0) || (Tiles[Constants.Antipodes[i]] < 0 && Tiles[i] < 0)) &&
                    Math.Abs(Tiles[i]) > VolcanoGame.Settings.MaxMagmaChamberLevel &&
                    Math.Abs(Tiles[Constants.Antipodes[i]]) > VolcanoGame.Settings.MaxMagmaChamberLevel)
                {
                    // Only search until we find a winner or detect a draw
                    if (Winner == Player.Empty || (Winner == Player.One && Tiles[i] < 0) || (Winner == Player.Two && Tiles[i] > 0))
                    {
                        if (fastWinSearch || StaticWinSearch.Enabled)
                        {
                            var winner = FastWinSearch(i, Constants.Antipodes[i]);
                            if (winner != Player.Empty)
                            {
                                if (Tiles[i] > 0)
                                {
                                    Winner = Winner != Player.Empty ? Player.Draw : Player.One;
                                }
                                else
                                {
                                    Winner = Winner != Player.Empty ? Player.Draw : Player.Two;
                                }

                                if (Winner == Player.Draw)
                                {
                                    return;
                                }
                            }
                        }
                        else
                        {
                            var path = pathFinder.FindPath(this, i, Constants.Antipodes[i]).Path;
                            if (path.Count > 0)
                            {
                                if (Tiles[i] > 0)
                                {
                                    Winner = Winner != Player.Empty ? Player.Draw : Player.One;
                                    WinningPathPlayerOne = path;
                                }
                                else
                                {
                                    Winner = Winner != Player.Empty ? Player.Draw : Player.Two;
                                    WinningPathPlayerTwo = path;
                                }

                                if (Winner == Player.Draw)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            if (allowHash)
            {
                winHashes[hash] = Winner;
            }

            if (StaticWinCache.Enabled)
            {
                StaticWinCache.Winners.TryAdd(hash, Winner);
            }
        }

        private Player FastWinSearch(int start, int end)
        {
            var visited = new bool[80];
            var queue = new int[80];
            var qi = 0;
            var qh = 0;

            queue[qh++] = start;
            visited[start] = true;

            while (qi < qh)
            {
                var next = queue[qi++];

                if (next == end)
                {
                    return Tiles[next] > 0 ? Player.One : Player.Two;
                }

                // Check if tile belongs to the same player as the start tile
                if ((Tiles[next] > 0 && Tiles[start] > 0) || (Tiles[next] < 0 && Tiles[start] < 0))
                {
                    var adjacent = Constants.AdjacentIndexes[next];
                    for (int i = 0; i < adjacent.Length; i++)
                    {
                        int neighbor = adjacent[i];
                        if (!visited[neighbor])
                        {
                            visited[neighbor] = true;
                            queue[qh++] = neighbor;
                        }
                    }
                }
            }

            return Player.Empty;
        }

        /// <summary>
        /// Get a list of all valid moves for the current player on the current board state.
        /// </summary>
        /// <returns></returns>
        public List<int> GetMoves()
        {
            return GetMoves(true, true, true, VolcanoGame.Settings.MaxVolcanoLevel);
        }

        public List<int> GetRandomMoves()
        {
            var moves = GetMoves(false, true, false, VolcanoGame.Settings.MaxVolcanoLevel);
            if (moves.Count > 0)
            {
                return moves;
            }
            return GetMoves();
        }

        /// <summary>
        /// Get a list of all valid moves for the current player on the current board state.
        /// </summary>
        /// <returns></returns>
        public List<int> GetMoves(bool growthMoves, bool expandMoves, bool captureMoves, int maxGrowthValue)
        {
            List<int> moves = new List<int>();

            if (Winner != Player.Empty)
            {
                return moves;
            }
            else if (GetMoveTypeForTurn(Turn) == MoveType.AllGrow)
            {
                moves.Add(Constants.AllGrowMove);
            }
            else
            {
                for (int i = 0; i < 80; i++)
                {
                    // Grow existing tiles
                    if (growthMoves && ((Tiles[i] > 0 && Player == Player.One) || (Tiles[i] < 0 && Player == Player.Two)) && Math.Abs(Tiles[i]) < maxGrowthValue)
                    {
                        if (!VolcanoGame.Settings.AllowDormantVolcanoes || !Dormant[i])
                        {
                            moves.Add(i);
                        }
                    }

                    // Claim new tiles
                    if (expandMoves && Tiles[i] == 0)
                    {
                        moves.Add(i);
                    }

                    // Capture enemy tiles
                    if (captureMoves)
                    {
                        Player opponent = Player == Player.One ? Player.Two : Player.One;
                        if (VolcanoGame.Settings.AllowMagmaChamberCaptures &&
                            ((Tiles[i] > 0 && opponent == Player.One) || (Tiles[i] < 0 && opponent == Player.Two)) &&
                            Math.Abs(Tiles[i]) <= VolcanoGame.Settings.MaxMagmaChamberLevel)
                        {
                            moves.Add(i);
                        }
                        if (VolcanoGame.Settings.AllowVolcanoCaptures &&
                            ((Tiles[i] > 0 && opponent == Player.One) || (Tiles[i] < 0 && opponent == Player.Two)) &&
                            Math.Abs(Tiles[i]) > VolcanoGame.Settings.MaxMagmaChamberLevel)
                        {
                            moves.Add(i);
                        }
                    }
                }
            }

            if (moves.Count == 0)
            {
                if (growthMoves && expandMoves & captureMoves && maxGrowthValue >= VolcanoGame.Settings.MaxVolcanoLevel)
                {
                    // There are no moves in this position
                    return moves;
                }
                else
                {
                    return GetMoves(true, true, true, VolcanoGame.Settings.MaxVolcanoLevel);
                }
            }

            return moves;
        }

        public bool IsValidMove(int move)
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
        /// Get the current player adjusted for flipped states
        /// </summary>
        /// <returns></returns>
        public Player GetAbsolutePlayer()
        {
            if (Flipped)
            {
                if (Player == Player.One)
                {
                    return Player.Two;
                }
                else if (Player == Player.Two)
                {
                    return Player.One;
                }
            }

            return Player;
        }

        /// <summary>
        /// Get the winning player adjusted for flipped states
        /// </summary>
        /// <returns></returns>
        public Player GetAbsoluteWinner()
        {
            if (Flipped)
            {
                if (Winner == Player.One)
                {
                    return Player.Two;
                }
                else if (Winner == Player.Two)
                {
                    return Player.One;
                }
            }

            return Winner;
        }

        public int GetAbsoluteTurn()
        {
            return Turn - (Flipped ? 3 : 0);
        }

        /// <summary>
        /// Which player should move on a given turn number.
        /// </summary>
        /// <param name="turn"></param>
        /// <returns></returns>
        public Player GetPlayerForTurn(int turn)
        {
            switch ((turn - 1 + 6) % 6)
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

        public Player GetPlayerForPreviousTurn()
        {
            return GetPlayerForTurn(Turn - 1);
        }

        public Player GetPlayerForNextTurn()
        {
            return GetPlayerForTurn(Turn + 1);
        }

        /// <summary>
        /// Get the type of move a specific turn requires.
        /// </summary>
        /// <param name="turn"></param>
        /// <returns></returns>
        public MoveType GetMoveTypeForTurn(int turn)
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

        /// <summary>
        /// Whether it's the first or second move for a given player (since you get two in a row)
        /// </summary>
        /// <param name="turn"></param>
        /// <returns></returns>
        public int GetMoveNumber()
        {
            if (GetMoveTypeForTurn(Turn + 1) == MoveType.AllGrow)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        public long GetHash()
        {
            long hash = 0;

            for (int i = 0; i < 80; i++)
            {
                // this only hashes for win detection, not unique board states
                hash ^= Constants.ZobristKeys[i, Tiles[i] == 0 ? 0 : (Tiles[i] > 0 ? 1 : 2)];
            }

            return hash;
        }
    }
}
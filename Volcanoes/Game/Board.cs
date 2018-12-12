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

        public Player Winner { get; private set; }
        public List<int> WinningPath { get; private set; }
        
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
            Player = Player.Blue;
            Tiles = new List<Tile>();
            Winner = Player.Empty;
            WinningPath = new List<int>();

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
                SearchForWin();

                if (Winner == Player.Empty)
                {
                    Turn++;
                    Player = GetPlayerForTurn(Turn);

                    if (GetMoveTypeForTurn(Turn) == MoveType.AllGrow)
                    {
                        MakeMove(new Move(-1, MoveType.AllGrow));
                    }
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

        private void SearchForWin()
        {
            for (int i = 0; i < 80; i++)
            {
                if (Tiles[i].Owner != Player.Empty && Tiles[Tiles[i].Antipode].Owner == Tiles[i].Owner)
                {
                    List<int> path = FindPath(i, Tiles[i].Antipode);

                    if (path.Count > 0)
                    {
                        Winner = Tiles[i].Owner;
                        WinningPath = path;
                    }
                }
            }
        }

        private List<int> FindPath(int startingIndex, int endingIndex)
        {
            // The set of nodes already evaluated
            List<int> closedSet = new List<int>();

            // The set of currently discovered nodes that are not evaluated yet.
            // Initially, only the start node is known.
            List<int> openSet = new List<int>();
            openSet.Add(startingIndex);

            // For each node, which node it can most efficiently be reached from.
            // If a node can be reached from many nodes, cameFrom will eventually contain the
            // most efficient previous step.
            int[] cameFrom = new int[80];
            for (int i = 0; i < 80; i++)
            {
                cameFrom[i] = -1;
            }

            // For each node, the cost of getting from the start node to that node.
            int[] gScore = new int[80];
            for (int i = 0; i < 80; i++)
            {
                gScore[i] = int.MaxValue;
            }

            // The cost of going from start to start is zero.
            gScore[startingIndex] = 0;

            // For each node, the total cost of getting from the start node to the goal
            // by passing by that node. That value is partly known, partly heuristic.
            int[] fScore = new int[80];
            for (int i = 0; i < 80; i++)
            {
                fScore[i] = int.MaxValue;
            }

            // For the first node, that value is completely heuristic.
            fScore[startingIndex] = Math.Abs(endingIndex - startingIndex);

            while (openSet.Count > 0)
            {
                // Get the next item in the open set with the lowest fScore
                int current = openSet[0];
                int best = fScore[current];
                foreach (int i in openSet)
                {
                    if (fScore[i] < best)
                    {
                        current = i;
                        best = fScore[i];
                    }
                }

                // If we found a path from the start to the end, reconstruct the path and return it
                if (current == endingIndex)
                {
                    List<int> path = new List<int>();
                    path.Add(current);

                    while (cameFrom[current] != -1)
                    {
                        current = cameFrom[current];
                        path.Add(current);
                    }

                    return path;
                }

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (int neighbor in Tiles[current].AdjacentIndexes)
                {
                    // Ignore the neighbor which is already evaluated.
                    if (closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    // Ignore tiles that aren't the player's
                    if (Tiles[neighbor].Owner != Tiles[startingIndex].Owner)
                    {
                        continue;
                    }

                    // The distance from start to a neighbor
                    int tentative_gScore = gScore[current] + 1;

                    // Discover a new node
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                    else if (tentative_gScore >= gScore[neighbor])
                    {
                        continue;
                    }

                    // This path is the best until now. Record it!
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentative_gScore;
                    fScore[neighbor] = gScore[neighbor] + Math.Abs(neighbor - startingIndex);
                }
            }

            return new List<int>();
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

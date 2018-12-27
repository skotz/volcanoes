using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;
using Volcano.Search;

namespace Volcano.Engine
{
    class MonteCarloBeelineEngine : IEngine
    {
        private long evaluations;

        private int _initialDepth;
        private int _maxTilesToPick;
        private bool _randomBlitz;

        private Random random = new Random();
        private PathFinder pathFinder = new WeightedNonEnemyPathFinder();
        private EngineCancellationToken cancellationToken;

        public MonteCarloBeelineEngine()
            : this(2, 100, true)
        {
            // (4, 5, true)
        }

        public MonteCarloBeelineEngine(int initialDepth, int maxTilesPerDepth, bool randomBlitz)
        {
            _initialDepth = initialDepth;
            _maxTilesToPick = maxTilesPerDepth;
            _randomBlitz = randomBlitz;
        }

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            Stopwatch timer = Stopwatch.StartNew();
            evaluations = 0;

            cancellationToken = token;

            var result = MonteCarlo(state, _initialDepth);

            result.Result.Evaluations = evaluations;
            result.Result.Milliseconds = timer.ElapsedMilliseconds;

            return result.Result;
        }

        private MonteCarloResult Blitz(Board position)
        {
            evaluations++;

            // Play the game out and determine the winner
            while (position.Winner == Player.Empty && position.Turn < VolcanoGame.Settings.TournamentAdjudicateMaxTurns)
            {
                List<Move> moves = position.GetMoves();

                if (_randomBlitz)
                {
                    // Randomly play out the rest of the game
                    position.MakeMove(moves[random.Next(moves.Count)]);
                }
                else
                {
                    // Radomly pick moves on our best antipde path
                    PathResult best = new PathResult { Distance = int.MaxValue };
                    for (int i = 0; i < 80; i++)
                    {
                        if (position.Tiles[i].Owner == position.Player)
                        {
                            var test = pathFinder.FindPath(position, i, position.Tiles[i].Antipode);

                            if (test.Distance < best.Distance)
                            {
                                best = test;
                            }
                        }
                    }

                    // Make the first available move on our path
                    bool played = false;
                    if (best.Path != null)
                    {
                        foreach (int i in best.Path)
                        {
                            if (position.Tiles[i].Owner == Player.Empty)
                            {
                                position.MakeMove(moves.Where(x => x.TileIndex == i).FirstOrDefault());
                                played = true;
                                break;
                            }
                        }
                    }

                    // When all else fails, pick a random move
                    if (!played)
                    {
                        position.MakeMove(moves[random.Next(moves.Count)]);
                    }
                }
            }

            return new MonteCarloResult
            {
                PlayerOneWins = position.Winner == Player.One ? 1 : 0,
                PlayerTwoWins = position.Winner == Player.Two ? 1 : 0
            };
        }

        private List<Move> GetFilteredMoves(Board position)
        {
            // Get all non-volcano moves
            List<Move> allMoves = position.GetMoves(true, true, true, VolcanoGame.Settings.MaxMagmaChamberLevel + 1);
            if (allMoves.Count == 0)
            {
                // Get all available moves without condition
                allMoves = position.GetMoves();
            }
            if (allMoves.Count == 0)
            {
                // There are no valid moves
                return allMoves;
            }
            if (allMoves[0].MoveType == MoveType.AllGrow)
            {
                // It's a growth phase, so don't wast time
                return allMoves;
            }

            // For each tile we own, figure out how long it'll take to get to it's antipode
            PathResult[] paths = new PathResult[80];
            PathResult[] enemyPaths = new PathResult[80];
            for (int i = 0; i < 80; i++)
            {
                if (position.Tiles[i].Owner == position.Player)
                {
                    paths[i] = pathFinder.FindPath(position, i, position.Tiles[i].Antipode);
                }
                else if (position.Tiles[i].Owner != Player.Empty)
                {
                    enemyPaths[i] = pathFinder.FindPath(position, i, position.Tiles[i].Antipode);
                }
            }

            // Of all the calculated paths, find the one that's fastest
            PathResult best = paths.Where(x => x != null && x.Distance != 0).OrderBy(x => x.Distance).FirstOrDefault();
            PathResult bestEnemy = enemyPaths.Where(x => x != null && x.Distance != 0).OrderBy(x => x.Distance).FirstOrDefault();

            List<Move> moves = new List<Move>();
            if (best != null)
            {
                // Create a list of moves that are on the ideal path
                foreach (int i in best.Path)
                {
                    if (position.Tiles[i].Owner == Player.Empty)
                    {
                        Move move = allMoves.Where(x => x.TileIndex == i).FirstOrDefault();
                        if (move != null)
                        {
                            moves.Add(move);
                        }
                    }
                }

                if (bestEnemy != null)
                {
                    // Create a list of moves that are on the enemy's best path
                    foreach (int i in bestEnemy.Path)
                    {
                        if (position.Tiles[i].Owner == Player.Empty)
                        {
                            Move move = allMoves.Where(x => x.TileIndex == i).FirstOrDefault();
                            if (move != null)
                            {
                                moves.Insert(0, move);
                            }
                        }
                    }
                }

                // If there are no empty tiles left, pick magma chambers
                if (moves.Count == 0)
                {
                    foreach (int i in best.Path)
                    {
                        if (position.Tiles[i].Owner == position.Player && position.Tiles[i].Value <= VolcanoGame.Settings.MaxMagmaChamberLevel)
                        {
                            Move move = allMoves.Where(x => x.TileIndex == i).FirstOrDefault();
                            if (move != null)
                            {
                                moves.Add(move);
                            }
                        }
                    }
                }
            }

            // If we haven't found a move yet, pick a random one
            if (moves.Count == 0)
            {
                if (allMoves.Count == 0)
                {
                    // Get all available moves without condition
                    allMoves = position.GetMoves();
                }

                moves.Add(allMoves[random.Next(allMoves.Count)]);
            }

            // Enforce a maxmimum number of candidate moves to speed up the search
            while (moves.Count > _maxTilesToPick)
            {
                moves.RemoveAt(random.Next(moves.Count));
            }

            return moves;
        }

        private MonteCarloResult MonteCarlo(Board position, int depth)
        {
            // We've been told to cancel our search
            if (cancellationToken.Cancelled)
            {
                return new MonteCarloResult();
            }

            // We've reached the depth of our search, so blitz out the rest of the game and see who wins
            if (depth <= 0)
            {
                return Blitz(position);
            }

            List<Move> moves = GetFilteredMoves(position);

            // If we have no moves, return the evaluation of the position
            if (moves.Count == 0)
            {
                return Blitz(position);
            }

            MonteCarloResult best = new MonteCarloResult
            {
                Result = new SearchResult
                {
                    Score = int.MinValue
                }
            };

            foreach (Move move in moves)
            {
                // Copy the board and make a move
                Board copy = new Board(position);
                copy.MakeMove(move, false, true);

                // Recursively make the opponent's moves
                MonteCarloResult counterMoveResult = MonteCarlo(copy, depth - 1);

                best.PlayerOneWins += counterMoveResult.PlayerOneWins;
                best.PlayerTwoWins += counterMoveResult.PlayerTwoWins;

                if (position.Player == Player.One && counterMoveResult.PlayerOneScore > best.Result.Score)
                {
                    best.Result.Score = counterMoveResult.PlayerOneScore;
                    best.Result.BestMove = move;
                }

                if (position.Player == Player.Two && counterMoveResult.PlayerTwoScore > best.Result.Score)
                {
                    best.Result.Score = counterMoveResult.PlayerTwoScore;
                    best.Result.BestMove = move;
                }
            }

            return best;
        }

        class MonteCarloResult
        {
            public int PlayerOneWins { get; set; }
            public int PlayerTwoWins { get; set; }

            public SearchResult Result { get; set; }

            private decimal factor = 1000m;

            public int PlayerOneScore
            {
                get
                {
                    if (PlayerOneWins + PlayerTwoWins > 0)
                    {
                        return (int)(PlayerOneWins * factor / (PlayerOneWins + PlayerTwoWins));
                    }

                    return 0;
                }
            }

            public int PlayerTwoScore
            {
                get
                {
                    if (PlayerOneWins + PlayerTwoWins > 0)
                    {
                        return (int)(PlayerTwoWins * factor / (PlayerOneWins + PlayerTwoWins));
                    }

                    return 0;
                }
            }
        }
    }
}

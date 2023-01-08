using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Volcano.Game;

namespace Volcano.Engine
{
    internal class BreadthFirstBeamSearch : IEngine, IStatus
    {
        private int _bufferMilliseconds = 5;

        private int _visitedNodes;

        private Stopwatch _statusUpdate;
        private int _millisecondsBetweenUpdates = 1000;

        private EngineCancellationToken cancel;

        private Random rand = new Random();

        public BreadthFirstBeamSearch()
        {
        }

        public event EventHandler<EngineStatus> OnStatus;

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            var timer = Stopwatch.StartNew();
            _statusUpdate = Stopwatch.StartNew();

            cancel = new EngineCancellationToken(() => token.Cancelled || timer.ElapsedMilliseconds >= maxSeconds * 1000L - _bufferMilliseconds);

            _visitedNodes = 0;

            var best = -1;

            // iterative widening
            for (var width = 10; width < 10000 && !token.Cancelled; width++)
            {
                var test = GetBestMove(state, width);

                if (!token.Cancelled && test >= 0)
                {
                    best = test;
                }
            }

            return new SearchResult
            {
                BestMove = best,
                Evaluations = _visitedNodes,
                Milliseconds = timer.ElapsedMilliseconds
            };
        }

        private int GetBestMove(Board start, int beamWidth)
        {
            var root = new BeamNode(-1, null, start, 0, 0);

            var set = new List<BeamNode>();
            var beam = new List<BeamNode>();
            beam.Add(root);

            var startingPlayer = start.Player;
            var terminateSearch = false;

            var best = root;

            while (beam.Count != 0 && !terminateSearch)
            {
                set.Clear();

                var playerToMove = Player.Empty;
                if (beam.Count > 0)
                {
                    playerToMove = beam[0].State.Player;
                }

                for (int i = 0; i < beam.Count; i++)
                {
                    _visitedNodes++;

                    var node = beam[i];

                    if (cancel.Cancelled)
                    {
                        terminateSearch = true;
                        break;
                    }

                    var moves = node.State.GetMoves();

                    foreach (var move in moves)
                    {
                        var copy = new Board(node.State);

                        copy.MakeMove(move);

                        var eval = Evaluate(copy, node.Depth + 1);
                        var winner = copy.Winner;

                        if (winner == startingPlayer && winner == playerToMove)
                        {
                            terminateSearch = true;

                            var final = node;
                            while (final.Parent != null && final.Parent.Move != -1)
                            {
                                final = final.Parent;
                            }
                            return final.Move;
                        }

                        var child = new BeamNode(move, node, copy, node.Depth + 1, eval);

                        node.Children.Add(child);
                        set.Add(child);
                    }
                }

                beam.Clear();

                if (playerToMove == Player.One)
                {
                    beam.AddRange(set.OrderByDescending(x => x.Evaluation).Take(beamWidth));
                }
                else
                {
                    beam.AddRange(set.OrderBy(x => x.Evaluation).Take(beamWidth));
                }

                if (beam.Count > 0)
                {
                    best = beam[0];
                }

                // Update Status
                if (OnStatus != null && _statusUpdate.ElapsedMilliseconds > _millisecondsBetweenUpdates && beam.Count > 0)
                {
                    var status = new EngineStatus();

                    var maxLines = 1;
                    for (int i = 0; i < Math.Min(maxLines, beam.Count); i++)
                    {
                        var parent = beam[i];
                        var line = Constants.TileNames[parent.Move];
                        while (parent.Parent != null && parent.Parent.Parent != null)
                        {
                            parent = parent.Parent;
                            line = Constants.TileNames[parent.Move] + " " + line;
                        }

                        status.Details.Add(new EngineStatusLine
                        {
                            Evaluation = beam[i].Evaluation,
                            MoveIndex = parent.Move,
                            ExtraInformation = "(" + beamWidth + ") " + line
                        });
                    }

                    status.Sort();

                    OnStatus(this, status);
                    _statusUpdate = Stopwatch.StartNew();
                }
            }

            return best.Move;

            // Perform a full minimax search, but only on the tree built out by a depth-first search
            //return MiniMax(root).Move;
        }

        private int Evaluate(Board board, int depth)
        {
            int eval = 0;

            if (board.Winner == Player.One)
            {
                return 10000 - depth;
            }
            if (board.Winner == Player.Two)
            {
                return -(10000 - depth);
            }

            // Get points for each connected tile
            for (int i = 0; i < 80; i++)
            {
                if (Math.Abs(board.Tiles[i]) > 0)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        if (board.Tiles[i] > 0 && board.Tiles[Constants.AdjacentIndexes[i][c]] > 0)
                        {
                            eval++;
                        }
                        if (board.Tiles[i] < 0 && board.Tiles[Constants.AdjacentIndexes[i][c]] < 0)
                        {
                            eval--;
                        }
                    }
                }
            }

            return eval;
        }

        private MiniMaxNode MiniMax(BeamNode node)
        {
            if (node.Children.Count == 0)
            {
                return new MiniMaxNode(node.Evaluation);
            }

            var best = new MiniMaxNode(node.State.Player == Player.One ? int.MinValue : int.MaxValue);

            foreach (var child in node.Children)
            {
                var test = MiniMax(child);

                if (node.State.Player == Player.One)
                {
                    if (test.Evaluation > best.Evaluation)
                    {
                        best.Move = child.Move;
                        best.Evaluation = test.Evaluation;
                    }
                }
                else
                {
                    if (test.Evaluation < best.Evaluation)
                    {
                        best.Move = child.Move;
                        best.Evaluation = test.Evaluation;
                    }
                }
            }

            return best;
        }

        private class MiniMaxNode
        {
            public int Move;

            public int Evaluation;

            public MiniMaxNode(int eval)
            {
                Evaluation = eval;
            }

            public MiniMaxNode(int move, int eval)
            {
                Move = move;
                Evaluation = eval;
            }
        }

        private class BeamNode
        {
            public int Move;

            public BeamNode Parent;

            public List<BeamNode> Children;

            public Board State;

            public int Depth;

            public int Evaluation;

            public BeamNode(int move, BeamNode parent, Board state, int depth, int evaluation)
            {
                Move = move;
                Parent = parent;
                Children = new List<BeamNode>();
                State = state;
                Depth = depth;
                Evaluation = evaluation;
            }
        }
    }
}
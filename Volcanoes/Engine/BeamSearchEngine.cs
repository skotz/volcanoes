using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Volcano.Game;
using Volcano.Search;

namespace Volcano.Engine
{
    internal class BeamSearchEngine : IEngine, IStatus
    {
        public event EventHandler<EngineStatus> OnStatus;

        private int _width;
        private EngineCancellationToken _cancel;
        private TotalPathFinder _pathFinder;

        public BeamSearchEngine()
            : this(80 * 80)
        {
        }

        public BeamSearchEngine(int width)
        {
            _width = width;
            _pathFinder = new TotalPathFinder();
        }

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            var timer = Stopwatch.StartNew();
            var update = Stopwatch.StartNew();
            _cancel = new EngineCancellationToken(() => token.Cancelled || timer.ElapsedMilliseconds >= maxSeconds * 1000L - 250);

            var evals = 0;

            var beam = new List<BeamNode>();
            beam.Add(new BeamNode
            {
                state = state,
                history = new List<int>(),
            });

            while (!_cancel.Cancelled)
            {
                var next = new List<BeamNode>();
                var found = false;

                foreach (var node in beam)
                {
                    evals++;

                    if (_cancel.Cancelled)
                    {
                        break;
                    }

                    if (node.state.Winner == Player.Empty)
                    {
                        foreach (var move in node.state.GetMoves())
                        {
                            var copy = new Board(node.state);
                            copy.MakeMove(move);

                            next.Add(new BeamNode
                            {
                                state = copy,
                                eval = Evaluate(copy),
                                history = [.. node.history, move],
                            });

                            found = true;
                        }
                    }
                    else
                    {
                        next.Add(node);
                    }
                }

                if (!found)
                {
                    // all games are over
                    break;
                }

                beam = next.OrderByDescending(x => x.eval).Take(_width).ToList();

                if (beam.GroupBy(x => x.history[0]).Count() == 1)
                {
                    // only one variation
                    break;
                }

                if ((update.ElapsedMilliseconds > 500 || _cancel.Cancelled) && OnStatus != null)
                {
                    var status = new EngineStatus();
                    var done = new bool[80];
                    foreach (var node in beam)
                    {
                        if (!done[node.history[0]])
                        {
                            done[node.history[0]] = true;
                            var pv = string.Join(" ", node.history.Select(x => Constants.TileNames[x]));
                            status.Add(node.history[0], Math.Round(node.eval, 2), pv, node.eval);
                        }
                    }
                    OnStatus?.Invoke(this, status);
                    update = Stopwatch.StartNew();
                }
            }

            var best = beam[0].history[0];

            return new SearchResult
            {
                BestMove = best,
                Evaluations = evals,
                Milliseconds = timer.ElapsedMilliseconds,
            };
        }

        private double Evaluate(Board state)
        {
            var score = 0.0;

            if (state.Winner == state.Player)
            {
                return 100;
            }
            else if (state.Winner == (state.Player == Player.One ? Player.Two : Player.One))
            {
                return -100;
            }

            var paths = _pathFinder.GetAllPaths(state);

            var myBest = paths.MyPaths.OrderBy(x => x.Distance).FirstOrDefault();
            var oppBest = paths.OpponentPaths.OrderBy(x => x.Distance).FirstOrDefault();

            if (myBest != null)
            {
                var filled = myBest.Path.Count(x => state.Tiles[x] != 0);
                var empty = myBest.Path.Count(x => state.Tiles[x] == 0);
                var progress = (double)filled / (filled + empty);

                score += progress;
            }

            if (oppBest != null)
            {
                var filled = oppBest.Path.Count(x => state.Tiles[x] != 0);
                var empty = oppBest.Path.Count(x => state.Tiles[x] == 0);
                var progress = (double)filled / (filled + empty);

                score -= progress;
            }

            return score;
        }
    }

    internal class BeamNode
    {
        public Board state;
        public double eval;
        public List<int> history;
    }
}
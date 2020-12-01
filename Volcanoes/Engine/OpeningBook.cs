using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Engine
{
    internal class OpeningBook
    {
        private string _file;

        private Dictionary<string, int> _book;

        private SemaphoreSlim _lock;

        private Random _rand;

        public bool Loaded { get; private set; }

        public int Depth { get; private set; }

        public int Seconds { get; private set; }

        public OpeningBook(string file)
        {
            _file = file;
            _book = new Dictionary<string, int>();
            _lock = new SemaphoreSlim(1);
            _rand = new Random();

            if (File.Exists(_file))
            {
                using (var r = new StreamReader(_file))
                {
                    Depth = int.Parse(r.ReadLine());

                    var count = int.Parse(r.ReadLine());

                    Seconds = int.Parse(r.ReadLine());

                    for (int i = 0; i < count; i++)
                    {
                        var transcript = r.ReadLine();
                        var move = Constants.TileIndexes[r.ReadLine()];

                        _book.Add(transcript, move);
                    }
                }

                Loaded = true;
            }
        }

        public int GetMove(string transcript)
        {
            if (_book.ContainsKey(transcript))
            {
                return _book[transcript];
            }
            else
            {
                return -1;
            }
        }

        //public void Generate(int depth, int seconds)
        //{
        //    var root = new BookNode();
        //    root.State = new VolcanoGame();
        //    root.Children = root.State.CurrentState.GetMoves().Select(move => new BookNode
        //    {
        //        Move = move,
        //        Parent = root,
        //        State = Expand(root.State, move)
        //    }).ToList();

        //    var nextQueue = new List<BookNode>();
        //    var queue = new List<BookNode>();
        //    queue.Add(root);

        //    for (int d = 0; d < depth; d++)
        //    {
        //        var lastToPlay = Player.Empty;

        //        while (queue.Count > 0)
        //        {
        //            var node = queue[0];
        //            queue.RemoveAt(0);

        //            lastToPlay = node.State.CurrentState.Player;

        //            // Score all possible moves in this position
        //            Parallel.ForEach(node.Children, child =>
        //            {
        //                for (int i = 0; i < _simulations; i++)
        //                {
        //                    var winner = Simlulate(child.State.CurrentState);

        //                    child.Simulations++;

        //                    if (winner == Player.One)
        //                    {
        //                        child.PlayerOneWins++;
        //                    }

        //                    if (winner == Player.Two)
        //                    {
        //                        child.PlayerTwoWins++;
        //                    }
        //                }
        //            });

        //            // Keep the best ones
        //            if (node.State.CurrentState.Player == Player.One)
        //            {
        //                node.Children = node.Children.OrderByDescending(x => x.PlayerOneScore).ToList();
        //            }
        //            if (node.State.CurrentState.Player == Player.Two)
        //            {
        //                node.Children = node.Children.OrderByDescending(x => x.PlayerTwoScore).ToList();
        //            }

        //            if (d > 0)
        //            {
        //                node.Children = node.Children.Take(_branchFactor).ToList();
        //            }

        //            // Save the results into the book
        //            var transcript = node.State.GetTranscriptLine();
        //            if (!_book.ContainsKey(transcript))
        //            {
        //                _book.Add(transcript, node.Children[0].Move);
        //            }

        //            foreach (var child in node.Children)
        //            {
        //                child.Children = child.State.CurrentState.GetMoves().Select(move => new BookNode
        //                {
        //                    Move = move,
        //                    Parent = child,
        //                    State = Expand(child.State, move)
        //                }).ToList();

        //                nextQueue.Add(child);
        //            }
        //        }

        //        using (var r = new StreamWriter(_file))
        //        {
        //            r.WriteLine(depth);
        //            r.WriteLine(_book.Count);
        //            r.WriteLine(seconds);

        //            foreach (var entry in _book)
        //            {
        //                r.WriteLine(entry.Key);
        //                r.WriteLine(Constants.TileNames[entry.Value]);
        //            }
        //        }

        //        if (lastToPlay == Player.One)
        //        {
        //            nextQueue = nextQueue.OrderByDescending(x => x.Parent.PlayerOneScore).Take(_beamWidth).ToList();
        //        }
        //        if (lastToPlay == Player.Two)
        //        {
        //            nextQueue = nextQueue.OrderByDescending(x => x.Parent.PlayerTwoScore).Take(_beamWidth).ToList();
        //        }

        //        queue.AddRange(nextQueue);
        //        nextQueue.Clear();
        //    }
        //}

        //public VolcanoGame Expand(VolcanoGame game, int move)
        //{
        //    var copy = new VolcanoGame();
        //    copy.LoadTranscript(game.GetTranscriptLine());
        //    copy.MakeMove(move);
        //    return copy;
        //}

        //public Player Simlulate(Board state)
        //{
        //    var copy = new Board(state);

        //    while (copy.Winner == Player.Empty && copy.Turn < 100)
        //    {
        //        var moves = copy.GetMoves();
        //        if (moves.Count == 0)
        //        {
        //            break;
        //        }
        //        copy.MakeMove(moves[_rand.Next(moves.Count)]);
        //    }

        //    return copy.Winner;
        //}

        public void Generate(int depth, int seconds)
        {
            // Blue's first move
            var blueStart = GenerateBookForPosition(depth, seconds, "", true);

            // Blue's second and third move (after all possible moves from orange)
            var allGames = GetAllTranscriptsAfterPosition(blueStart, false);

            Parallel.ForEach(allGames, transcript =>
            {
                GenerateBookForPosition(depth, seconds, transcript, false);
            });

            // Orange's first and second move (after all possible moves from blue)
            allGames = GetAllTranscriptsAfterPosition("", true);

            Parallel.ForEach(allGames, transcript =>
            {
                GenerateBookForPosition(depth, seconds, transcript, false);
            });
        }

        private string GenerateBookForPosition(int depth, int seconds, string transcript, bool singleOnly)
        {
            var bestTranscript = "";

            if (!_book.ContainsKey(transcript))
            {
                var game = new VolcanoGame();
                game.LoadTranscript(transcript);

                var engine = new MonteCarloTreeSearchEngine(false);
                var best = engine.GetBestMove(game.CurrentState, seconds, new EngineCancellationToken(() => false));

                var t = game.GetTranscriptLine();
                var b = best.BestMove;

                lock (_book)
                {
                    _book.Add(t, b);
                }

                game.MakeMove(b);
                bestTranscript = game.GetTranscriptLine();

                UpdateBook(depth, seconds);

                if (!singleOnly)
                {
                    best = engine.GetBestMove(game.CurrentState, seconds, new EngineCancellationToken(() => false));

                    t = game.GetTranscriptLine();
                    b = best.BestMove;

                    lock (_book)
                    {
                        _book.Add(t, b);
                    }

                    UpdateBook(depth, seconds);
                }
            }

            return bestTranscript;
        }

        private void UpdateBook(int depth, int seconds)
        {
            _lock.Wait();

            using (var r = new StreamWriter(_file))
            {
                r.WriteLine(depth);
                r.WriteLine(_book.Count);
                r.WriteLine(seconds);

                lock (_book)
                {
                    foreach (var entry in _book)
                    {
                        r.WriteLine(entry.Key);
                        r.WriteLine(Constants.TileNames[entry.Value]);
                    }
                }
            }

            _lock.Release();
        }

        private List<string> GetAllTranscriptsAfterPosition(string transcript, bool singleOnly)
        {
            var transcripts = new List<string>();

            var baseGame = new VolcanoGame();
            baseGame.LoadTranscript(transcript);

            foreach (var firstMove in baseGame.CurrentState.GetMoves())
            {
                var firstCopy = new VolcanoGame();
                firstCopy.LoadTranscript(baseGame.GetTranscriptLine());

                firstCopy.MakeMove(firstMove);

                if (singleOnly)
                {
                    transcripts.Add(firstCopy.GetTranscriptLine());
                }
                else
                {
                    foreach (var secondMove in firstCopy.CurrentState.GetMoves())
                    {
                        var secondCopy = new VolcanoGame();
                        secondCopy.LoadTranscript(firstCopy.GetTranscriptLine());

                        secondCopy.MakeMove(secondMove);

                        transcripts.Add(secondCopy.GetTranscriptLine());
                    }
                }
            }

            return transcripts;
        }

        private void GetAllGames(bool playerOne, List<string> games, VolcanoGame board, int depth, int maxDepth)
        {
            if (depth >= maxDepth)
            {
                return;
            }

            var game = new VolcanoGame();
            game.LoadTranscript(board.GetTranscriptLine());

            var allMoves = game.CurrentState.GetMoves();

            // Building a book for blue
            if (playerOne)
            {
                if (game.CurrentState.Player == Player.Two)
                {
                    // Get every possible move for the opponent
                    allMoves = game.CurrentState.GetMoves();
                }
                else
                {
                    if (depth == 1)
                    {
                    }
                }
            }

            // Building a book for orange
            if (!playerOne)
            {
                if (game.CurrentState.Player == Player.One)
                {
                    // Get every possible move for the opponent
                    allMoves = game.CurrentState.GetMoves();
                }
                else
                {
                }
            }

            foreach (var move in allMoves)
            {
                var copy = new VolcanoGame();
                copy.LoadTranscript(game.GetTranscriptLine());

                copy.MakeMove(move);

                games.Add(copy.GetTranscriptLine());

                GetAllGames(playerOne, games, copy, depth + 1, maxDepth);
            }
        }

        private class BookNode
        {
            public BookNode Parent { get; set; }

            public List<BookNode> Children { get; set; }

            public int Move { get; set; }

            public VolcanoGame State { get; set; }

            public int PlayerOneWins { get; set; }

            public int PlayerTwoWins { get; set; }

            public int Simulations { get; set; }

            public double PlayerOneScore
            {
                get
                {
                    if (Simulations <= 0)
                    {
                        return 0;
                    }

                    return (double)PlayerOneWins / Simulations;
                }
            }

            public double PlayerTwoScore
            {
                get
                {
                    if (Simulations <= 0)
                    {
                        return 0;
                    }

                    return (double)PlayerTwoWins / Simulations;
                }
            }

            public BookNode()
            {
                Children = new List<BookNode>();
            }
        }
    }
}
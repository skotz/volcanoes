using System;
using System.Collections.Concurrent;
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

        private ConcurrentDictionary<string, int> _book;

        private SemaphoreSlim _lock;

        private Random _rand;

        public bool Loaded { get; private set; }

        public int Depth { get; private set; }

        public int Seconds { get; private set; }

        public event BookGenerationHandler OnStatusUpdate;

        public delegate void BookGenerationHandler(int completed, int total);

        public OpeningBook(string file)
        {
            _file = file;
            _book = new ConcurrentDictionary<string, int>();
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

                        _book[transcript] = move;
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

        public void Generate(int depth, int seconds)
        {
            int done = 0;
            OnStatusUpdate?.Invoke(done, 12643);

            // Blue's first move
            var blueStart = GenerateBookForPosition(depth, seconds, "", true);

            OnStatusUpdate?.Invoke(++done, 12643);

            // Blue's second and third move (after all possible moves from orange)
            var allGamesBlue = GetAllTranscriptsAfterPosition(blueStart, false);

            // Orange's first and second move (after all possible moves from blue)
            var allGamesOrange = GetAllTranscriptsAfterPosition("", true);

            Parallel.ForEach(allGamesBlue, transcript =>
            {
                GenerateBookForPosition(depth, seconds, transcript, false);

                OnStatusUpdate?.Invoke(++done, 12643);
            });

            Parallel.ForEach(allGamesOrange, transcript =>
            {
                GenerateBookForPosition(depth, seconds, transcript, false);

                OnStatusUpdate?.Invoke(++done, 12643);
            });

            OnStatusUpdate?.Invoke(12643, 12643);
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
                    _book[t] = b;
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
                        _book[t] = b;
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
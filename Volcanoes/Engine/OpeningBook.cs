using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private const string _fileHeader = "volcanoes-opening-book";

        public OpeningBook(string file)
        {
            _file = file;
            _book = new ConcurrentDictionary<string, int>();
            _lock = new SemaphoreSlim(1);
            _rand = new Random();

            if (File.Exists(_file))
            {
                switch (DetectVersion(_file))
                {
                    case 1:
                        // backwards compatible
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
                        break;

                    case 2:
                        using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var reader = new BinaryReader(stream))
                        {
                            Seconds = -1;
                            Depth = 0;
                            _ = reader.ReadString(); // header
                            _ = reader.ReadString(); // engine
                            _ = reader.ReadInt32(); // iterations
                            _ = reader.ReadInt32(); // gap
                            var numLines = reader.ReadInt32();
                            for (var i = 0; i < numLines; i++)
                            {
                                var numMoves = (int)reader.ReadByte();
                                var transcript = "";
                                for (var m = 0; m < numMoves; m++)
                                {
                                    var transcriptMove = (int)reader.ReadByte();
                                    transcript += Constants.TileNames[transcriptMove] + " ";
                                }
                                transcript = transcript.Trim();

                                Depth = Math.Max(Depth, numMoves + 1);

                                var bestMove = (int)reader.ReadByte();
                                _book[transcript] = bestMove;
                            }
                        }
                        break;
                }

                Loaded = true;
            }
        }

        private int DetectVersion(string file)
        {
            try
            {
                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new BinaryReader(stream))
                {
                    return reader.ReadString() == _fileHeader ? 2 : 1;
                }
            }
            catch
            {
                return 1;
            }
        }

        public int GetMove(string transcript)
        {
            var game = new VolcanoGame();
            game.LoadTranscript(transcript);

            var canonical = new Canonical();
            canonical.SetIndex(game);

            var canonicalTranscript = "";
            if (game.MoveHistory.Count > 0)
            {
                canonicalTranscript = game.MoveHistory
                    .Select(canonical.BoardToCanonical)
                    .Select(x => Constants.TileNames[x])
                    .Aggregate((c, n) => c + " " + n);
            }

            if (_book.ContainsKey(canonicalTranscript))
            {
                if (canonicalTranscript == "" && _book[canonicalTranscript] == 0)
                {
                    // future moves will also be canonicalized, so for aethetic purposes just pick a more centralized identical tile
                    return Constants.TileIndexes["N26"];
                }

                return canonical.CanonicalToBoard(_book[canonicalTranscript]);
            }
            else
            {
                return -1;
            }
        }

        public void Generate(int depth, int iterations, int gap)
        {
            var done = 0;
            var total = 1;

            // Blue's first move (hardcode to one of the 20 identical equilateral triangles since plenty of computational power says that's slightly better than one of the 60 identical isosceles triangles)
            var blueStart = "N07";
            _book[""] = Constants.TileIndexes[blueStart];
            OnStatusUpdate?.Invoke(done, 1);
            UpdateBook(depth, iterations, gap);

            // Blue's second and third move (after all possible moves from orange)
            var allGamesBlue = GetAllTranscriptsAfterPosition(blueStart, false);
            total += allGamesBlue.Count;

            // Orange's first and second move (after all possible moves from blue)
            var allGamesOrange = GetAllTranscriptsAfterPosition("", true);
            total += allGamesOrange.Count;

            OnStatusUpdate?.Invoke(++done, total);

            Parallel.ForEach(allGamesBlue, transcript =>
            {
                GenerateBookForPosition(depth, iterations, transcript, false, gap);

                OnStatusUpdate?.Invoke(++done, total);
            });

            Parallel.ForEach(allGamesOrange, transcript =>
            {
                GenerateBookForPosition(depth, iterations, transcript, false, gap);

                OnStatusUpdate?.Invoke(++done, total);
            });

            OnStatusUpdate?.Invoke(done, total);
        }

        private string GenerateBookForPosition(int depth, int iterations, string transcript, bool singleOnly, int gap)
        {
            var bestTranscript = "";

            var game = new VolcanoGame();
            game.LoadTranscript(transcript);

            var canonical = new Canonical();
            canonical.SetIndex(game);

            var t = "";
            if (game.MoveHistory.Count > 0)
            {
                t = game.MoveHistory
                    .Select(canonical.BoardToCanonical)
                    .Select(x => Constants.TileNames[x])
                    .Aggregate((c, n) => c + " " + n);
            }

            if (!_book.ContainsKey(t))
            {
                // use iterations instead of time so we can run in parallel without starving a thread and getting bad results
                // on my machine a 120 second search resulted in 1,260,000 mcts playouts, so roughly 10,000 per second
                var engine = new MonteCarloTreeSearchEngine(MCTSVersion.V2);
                engine.simplifyFirstMove = true;
                engine.forcedIterations = iterations;
                engine.forcedGap = gap;

                var best = engine.GetBestMove(game.CurrentState, iterations, new EngineCancellationToken(() => false));

                var b = best.BestMove;

                lock (_book)
                {
                    _book[t] = b;
                }

                game.MakeMove(b);
                bestTranscript = game.GetTranscriptLine().Replace("+", "");

                UpdateBook(depth, iterations, gap);

                if (!singleOnly)
                {
                    best = engine.GetBestMove(game.CurrentState, iterations, new EngineCancellationToken(() => false));

                    t = game.GetTranscriptLine().Replace("+", "");
                    b = best.BestMove;

                    lock (_book)
                    {
                        _book[t] = b;
                    }

                    UpdateBook(depth, iterations, gap);
                }
            }

            return bestTranscript;
        }

        private void UpdateBook(int depth, int iterations, int gap)
        {
            _lock.Wait();

            //using (var r = new StreamWriter(_file))
            //{
            //    r.WriteLine(depth);
            //    r.WriteLine(_book.Count);
            //    r.WriteLine(iterations);
            //    lock (_book)
            //    {
            //        foreach (var entry in _book)
            //        {
            //            r.WriteLine(entry.Key);
            //            r.WriteLine(Constants.TileNames[entry.Value]);
            //        }
            //    }
            //}

            using (var writer = new BinaryWriter(File.Open(_file, FileMode.Create)))
            {
                writer.Write(_fileHeader); // header
                writer.Write("mcts-v2"); // engine
                writer.Write(iterations); // iterations
                writer.Write(gap); // gap

                writer.Write(_book.Count);
                foreach (var entry in _book)
                {
                    var transcriptMoves = entry.Key == "" ? [] : entry.Key.Split(' ').Select(x => Constants.TileIndexes[x.Replace("+", "")]).ToList();
                    writer.Write((byte)transcriptMoves.Count);
                    foreach (var move in transcriptMoves)
                    {
                        writer.Write((byte)move);
                    }
                    writer.Write((byte)entry.Value);
                }
            }

            _lock.Release();
        }

        private List<string> GetAllTranscriptsAfterPosition(string transcript, bool singleOnly)
        {
            var transcripts = new List<string>();

            var baseGame = new VolcanoGame();
            baseGame.LoadTranscript(transcript);

            // sine the board is canonicalized, there are really only two first moves
            var allMoves = transcript == "" ? [0, 1] : baseGame.CurrentState.GetMoves();

            foreach (var firstMove in allMoves)
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
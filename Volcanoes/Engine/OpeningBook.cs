using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volcano.Game;
using static System.Environment;

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

        public delegate void BookGenerationHandler(int completed, int total, string message);

        private const string _fileHeader = "volcanoes-opening-book";

        private bool _stop;

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

        private (Canonical, string) GetCanonicalTranscript(string transcript)
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

            return (canonical, canonicalTranscript);
        }

        public int GetMove(string transcript)
        {
            var (canonical, canonicalTranscript) = GetCanonicalTranscript(transcript);

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

        public void Extend(int depth, int iterations, int gap, bool parallel, bool resume, string bot)
        {
            _stop = false;

            var batch = 2;
            var maxstreak = 100;
            var winstreak = 0;
            var failstreak = 0;
            var first = true;

            while (winstreak < maxstreak && failstreak < maxstreak)
            {
                var transcripts = new List<(Player, string)>();

                if (!string.IsNullOrEmpty(bot) && first)
                {
                    first = false;
                    var gameFolder = $"{Environment.GetFolderPath(SpecialFolder.MyDocuments)}\\My Games\\Volcanoes\\";
                    foreach (var tf in Directory.GetFiles(gameFolder, "*.csv"))
                    {
                        if (tf.Contains("-data-"))
                        {
                            var records = File.ReadAllLines(tf).Skip(1).ToList();
                            foreach (var result in records)
                            {
                                var cols = result.Split(",");
                                if (cols[0] == bot && cols[2] != "One")
                                {
                                    transcripts.Add((Player.One, cols[7]));
                                }
                                else if (cols[1] == bot && cols[2] != "Two")
                                {
                                    transcripts.Add((Player.Two, cols[7]));
                                }
                            }
                        }
                    }

                    OnStatusUpdate?.Invoke(_book.Count, _book.Count, $"Processing {transcripts.Count} tournament losses");
                }
                else
                {
                    transcripts = GenerateLosingTranscripts(batch);
                }

                if (transcripts.Count == 0)
                {
                    winstreak += batch;
                    OnStatusUpdate?.Invoke(_book.Count, _book.Count, $"{_book.Count} positions (win streak {winstreak})");
                    continue;
                }
                else
                {
                    winstreak = 0;
                }

                var count = _book.Count;

                foreach (var loss in transcripts)
                {
                    if (_stop)
                    {
                        return;
                    }

                    var (_, canonicalTranscript) = GetCanonicalTranscript(loss.Item2);

                    // find the first non-book move
                    var player = loss.Item1;
                    var moves = canonicalTranscript.Split(" ");
                    for (var i = 1; i <= Math.Min(depth, moves.Length); i++)
                    {
                        var extra = i < moves.Length && moves[i] == "G" ? 1 : 0;
                        var partial = string.Join(" ", moves.Take(i + extra));
                        if (GetPlayerToMove(partial) == player)
                        {
                            var next = GetMove(partial);
                            if (next == -1)
                            {
                                // update the book with a better move
                                GenerateBookForPosition(depth, iterations, partial, true, gap);
                                OnStatusUpdate?.Invoke(_book.Count, _book.Count, $"{_book.Count} positions");
                                break;
                            }
                        }
                    }
                }

                // was anything added
                if (count == _book.Count)
                {
                    failstreak += batch;
                    OnStatusUpdate?.Invoke(_book.Count, _book.Count, $"{_book.Count} positions (fail streak {failstreak})");
                    continue;
                }
                else
                {
                    failstreak = 0;
                }
            }
        }

        public void Generate(int depth, int iterations, int gap, bool parallel, bool resume)
        {
            _stop = false;

            var done = 0;
            var total = 1;

            // Blue's first move (hardcode to one of the 20 identical equilateral triangles since plenty of computational power says that's slightly better than one of the 60 identical isosceles triangles)
            var blueStart = "N07";
            _book[""] = Constants.TileIndexes[blueStart];
            UpdateBook(depth, iterations, gap);

            //// Prime the root node
            //_engine.forcedIterations = 80;
            //_engine._persistable = true;
            //_engine._persistableFull = true;
            //var prime = new Board();
            //_engine.GetBestMove(prime, 1, new EngineCancellationToken(() => false));
            //prime.MakeMove(_book[""]);
            //_engine.GetBestMove(prime, 1, new EngineCancellationToken(() => false));

            // Blue's second and third move (after all possible moves from orange)
            var allGamesBlue = GetAllTranscriptsAfterPosition(blueStart, false);
            total += allGamesBlue.Count * 2;

            // Orange's first and second move (after all possible moves from blue)
            var allGamesOrange = GetAllTranscriptsAfterPosition("", true);
            total += allGamesOrange.Count * 2;
            total += allGamesOrange.Count * 2;

            var allGames = new List<string>();
            allGames.AddRange(allGamesBlue);
            allGames.AddRange(allGamesOrange);

            //if (resume && File.Exists("book.temp"))
            //{
            //    var completed = File.ReadAllLines("book.temp");
            //    for (var i = allGames.Count - 1; i >= 0; i--)
            //    {
            //        if (completed.Contains(allGames[i]))
            //        {
            //            allGames.RemoveAt(i);
            //            done++;
            //        }
            //    }
            //    OnStatusUpdate?.Invoke(done, total);
            //}

            OnStatusUpdate?.Invoke(_book.Count, total, null);

            // Order by initial depth
            allGames = allGames.OrderBy(x => x.Length).ThenBy(x => x).ToList();

            if (parallel)
            {
                // Warning! This consumes a ton of memory and will grind to a halt if you set an interation count too high!
                Parallel.ForEach(allGames, transcript =>
                {
                    if (_stop)
                    {
                        return;
                    }

                    GenerateBookForPosition(depth, iterations, transcript, false, gap);
                    SaveProgress(transcript);
                    OnStatusUpdate?.Invoke(_book.Count, total, null);
                });
            }
            else
            {
                allGames.ForEach(transcript =>
                {
                    if (_stop)
                    {
                        return;
                    }

                    GenerateBookForPosition(depth, iterations, transcript, false, gap);
                    SaveProgress(transcript);
                    OnStatusUpdate?.Invoke(_book.Count, total, null);
                });
            }

            OnStatusUpdate?.Invoke(_book.Count, total, null);
        }

        private static readonly object _fileLock = new object();

        private void SaveProgress(string transcript)
        {
            lock (_fileLock)
            {
                File.AppendAllLines("book.temp", [transcript]);
            }
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

            // use iterations instead of time so we can run in parallel without starving a thread and getting bad results
            // on my machine a 60 second search resulted in just over 1,000,000 mcts playouts, so roughly 15,000 per second
            var engine = new MonteCarloTreeSearchEngine(MCTSVersion.V2);
            engine.simplifyFirstMove = true;
            engine.forcedIterations = iterations;
            engine.forcedGap = gap;

            if (!_book.ContainsKey(t) && game.CurrentState.GetMoves().Count > 0)
            {
                var best = engine.GetBestMove(game.CurrentState, iterations, new EngineCancellationToken(() => false));

                var b = best.BestMove;

                lock (_book)
                {
                    _book[t] = b;
                }

                game.MakeMove(b);
                bestTranscript = game.GetTranscriptLine().Replace("+", "");

                UpdateBook(depth, iterations, gap);

                if (!singleOnly && game.CurrentState.GetMoves().Count > 0)
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

            if (File.Exists(_file))
            {
                if (File.Exists($"{_file}.tempsave"))
                {
                    File.Delete($"{_file}.tempsave");
                }
                File.Copy(_file, $"{_file}.tempsave");
            }

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

            if (File.Exists($"{_file}.tempsave"))
            {
                File.Delete($"{_file}.tempsave");
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

                //_engine.GetBestMove(firstCopy.CurrentState, 1, new EngineCancellationToken(() => false));

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

                        //_engine.GetBestMove(secondCopy.CurrentState, 1, new EngineCancellationToken(() => false));

                        transcripts.Add(secondCopy.GetTranscriptLine());
                    }
                }
            }

            return transcripts;
        }

        private List<(Player, string)> GenerateLosingTranscripts(int rounds)
        {
            var transcripts = new List<(Player, string)>();
            var games = new List<Action>();

            for (var i = 0; i < rounds; i++)
            {
                games.Add(() =>
                {
                    try
                    {
                        var killswitch = Stopwatch.StartNew();
                        var game = new VolcanoGame();
                        var victory = VictoryType.None;
                        game.OnGameOver += (p, v) => victory = v;

                        var me = new MonteCarloTreeSearchEngine(MCTSVersion.V2, this); // new AlphaZeroEngine(VolcanoZeroVersion.V2, this) { _policyOnly = true }; // new RandomEngine(this);
                        var op = new MonteCarloTreeSearchEngine(MCTSVersion.V2); // new RandomEngine(); // AlphaZeroEngine(VolcanoZeroVersion.v1_6r_128f_233472g) { _policyOnly = true };

                        game.RegisterEngine(Player.One, i % 2 == 0 ? me : op, true);
                        game.RegisterEngine(Player.Two, i % 2 == 0 ? op : me, true);
                        game.SecondsPerEngineMove = 1;
                        game.TimeoutGrace = 5000;
                        game.StartNewGame();
                        game.ComputerPlay();

                        while (victory == VictoryType.None &&
                            game.CurrentState.Winner == Player.Empty &&
                            game.CurrentState.Turn < VolcanoGame.Settings.TournamentAdjudicateMaxTurns &&
                            killswitch.ElapsedMilliseconds < VolcanoGame.Settings.TournamentAdjudicateMaxSeconds * 1000)
                        {
                            System.Threading.Thread.Sleep(100);
                        }

                        game.ForceStop();

                        // played as player 1 but lost
                        if (i % 2 == 0 && game.CurrentState.Winner != Player.One)
                        {
                            lock (transcripts)
                            {
                                transcripts.Add((Player.One, game.GetTranscriptLine()));
                            }
                        }

                        // played as player 2 but lost
                        if (i % 2 == 1 && game.CurrentState.Winner != Player.Two)
                        {
                            lock (transcripts)
                            {
                                transcripts.Add((Player.Two, game.GetTranscriptLine()));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            using (var sw = new StreamWriter("errors.txt", true))
                            {
                                sw.WriteLine("Failed to run validation game: " + ex.ToString());
                            }
                        }
                        catch
                        {
                            // oh well
                        }
                    }
                });
            }

            Parallel.ForEach(games, g => g());

            return transcripts;
        }

        private Player GetPlayerToMove(string transcript)
        {
            var turn = transcript.Split(" ").Length;
            switch (turn % 6)
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

        internal void Cancel()
        {
            _stop = true;
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
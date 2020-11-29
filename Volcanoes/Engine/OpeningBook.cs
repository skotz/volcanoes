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

        public bool Loaded { get; private set; }

        public int Depth { get; private set; }

        public int Seconds { get; private set; }

        public OpeningBook(string file)
        {
            _file = file;
            _book = new Dictionary<string, int>();
            _lock = new SemaphoreSlim(1);

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

        public void Generate(int depth, int seconds)
        {
            var baseGame = new VolcanoGame();

            var allGames = new List<string>();
            allGames.Add("");

            GetAllGames(allGames, baseGame, 1, depth);

            Parallel.ForEach(allGames, transcript =>
            {
                if (!_book.ContainsKey(transcript))
                {
                    var game = new VolcanoGame();
                    game.LoadTranscript(transcript);

                    var engine = new MonteCarloTreeSearchEngine(false);
                    var best = engine.GetBestMove(game.CurrentState, seconds, new EngineCancellationToken(() => false));

                    var t = game.GetTranscriptLine();
                    var b = best.BestMove;

                    _lock.Wait();

                    _book.Add(t, b);

                    using (var r = new StreamWriter(_file))
                    {
                        r.WriteLine(depth);
                        r.WriteLine(_book.Count);
                        r.WriteLine(seconds);

                        foreach (var entry in _book)
                        {
                            r.WriteLine(entry.Key);
                            r.WriteLine(Constants.TileNames[entry.Value]);
                        }
                    }

                    _lock.Release();
                }
            });
        }

        private void GetAllGames(List<string> games, VolcanoGame board, int depth, int maxDepth)
        {
            if (depth >= maxDepth)
            {
                return;
            }

            var game = new VolcanoGame();
            game.LoadTranscript(board.GetTranscriptLine());

            foreach (var move in game.CurrentState.GetMoves())
            {
                var copy = new VolcanoGame();
                copy.LoadTranscript(game.GetTranscriptLine());

                copy.MakeMove(move);

                games.Add(copy.GetTranscriptLine());

                GetAllGames(games, copy, depth + 1, maxDepth);
            }
        }
    }
}
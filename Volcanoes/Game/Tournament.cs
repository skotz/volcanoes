using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Engine;

namespace Volcano.Game
{
    class Tournament
    {
        private int _rounds;
        private string _file;
        private EngineHelper _engines;

        private BackgroundWorker worker;

        public event TournamentOverHandler OnTournamentCompleted;
        public delegate void TournamentOverHandler();

        private bool allowSelfPlay = false;

        public Tournament(int rounds, string resultsFile, EngineHelper engines)
        {
            _rounds = rounds;
            _file = resultsFile;
            _engines = engines;

            worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        public void Start()
        {
            if (!worker.IsBusy)
            {
                worker.RunWorkerAsync();
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Action> games = new List<Action>();
            List<TournamentResult> results = new List<TournamentResult>();

            foreach (var engine1 in _engines.EngineNames)
            {
                foreach (var engine2 in _engines.EngineNames)
                {
                    if (engine1 != engine2 || allowSelfPlay)
                    {
                        games.Add(() =>
                        {
                            try
                            {
                                VolcanoGame game = new VolcanoGame();
                                game.RegisterEngine(Player.One, _engines.GetEngine(engine1));
                                game.RegisterEngine(Player.Two, _engines.GetEngine(engine2));
                                game.StartNewGame();
                                game.ComputerPlay();

                                while (game.CurrentState.Winner == Player.Empty && game.CurrentState.Turn < 1000)
                                {
                                    System.Threading.Thread.Sleep(100);
                                }

                                lock (results)
                                {
                                    results.Add(new TournamentResult(game, engine1, engine2));
                                }
                            }
                            catch (Exception ex)
                            {
                                using (StreamWriter w = new StreamWriter("errors.txt", true))
                                {
                                    w.WriteLine("Failed to play \"" + engine1 + "\" and \"" + engine2 + "\"! :: " + ex.Message);
                                }
                            }
                        });
                    }
                }
            }

            for (int r = 0; r < _rounds; r++)
            {
                Parallel.ForEach(games, x => x());
            }

            using (StreamWriter w = new StreamWriter(_file))
            {
                w.WriteLine("," + _engines.EngineNames.Aggregate((c, n) => c + "," + n) + ",SCORE");

                List<TournamentResultLine> lines = new List<TournamentResultLine>();
                foreach (var engine1 in _engines.EngineNames)
                {
                    decimal score = 0m;
                    TournamentResultLine line = new TournamentResultLine();
                    line.Data += engine1;
                    foreach (var engine2 in _engines.EngineNames)
                    {
                        if (engine1 != engine2 || allowSelfPlay)
                        {
                            decimal wins = results.Where(x => x.PlayerOne == engine1 && x.PlayerTwo == engine2).Sum(x => x.PlayerOneScore) + results.Where(x => x.PlayerOne == engine2 && x.PlayerTwo == engine1).Sum(x => x.PlayerTwoScore);
                            score += wins;
                            line.Data += "," + wins;
                        }
                        else
                        {
                            line.Data += ",";
                        }
                    }
                    line.Data += "," + score;
                    line.Score = score;
                    lines.Add(line);
                }

                lines.Sort((c, n) => n.Score.CompareTo(c.Score));

                foreach (var line in lines)
                {
                    w.WriteLine(line.Data);
                }             
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnTournamentCompleted?.Invoke();
        }
    }

    class TournamentResult
    {
        public string PlayerOne { get; set; }
        public string PlayerTwo { get; set; }
        public decimal PlayerOneScore { get; set; }
        public decimal PlayerTwoScore { get; set; }

        public TournamentResult(VolcanoGame state, string playerOne, string playerTwo)
        {
            PlayerOne = playerOne;
            PlayerTwo = playerTwo;

            if (state.CurrentState.Winner == Player.One)
            {
                PlayerOneScore = 1m;
            }
            else if (state.CurrentState.Winner == Player.Two)
            {
                PlayerTwoScore = 1m;
            }
            else
            {
                // Draw
                PlayerOneScore = 0.5m;
                PlayerTwoScore = 0.5m;
            }
        }

        public TournamentResult()
        {
        }
    }

    class TournamentResultLine
    {
        public string Data { get; set; }
        public decimal Score { get; set; }
    }
}

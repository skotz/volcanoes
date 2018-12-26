using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        private string _dataFile;
        private EngineHelper _engines;
        private List<string> _players;

        private BackgroundWorker worker;

        public event TournamentOverHandler OnTournamentCompleted;
        public delegate void TournamentOverHandler();
        
        public event TournamentStatusHandler OnTournamentStatus;
        public delegate void TournamentStatusHandler(TournamentStatus status);

        private bool allowSelfPlay = false;

        public Tournament(int rounds, string resultsFile, string dataFile, EngineHelper engines, List<string> players)
        {
            _rounds = rounds;
            _file = resultsFile;
            _dataFile = dataFile;
            _engines = engines;
            _players = players;

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.ProgressChanged += Worker_ProgressChanged;
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

            int completedGames = 0;
            int totalGames = _rounds * _players.Count * (_players.Count - 1);

            foreach (var engine1 in _players)
            {
                foreach (var engine2 in _players)
                {
                    if (engine1 != engine2 || allowSelfPlay)
                    {
                        games.Add(() =>
                        {
                            Stopwatch killswitch = Stopwatch.StartNew();
                            VolcanoGame game = new VolcanoGame();

                            VictoryType victory = VictoryType.None;
                            game.OnGameOver += (p, v) => victory = v;

                            try
                            {
                                game.RegisterEngine(Player.One, _engines.GetEngine(engine1));
                                game.RegisterEngine(Player.Two, _engines.GetEngine(engine2));
                                game.StartNewGame();
                                game.ComputerPlay();

                                while (victory == VictoryType.None && game.CurrentState.Winner == Player.Empty && game.CurrentState.Turn < VolcanoGame.Settings.TournamentAdjudicateMaxTurns && killswitch.ElapsedMilliseconds < VolcanoGame.Settings.TournamentAdjudicateMaxSeconds * 1000)
                                {
                                    System.Threading.Thread.Sleep(100);
                                }

                                var termination = TournamentTerminationType.Normal;
                                if (game.CurrentState.Winner == Player.Empty)
                                {
                                    if (game.CurrentState.Turn >= VolcanoGame.Settings.TournamentAdjudicateMaxTurns)
                                    {
                                        termination = TournamentTerminationType.AdjudicateMoves;
                                    }
                                    else if (killswitch.ElapsedMilliseconds >= VolcanoGame.Settings.TournamentAdjudicateMaxSeconds * 1000)
                                    {
                                        termination = TournamentTerminationType.AdjudicateTime;
                                    }
                                }
                                if (victory == VictoryType.ArenaAdjudication)
                                {
                                    termination = TournamentTerminationType.IllegalMove;
                                }

                                lock (results)
                                {
                                    results.Add(new TournamentResult(game, engine1, engine2, termination, killswitch.ElapsedMilliseconds));
                                }
                            }
                            catch (Exception ex)
                            {
                                using (StreamWriter w = new StreamWriter("errors.txt", true))
                                {
                                    w.WriteLine("Failed to play \"" + engine1 + "\" and \"" + engine2 + "\"! :: " + ex.Message);
                                }

                                try
                                {
                                    var termination = game?.CurrentState?.Player == Player.One ? TournamentTerminationType.PlayerOneError : TournamentTerminationType.PlayerTwoError;

                                    lock (results)
                                    {
                                        results.Add(new TournamentResult(game, engine1, engine2, termination, killswitch.ElapsedMilliseconds));
                                    }
                                }
                                catch (Exception eotw)
                                {
                                    using (StreamWriter w2 = new StreamWriter("errors.txt", true))
                                    {
                                        w2.WriteLine("Failed to log result! :: " + eotw.Message);
                                    }
                                }
                            }

                            completedGames++;
                            worker.ReportProgress(0, new TournamentStatus(completedGames, totalGames));
                        });
                    }
                }
            }

            for (int r = 0; r < _rounds; r++)
            {
                Parallel.ForEach(games, x => x());
            }

            using (StreamWriter w = new StreamWriter(_dataFile))
            {
                w.WriteLine("Player One,Player Two,Winner,Termination,Total Moves,Total Milliseconds,Starting Tile Index,Transcript");
                foreach (var result in results)
                {
                    string gameResult = "Draw";
                    if (result.PlayerOneScore > result.PlayerTwoScore)
                    {
                        gameResult = "One";
                    }
                    if (result.PlayerTwoScore > result.PlayerOneScore)
                    {
                        gameResult = "Two";
                    }

                    string transcript = "";
                    if (result.Moves.Count > 0)
                    {
                        try
                        {
                            transcript = result.Moves.Select(x => Constants.TileNames[x]).Aggregate((c, n) => c + " " + n);
                        }
                        catch (Exception ex)
                        {
                            transcript = ex.Message;
                        }
                    }

                    w.WriteLine(result.PlayerOne + "," + result.PlayerTwo + "," + gameResult + "," + result.Termination.ToString() + "," + result.TotalMoves + "," + result.ElapsedMilliseconds + "," + result.FirstTile + "," + transcript);
                }
            }

            List<TournamentResultLine> lines = new List<TournamentResultLine>();
            foreach (var engine1 in _players)
            {
                decimal score = 0m;
                TournamentResultLine line = new TournamentResultLine();
                line.Name = engine1;
                foreach (var engine2 in _players)
                {
                    if (engine1 != engine2 || allowSelfPlay)
                    {
                        decimal wins = results.Where(x => x.PlayerOne == engine1 && x.PlayerTwo == engine2).Sum(x => x.PlayerOneScore) + results.Where(x => x.PlayerOne == engine2 && x.PlayerTwo == engine1).Sum(x => x.PlayerTwoScore);
                        score += wins;
                        line.Data.Add(engine2, wins.ToString());
                    }
                    else
                    {
                        line.Data.Add(engine2, "");
                    }
                }
                decimal total = results.Where(x => x.PlayerOne == engine1 || x.PlayerTwo == engine1).Count();
                line.TotalScore = score;
                line.TotalPercentage = score * 100m / total;
                line.Score = score;
                lines.Add(line);
            }

            lines.Sort((c, n) => n.Score.CompareTo(c.Score));

            using (StreamWriter w = new StreamWriter(_file))
            {
                w.WriteLine("," + lines.Select(x => x.Name).Aggregate((c, n) => c + "," + n) + ",Wins,Percentage");
                foreach (var player in lines)
                {
                    w.Write(player.Name);
                    foreach (var opponent in lines)
                    {
                        w.Write("," + player.Data[opponent.Name]);
                    }
                    w.Write(", " + player.TotalScore);
                    w.Write(", " + player.TotalPercentage.ToString("0.00"));
                    w.WriteLine();
                }
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            OnTournamentStatus?.Invoke(e.UserState as TournamentStatus);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnTournamentCompleted?.Invoke();
        }
    }

    class TournamentStatus
    {
        public int CompletedGames { get; set; }
        public int TotalGames { get; set; }
        public decimal PercentageComplete
        {
            get
            {
                return TotalGames > 0 ? 100m * CompletedGames / TotalGames : 0m;
            }
        }

        public TournamentStatus(int completed, int total)
        {
            CompletedGames = completed;
            TotalGames = total;
        }
    }

    class TournamentResult
    {
        public string PlayerOne { get; set; }
        public string PlayerTwo { get; set; }
        public decimal PlayerOneScore { get; set; }
        public decimal PlayerTwoScore { get; set; }

        public TournamentTerminationType Termination { get; set; }
        public int FirstTile { get; set; }
        public int TotalMoves { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public List<int> Moves { get; set; }

        public TournamentResult(VolcanoGame state, string playerOne, string playerTwo, TournamentTerminationType termination, long milliseconds)
        {
            PlayerOne = playerOne;
            PlayerTwo = playerTwo;

            Termination = termination;
            ElapsedMilliseconds = milliseconds;

            if (state.CurrentState.Winner == Player.One || termination == TournamentTerminationType.PlayerTwoError)
            {
                PlayerOneScore = 1m;
            }
            else if (state.CurrentState.Winner == Player.Two || termination == TournamentTerminationType.PlayerOneError)
            {
                PlayerTwoScore = 1m;
            }
            else
            {
                // Draw
                PlayerOneScore = 0.5m;
                PlayerTwoScore = 0.5m;
            }

            Moves = state.MoveHistory;
            TotalMoves = state?.CurrentState?.Turn ?? 0;
            if (state.MoveHistory.Count > 0)
            {
                FirstTile = state.MoveHistory[0];
            }
        }

        public TournamentResult()
        {
        }
    }

    enum TournamentTerminationType
    {
        Normal,
        AdjudicateTime,
        AdjudicateMoves,
        PlayerOneError,
        PlayerTwoError,
        IllegalMove
    }

    class TournamentResultLine
    {
        public string Name { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public decimal Score { get; set; }
        public decimal TotalScore { get; set; }
        public decimal TotalPercentage { get; set; }

        public TournamentResultLine()
        {
            Data = new Dictionary<string, string>();
        }
    }
}

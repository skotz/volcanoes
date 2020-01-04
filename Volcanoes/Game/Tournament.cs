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
        private int _secondsPerMove;
        private string _crossTableFile;
        private string _gameDataFile;
        private EngineHelper _engines;
        private List<string> _players;
        private bool _allowSelfPlay;

        int totalGames;

        private BackgroundWorker worker;

        public event TournamentOverHandler OnTournamentCompleted;
        public delegate void TournamentOverHandler();
        
        public event TournamentStatusHandler OnTournamentStatus;
        public delegate void TournamentStatusHandler(TournamentStatus status);


        public Tournament(int rounds, int secondsPerMove, string crossTableFile, string gameDataFile, EngineHelper engines, List<string> players, bool allowSelfPlay)
        {
            _rounds = rounds;
            _secondsPerMove = secondsPerMove;
            _crossTableFile = crossTableFile;
            _gameDataFile = gameDataFile;
            _engines = engines;
            _players = players;
            _allowSelfPlay = allowSelfPlay;

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
                totalGames = _rounds * _players.Count * (_allowSelfPlay ? _players.Count : (_players.Count - 1));
                OnTournamentStatus?.Invoke(new TournamentStatus(0, totalGames));
                worker.RunWorkerAsync();
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<Action> games = new List<Action>();
            List<TournamentResult> results = new List<TournamentResult>();

            int completedGames = 0;

            foreach (var engine1 in _players)
            {
                foreach (var engine2 in _players)
                {
                    if (engine1 != engine2 || _allowSelfPlay)
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
                                game.SecondsPerEngineMove = _secondsPerMove;
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
                                    if (game.BackgroundError != null)
                                    {
                                        if (game.CurrentState.Winner == Player.One)
                                        {
                                            termination = TournamentTerminationType.PlayerTwoError;
                                        }
                                        else if (game.CurrentState.Winner == Player.Two)
                                        {
                                            termination = TournamentTerminationType.PlayerOneError;
                                        }
                                        else
                                        {
                                            termination = TournamentTerminationType.Error;
                                        }
                                    }
                                    else
                                    {
                                        termination = TournamentTerminationType.IllegalMove;
                                    }
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

            using (StreamWriter w = new StreamWriter(_gameDataFile))
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
                            transcript = result.Moves.Select(x => x.Tile + (VolcanoGame.Settings.IndicateTranscriptMoveType && x.Addition ? "+" : "")).Aggregate((c, n) => c + " " + n);
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
                    if (engine1 != engine2 || _allowSelfPlay)
                    {
                        decimal wins = results.Where(x => x.PlayerOne == engine1 && x.PlayerTwo == engine2).Sum(x => x.PlayerOneScore) + results.Where(x => x.PlayerOne == engine2 && x.PlayerTwo == engine1).Sum(x => x.PlayerTwoScore);
                        score += wins;
                        line.Data.Add(engine2, wins);
                    }
                    else
                    {
                        line.Data.Add(engine2, -1);
                    }
                }
                decimal total = results.Where(x => x.PlayerOne == engine1 || x.PlayerTwo == engine1).Count();
                line.TotalScore = score;
                line.TotalPercentage = score * 100m / total;
                line.Score = score;
                lines.Add(line);
            }

            lines.Sort((c, n) => n.Sort.CompareTo(c.Sort));

            List<string> names = new List<string>();
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].CrossTableName = (i + 1) + ". " + lines[i].Name;
                names.Add((i + 1).ToString());

                lines[i].NeustadtlScore = 0m;
                foreach (var opponent in lines)
                {
                    if (opponent.Name != lines[i].Name)
                    {
                        lines[i].NeustadtlScore += opponent.TotalScore * lines[i].Data[opponent.Name];
                    }
                }
            }

            // It doesn't make sense to save a cross table when there's only one person competing
            if (!string.IsNullOrEmpty(_crossTableFile) && lines.Count > 1)
            {
                using (StreamWriter w = new StreamWriter(_crossTableFile))
                {
                    // cs = common score
                    // ns = neustadtl score (figures in strength of opposition)
                    w.WriteLine("," + names.Aggregate((c, n) => c + "," + n) + ",cs,ns");
                    foreach (var player in lines)
                    {
                        w.Write(player.CrossTableName);
                        foreach (var opponent in lines)
                        {
                            string val = player.Data[opponent.Name] >= 0 ? player.Data[opponent.Name].ToString() : "";
                            w.Write("," + val);
                        }
                        w.Write(", " + player.TotalScore);
                        w.Write(", " + player.NeustadtlScore.ToString("0.00"));
                        w.WriteLine();
                    }
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
        public List<Move> Moves { get; set; }

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

            try
            {
                Moves = state.GetDetailedMoveList();
            }
            catch (Exception ex)
            {
                using (StreamWriter w = new StreamWriter("errors.txt", true))
                {
                    w.WriteLine("Failed to copy move history! :: " + ex.Message);
                }
                Moves = new List<Move>();
            }
            TotalMoves = state?.CurrentState?.Turn ?? 0;
            if (Moves.Count > 0)
            {
                FirstTile = Moves[0].Location;
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
        Error,
        IllegalMove
    }

    class TournamentResultLine
    {
        public string Name { get; set; }
        public string CrossTableName { get; set; }
        public Dictionary<string, decimal> Data { get; set; }
        public decimal Score { get; set; }
        public decimal TotalScore { get; set; }
        public decimal TotalPercentage { get; set; }
        public decimal NeustadtlScore { get; set; }

        public decimal Sort
        {
            get
            {
                return Score * 100000 + NeustadtlScore;
            }
        }

        public TournamentResultLine()
        {
            Data = new Dictionary<string, decimal>();
        }
    }
}

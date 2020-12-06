using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volcano.Engine;

namespace Volcano.Game
{
    internal class Tournament
    {
        private int _rounds;
        private int _secondsPerMove;
        private string _crossTableFile;
        private string _gameDataFile;
        private EngineHelper _engines;
        private List<string> _players;
        private bool _allowSelfPlay;
        private TournamentType _tournamentType;

        private List<Action> games;
        private List<TournamentResult> results;
        private int completedGames = 0;
        private int timeouts = 0;

        private int totalGames;

        private BackgroundWorker worker;

        public event TournamentOverHandler OnTournamentCompleted;

        public delegate void TournamentOverHandler();

        public event TournamentStatusHandler OnTournamentStatus;

        public delegate void TournamentStatusHandler(TournamentStatus status);

        public Tournament(int rounds, int secondsPerMove, string crossTableFile, string gameDataFile, EngineHelper engines, List<string> players, bool allowSelfPlay, TournamentType tournamentType)
        {
            _rounds = rounds;
            _secondsPerMove = secondsPerMove;
            _crossTableFile = crossTableFile;
            _gameDataFile = gameDataFile;
            _engines = engines;
            _players = players;
            _allowSelfPlay = allowSelfPlay;
            _tournamentType = tournamentType;

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
                if (_tournamentType == TournamentType.RoundRobin)
                {
                    totalGames = _rounds * _players.Count * (_allowSelfPlay ? _players.Count : (_players.Count - 1));
                }
                else if (_tournamentType == TournamentType.Swiss)
                {
                    totalGames = _rounds * (_players.Count % 2 == 1 ? _players.Count - 1 : _players.Count);
                }
                OnTournamentStatus?.Invoke(new TournamentStatus(0, totalGames, 0));
                worker.RunWorkerAsync();
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            results = new List<TournamentResult>();

            completedGames = 0;
            timeouts = 0;

            if (_tournamentType == TournamentType.RoundRobin)
            {
                games = new List<Action>();

                foreach (var engine1 in _players)
                {
                    foreach (var engine2 in _players)
                    {
                        if (engine1 != engine2 || _allowSelfPlay)
                        {
                            QueueGame(engine1, engine2);
                        }
                    }
                }

                for (int r = 0; r < _rounds; r++)
                {
                    Parallel.ForEach(games, x => x());
                }
            }
            else if (_tournamentType == TournamentType.Swiss)
            {
                var rand = new Random();

                var swissPlayers = _players.Select(x => new SwissPlayer
                {
                    Engine = x,
                    Score = 0
                }).ToList();

                for (int r = 0; r < _rounds; r++)
                {
                    swissPlayers = swissPlayers.OrderByDescending(x => x.Score).ThenBy(x => rand.Next()).ToList();

                    games = new List<Action>();

                    // If there's an odd number of players, the last player doesn't play
                    for (int i = 0; i < ((swissPlayers.Count % 2 == 1) ? swissPlayers.Count - 1 : swissPlayers.Count); i += 2)
                    {
                        // Play both colors
                        QueueGame(swissPlayers[i].Engine, swissPlayers[i + 1].Engine);
                        QueueGame(swissPlayers[i + 1].Engine, swissPlayers[i].Engine);
                    }

                    Parallel.ForEach(games, x => x());

                    // Update scores for next round
                    for (int i = 0; i < ((swissPlayers.Count % 2 == 1) ? swissPlayers.Count - 1 : swissPlayers.Count); i++)
                    {
                        swissPlayers[i].Score = results.Where(x => x.PlayerOne == swissPlayers[i].Engine).Sum(x => x.PlayerOneScore);
                        swissPlayers[i].Score += results.Where(x => x.PlayerTwo == swissPlayers[i].Engine).Sum(x => x.PlayerTwoScore);
                    }

                    // If someone didn't play, they get a half point bye (per game) for the sake of pairings
                    if (swissPlayers.Count % 2 == 1)
                    {
                        swissPlayers[swissPlayers.Count - 1].Score++;
                    }
                }
            }

            using (StreamWriter w = new StreamWriter(_gameDataFile))
            {
                w.WriteLine("Player One,Player Two,Winner,Termination,Total Moves,Total Milliseconds,Starting Tile Index,Transcript,Winning Path One,Winning Path Two");
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

                    string winningPathOne = GetWinningPath(result.WinningPathOne);
                    string winningPathTwo = GetWinningPath(result.WinningPathTwo);

                    w.WriteLine(result.PlayerOne + "," + result.PlayerTwo + "," + gameResult + "," + result.Termination.ToString() + "," + result.TotalMoves + "," + result.ElapsedMilliseconds + "," + result.FirstTile + "," + transcript + "," + winningPathOne + "," + winningPathTwo);
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

        private void QueueGame(string engine1, string engine2)
        {
            games.Add(() =>
            {
                Stopwatch killswitch = Stopwatch.StartNew();
                VolcanoGame game = new VolcanoGame();

                VictoryType victory = VictoryType.None;
                game.OnGameOver += (p, v) => victory = v;

                try
                {
                    game.RegisterEngine(Player.One, _engines.GetEngine(engine1), true);
                    game.RegisterEngine(Player.Two, _engines.GetEngine(engine2), true);
                    game.SecondsPerEngineMove = _secondsPerMove;
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
                    else if (victory == VictoryType.OpponentTimeout)
                    {
                        termination = TournamentTerminationType.Timeout;
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
                worker.ReportProgress(0, new TournamentStatus(completedGames, totalGames, timeouts));
            });
        }

        private static string GetWinningPath(List<int> path)
        {
            string winningPath = "";
            if (path != null && path.Count > 0)
            {
                try
                {
                    winningPath = path.Select(x => Constants.TileNames[x]).Aggregate((c, n) => c + " " + n);
                }
                catch (Exception ex)
                {
                    winningPath = ex.Message;
                }
            }

            return winningPath;
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

    internal class SwissPlayer
    {
        public string Engine { get; set; }

        public decimal Score { get; set; }
    }

    internal class TournamentStatus
    {
        public int CompletedGames { get; set; }
        public int TotalGames { get; set; }
        public int Timeouts { get; set; }

        public decimal PercentageComplete
        {
            get
            {
                return TotalGames > 0 ? 100m * CompletedGames / TotalGames : 0m;
            }
        }

        public TournamentStatus(int completed, int total, int timeouts)
        {
            CompletedGames = completed;
            TotalGames = total;
            Timeouts = timeouts;
        }
    }

    internal class TournamentResult
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
        public List<int> WinningPathOne { get; set; }
        public List<int> WinningPathTwo { get; set; }

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

            WinningPathOne = state?.CurrentState?.WinningPathPlayerOne ?? new List<int>();
            WinningPathTwo = state?.CurrentState?.WinningPathPlayerTwo ?? new List<int>();
        }

        public TournamentResult()
        {
        }
    }

    internal enum TournamentTerminationType
    {
        Normal,
        AdjudicateTime,
        AdjudicateMoves,
        PlayerOneError,
        PlayerTwoError,
        Error,
        IllegalMove,
        Timeout
    }

    internal class TournamentResultLine
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
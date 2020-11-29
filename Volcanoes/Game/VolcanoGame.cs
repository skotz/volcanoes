﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Volcano.Engine;

namespace Volcano.Game
{
    internal class VolcanoGame
    {
        public Board CurrentState { get; set; }

        private IEngine _playerOneEngine;
        private IEngine _playerTwoEngine;

        private BackgroundWorker _worker;
        private SearchResult _lastSearch;

        public event GameOverHandler OnGameOver;

        public delegate void GameOverHandler(Player winner, VictoryType type);

        public event MoveMadeHandler OnMoveMade;

        public delegate void MoveMadeHandler(bool growthHappened);

        public event EngineStatusHandler OnEngineStatus;

        public delegate void EngineStatusHandler(Player player, EngineStatus status);

        public List<int> MoveHistory { get; private set; }

        public bool Thinking { get { return _worker.IsBusy; } }

        public Exception BackgroundError { get; private set; }

        public int NodesPerSecond { get { return _lastSearch?.NodesPerSecond ?? 0; } }

        public int SecondsPerEngineMove { get; set; } = 10;

        public static GameSettings Settings = GameSettings.LoadOrDefault("volcano.json");

        public VolcanoGame()
        {
            CurrentState = new Board();
            MoveHistory = new List<int>();

            _worker = new BackgroundWorker();
            _worker.DoWork += BackgroundWork;
            _worker.RunWorkerCompleted += BackgroundWorkCompleted;
            _worker.WorkerSupportsCancellation = true;
            _worker.WorkerReportsProgress = true;
            _worker.ProgressChanged += BackgroundWorkProgressChanged;
        }

        public void StartNewGame()
        {
            if (_worker.IsBusy)
            {
                _worker.CancelAsync();
            }

            CurrentState = new Board();
            MoveHistory = new List<int>();
        }

        public bool LoadTranscript(string transcript)
        {
            CurrentState = new Board();
            MoveHistory = new List<int>();

            if (!string.IsNullOrEmpty(transcript))
            {
                string[] lines = transcript.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    string[] moves = line.Split(new string[] { " ", "+" }, StringSplitOptions.RemoveEmptyEntries);
                    if (moves.Length > 0 && Constants.TileIndexes.ContainsKey(moves[0]))
                    {
                        foreach (string move in moves)
                        {
                            if (Constants.TileIndexes.ContainsKey(move))
                            {
                                int index = Constants.TileIndexes[move];
                                if (index != Constants.AllGrowMove)
                                {
                                    MakeMove(index);
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }

                return MoveHistory.Count > 0;
            }

            return false;
        }

        public Board GetPreviousState(int turn)
        {
            Board board = new Board();
            for (int i = 0; i < Math.Min(turn, MoveHistory.Count); i++)
            {
                board.MakeMove(MoveHistory[i], true, false);
            }
            return board;
        }

        public void RegisterEngine(Player player, IEngine engine)
        {
            if (player == Player.One)
            {
                _playerOneEngine = engine;

                var status = _playerOneEngine as IStatus;
                if (status != null)
                {
                    status.OnStatus += Status_OnStatusOne;
                }
            }
            if (player == Player.Two)
            {
                _playerTwoEngine = engine;

                var status = _playerTwoEngine as IStatus;
                if (status != null)
                {
                    status.OnStatus += Status_OnStatusTwo;
                }
            }
        }

        private void BackgroundWorkProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            OnEngineStatus?.Invoke(e.ProgressPercentage == 1 ? Player.One : Player.Two, e.UserState as EngineStatus);
        }

        private void Status_OnStatusOne(object sender, EngineStatus e)
        {
            _worker.ReportProgress(1, e);
        }

        private void Status_OnStatusTwo(object sender, EngineStatus e)
        {
            _worker.ReportProgress(2, e);
        }

        public void MakeMove(int move)
        {
            if (CurrentState.IsValidMove(move))
            {
                bool growthHappened = CurrentState.MakeMove(move);

                MoveHistory.Add(move);
                if (growthHappened)
                {
                    MoveHistory.Add(Constants.AllGrowMove);
                }
                OnMoveMade?.Invoke(growthHappened);

                if (CurrentState.State == GameState.GameOver)
                {
                    OnGameOver?.Invoke(CurrentState.Winner, CurrentState.Winner == Player.Draw ? VictoryType.InfiniteEruption : VictoryType.AntipodePathCreation);
                }
                else
                {
                    ComputerPlay();
                }
            }
        }

        public void ComputerPlay()
        {
            if (!_worker.IsBusy && CurrentState.State == GameState.InProgress)
            {
                if (CurrentState.Player == Player.One && _playerOneEngine != null || CurrentState.Player == Player.Two && _playerTwoEngine != null)
                {
                    _worker.RunWorkerAsync();
                }
            }
        }

        public List<Move> GetDetailedMoveList()
        {
            var moves = new List<Move>();
            var board = new Board();

            for (int i = 0; i < MoveHistory.Count; i++)
            {
                board.MakeMove(MoveHistory[i], true, false);
                moves.Add(new Move(MoveHistory[i], board.LastMoveIncreasedTile));
            }

            return moves;
        }

        public string GetTranscriptLine()
        {
            if (MoveHistory.Count > 0)
            {
                if (Settings.IndicateTranscriptMoveType)
                {
                    var moves = GetDetailedMoveList();
                    return moves.Select(x => x.Tile + (x.Addition ? "+" : "")).Aggregate((c, n) => c + " " + n);
                }
                else
                {
                    return MoveHistory.Select(x => Constants.TileNames[x]).Aggregate((c, n) => c + " " + n);
                }
            }

            return "";
        }

        public string GetTranscript(bool includeHeader)
        {
            if (MoveHistory.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                if (includeHeader)
                {
                    sb.AppendLine("[Volcanoes Saved Game Transcript v1]");
                    sb.AppendLine("[Date " + DateTime.Now.ToString("yyyy.MM.dd") + "]");
                    sb.AppendLine("");
                }

                sb.AppendLine(GetTranscriptLine());

                return sb.ToString();
            }

            return "";
        }

        private void BackgroundWork(object sender, DoWorkEventArgs e)
        {
            EngineCancellationToken token = new EngineCancellationToken(_worker);

            try
            {
                BackgroundError = null;

                CurrentState.Transcript = GetTranscriptLine();

                if (CurrentState.Player == Player.One)
                {
                    e.Result = _playerOneEngine.GetBestMove(CurrentState, SecondsPerEngineMove, token);
                }
                else if (CurrentState.Player == Player.Two)
                {
                    e.Result = _playerTwoEngine.GetBestMove(CurrentState, SecondsPerEngineMove, token);
                }
            }
            catch (Exception ex)
            {
                BackgroundError = ex;

                try
                {
                    using (StreamWriter w = new StreamWriter("errors.txt", true))
                    {
                        w.WriteLine("Failed to get a move from an engine! :: " + ex.Message);
                    }
                }
                catch { /* TODO */ }

                e.Result = new SearchResult();
            }

            if (token.Cancelled)
            {
                e.Cancel = true;
            }
        }

        private void BackgroundWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                _lastSearch = (SearchResult)e.Result;

                if (_lastSearch.BestMove >= 0 && CurrentState.IsValidMove(_lastSearch.BestMove))
                {
                    bool growthHappened = CurrentState.MakeMove(_lastSearch.BestMove);

                    MoveHistory.Add(_lastSearch.BestMove);
                    if (growthHappened)
                    {
                        MoveHistory.Add(Constants.AllGrowMove);
                    }
                    OnMoveMade?.Invoke(growthHappened);

                    if (CurrentState.State == GameState.GameOver)
                    {
                        OnGameOver?.Invoke(CurrentState.Winner, CurrentState.Winner == Player.Draw ? VictoryType.InfiniteEruption : VictoryType.AntipodePathCreation);
                    }
                    else
                    {
                        ComputerPlay();
                    }
                }
                else
                {
                    // If an engine doesn't find a move, then he adjourns the game
                    CurrentState.Winner = CurrentState.Player == Player.One ? Player.Two : Player.One;
                    OnGameOver?.Invoke(CurrentState.Winner, VictoryType.ArenaAdjudication);
                }
            }
        }

        public void ForceStop()
        {
            if (_worker.IsBusy)
            {
                _worker.CancelAsync();
            }
        }
    }
}
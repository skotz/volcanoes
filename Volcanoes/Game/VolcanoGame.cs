using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Engine;

namespace Volcano.Game
{
    class VolcanoGame
    {
        public Board CurrentState { get; set; }

        private IEngine _playerOneEngine;
        private IEngine _playerTwoEngine;

        private BackgroundWorker _worker;
        private SearchResult _lastSearch;

        public event GameOverHandler OnGameOver;
        public delegate void GameOverHandler(Player winner);

        public List<int> MoveHistory { get; private set; }

        public bool Thinking
        {
            get
            {
                return _worker.IsBusy;
            }
        }

        public int NodesPerSecond { get { return _lastSearch?.NodesPerSecond ?? 0; } }

        public VolcanoGame()
        {
            CurrentState = new Board();
            MoveHistory = new List<int>();

            _worker = new BackgroundWorker();
            _worker.DoWork += BackgroundWork;
            _worker.RunWorkerCompleted += BackgroundWorkCompleted;
        }

        public void StartNewGame()
        {
            CurrentState = new Board();
            MoveHistory = new List<int>();
        }

        public void RegisterEngine(Player player, IEngine engine)
        {
            if (player == Player.One)
            {
                _playerOneEngine = engine;
            }
            if (player == Player.Two)
            {
                _playerTwoEngine = engine;
            }
        }

        public void MakeMove(int tileIndex)
        {
            Move move = new Move(tileIndex, MoveType.SingleGrow);
            if (CurrentState.IsValidMove(move))
            {
                CurrentState.MakeMove(move);
                MoveHistory.Add(move.TileIndex);

                if (CurrentState.State == GameState.GameOver)
                {
                    OnGameOver?.Invoke(CurrentState.Winner);
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

        private void BackgroundWork(object sender, DoWorkEventArgs e)
        {
            if (CurrentState.Player == Player.One)
            {
                e.Result = _playerOneEngine.GetBestMove(CurrentState);
            }
            else if (CurrentState.Player == Player.Two)
            {
                e.Result = _playerTwoEngine.GetBestMove(CurrentState);
            }
        }

        private void BackgroundWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _lastSearch = (SearchResult)e.Result;
            CurrentState.MakeMove(_lastSearch.BestMove);
            MoveHistory.Add(_lastSearch.BestMove.TileIndex);

            if (CurrentState.State == GameState.GameOver)
            {
                OnGameOver?.Invoke(CurrentState.Winner);
            }
            else
            {
                ComputerPlay();
            }
        }
    }
}

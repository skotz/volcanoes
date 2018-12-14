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

        public event GameOverHandler OnGameOver;
        public delegate void GameOverHandler(Player winner);

        public VolcanoGame()
        {
            CurrentState = new Board();

            _worker = new BackgroundWorker();
            _worker.DoWork += BackgroundWork;
            _worker.RunWorkerCompleted += BackgroundWorkCompleted;
        }

        public void StartNewGame()
        {
            CurrentState = new Board();
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
            if (!_worker.IsBusy)
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
            CurrentState.MakeMove(e.Result as Move);

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

using Volcano.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Volcano.Game;
using System.IO;
using Volcano.Engine;

namespace Volcano
{
    public partial class GameForm : Form
    {
        GameGraphics graphics;
        VolcanoGame game;

        int autoPlay = 0;

        public GameForm()
        {
            InitializeComponent();
            
            graphics = new GameGraphics(gamePanel, GameGraphicsSettings.Default);
            game = new VolcanoGame();
            game.OnGameOver += Game_OnGameOver;

            ConfigureComputer();

            gameTimer.Start();
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            graphics.Draw(game.CurrentState, gamePanel.PointToClient(Cursor.Position));
        }

        private void gamePanel_Click(object sender, EventArgs e)
        {
            if (game.CurrentState.State == GameState.InProgress)
            {
                Point mouse = gamePanel.PointToClient(Cursor.Position);
                int tileIndex = graphics.GetTileIndex(mouse);
                game.MakeMove(tileIndex);
            }
        }

        private void btnNewGame_Click(object sender, EventArgs e)
        {
            StartNewGame();
        }

        private void StartNewGame()
        {
            game.StartNewGame();
            ConfigureComputer();
        }

        private void ddlPlayerOne_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConfigureComputer();
        }

        private void ddlPlayerTwo_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConfigureComputer();
        }

        private void ConfigureComputer()
        {
            switch (ddlPlayerOne.Text)
            {
                case "Human":
                    game.RegisterEngine(Player.One, null);
                    break;
                case "Random AI":
                    game.RegisterEngine(Player.One, new RandomEngine());
                    break;
            }

            switch (ddlPlayerTwo.Text)
            {
                case "Human":
                    game.RegisterEngine(Player.Two, null);
                    break;
                case "Random AI":
                    game.RegisterEngine(Player.Two, new RandomEngine());
                    break;
            }

            game.ComputerPlay();
        }

        private void btnRunTests_Click(object sender, EventArgs e)
        {
            autoPlay = 1000;
            StartNewGame();
        }

        private void Game_OnGameOver(Player winner)
        {
            if (autoPlay-- > 0)
            {
                using (StreamWriter w = new StreamWriter("data.csv", true))
                {
                    w.WriteLine("\"" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\"," + game.CurrentState.Turn + "," + winner.ToString());
                }

                StartNewGame();
            }
        }
    }
}

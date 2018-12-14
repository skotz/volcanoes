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

        public GameForm()
        {
            InitializeComponent();
            
            graphics = new GameGraphics(gamePanel, GameGraphicsSettings.Default);
            game = new VolcanoGame();

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
            game = new VolcanoGame();
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
    }
}

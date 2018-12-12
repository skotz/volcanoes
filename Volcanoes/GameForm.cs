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

namespace Volcano
{
    public partial class GameForm : Form
    {
        GameGraphics graphics;
        VolcanoGame game;

        public GameForm()
        {
            InitializeComponent();
            
            graphics = new GameGraphics(gamePanel, GetGraphicsSettings());

            game = new VolcanoGame();

            gameTimer.Start();
        }

        private static GameGraphicsSettings GetGraphicsSettings()
        {
            string file = "volcanoes.json";
            GameGraphicsSettings settings = GameGraphicsSettings.Default;

            if (File.Exists(file))
            {
                settings = GameGraphicsSettings.Load(file);
            }
            else
            {
                settings.Save(file);
            }

            return settings;
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
    }
}

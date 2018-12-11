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

namespace Volcano
{
    public partial class GameForm : Form
    {
        GameGraphics g;
        VolcanoGame game;

        public GameForm()
        {
            InitializeComponent();

            var settings = new GameGraphicsSettings(80, 8, 14);
            g = new GameGraphics(gamePanel, settings);

            game = new VolcanoGame();

            gameTimer.Start();
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            g.Draw(game.CurrentState);
        }
    }
}

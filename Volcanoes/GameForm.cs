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
        EngineHelper engines;

        int autoPlay = 0;

        public GameForm()
        {
            InitializeComponent();
            
            graphics = new GameGraphics(gamePanel, GameGraphicsSettings.Default);
            game = new VolcanoGame();
            game.OnGameOver += Game_OnGameOver;

            gameTimer.Start();

            engines = new EngineHelper();
            engines.Add<RandomEngine>("Random");
            engines.Add<LongestPathEngine>("Longest Path L1");
            engines.Add("MiniMax Alpha-Beta L4", () => new MiniMaxAlphaBetaEngine(4));
            engines.Add<AlphaEngine>("Alpha Tile");
            engines.Add<SkipTileEngine>("Tile Skipper");
            engines.Add<KittyCornerEngine>("Kitty Corner");

            foreach (string engine in engines.EngineNames)
            {
                ddlPlayerOne.Items.Add(engine);
                ddlPlayerTwo.Items.Add(engine);
            }

            ConfigureComputer();
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            graphics.Draw(game, gamePanel.PointToClient(Cursor.Position));
        }

        private void gamePanel_Click(object sender, EventArgs e)
        {
            if (game.CurrentState.State == GameState.InProgress && !game.Thinking)
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
            game.RegisterEngine(Player.One, engines.GetEngine(ddlPlayerOne.Text));
            game.RegisterEngine(Player.Two, engines.GetEngine(ddlPlayerTwo.Text));

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

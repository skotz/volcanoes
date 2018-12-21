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

        int totalAutoPlay = 0;
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
            //engines.Add<LongestPathEngine>("Longest Path L1"); // Garbage AI
            //engines.Add("MiniMax Alpha-Beta L4", () => new MiniMaxAlphaBetaEngine(4)); // Garbage AI
            //engines.Add<AlphaEngine>("Alpha Tile"); // Garbage AI
            engines.Add<SkipTileEngine>("Tile Skipper");
            engines.Add<KittyCornerEngine>("Kitty Corner");
            engines.Add<BeeLineEngine>("Bee Line");
            engines.Add<DeepBeelineEngine>("Deep Beeline");

            cbPlayerOne.Items.Add("Human");
            cbPlayerTwo.Items.Add("Human");
            cbPlayerOne.SelectedIndex = 0;
            cbPlayerTwo.SelectedIndex = 0;
            foreach (string engine in engines.EngineNames)
            {
                cbPlayerOne.Items.Add(engine);
                cbPlayerTwo.Items.Add(engine);
            }

            ConfigureComputer();

            menuStrip1.Renderer = new MenuProfessionalRenderer(new MenuColorTable());
            toolStrip1.Renderer = new MenuProfessionalRenderer(new MenuColorTable());
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
            game.RegisterEngine(Player.One, engines.GetEngine(cbPlayerOne.Text));
            game.RegisterEngine(Player.Two, engines.GetEngine(cbPlayerTwo.Text));

            game.ComputerPlay();
        }

        private void selfPlayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selfPlayForm = new SelfPlayForm(engines.EngineNames);
            var dr = selfPlayForm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                autoPlay = totalAutoPlay = selfPlayForm.GamesToPlay;

                cbPlayerOne.Text = selfPlayForm.EngineOne;
                cbPlayerTwo.Text = selfPlayForm.EngineTwo;

                lblStatusBar.Text = "0/" + totalAutoPlay;
                progStatus.Value = 0;
                progStatus.Visible = true;

                StartNewGame();
            }
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

            if (totalAutoPlay > 0)
            {
                lblStatusBar.Text = Math.Min(totalAutoPlay - autoPlay, totalAutoPlay) + "/" + totalAutoPlay;
                progStatus.Value = (int)(100m * (totalAutoPlay - autoPlay) / totalAutoPlay);
            }

            if (autoPlay == 0)
            {
                lblStatusBar.Text = "Autoplay finished";
                progStatus.Visible = false;
            }
        }

        private void Tourney_OnTournamentStatus(TournamentStatus status)
        {
            lblStatusBar.Text = status.CompletedGames + "/" + status.TotalGames + " tournament games completed";
            progStatus.Value = (int)status.PercentageComplete;
        }

        private void Tourney_OnTournamentCompleted()
        {
            progStatus.Visible = false;
            newTournamentToolStripMenuItem.Enabled = true;

            lblStatusBar.Text = "Tournament finished";

            MessageBox.Show("Tournament complete! Round robin cross table saved to file.");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Designed by Simon Dorfman\r\nDeveloped by Scott Clayton", "Volcanoes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void newTournamentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new TournamentForm(engines.EngineNames);
            var dr = form.ShowDialog();
            if (dr == DialogResult.OK)
            {
                progStatus.Value = 0;
                progStatus.Visible = true;

                newTournamentToolStripMenuItem.Enabled = false;
                progStatus.Value = 0;
                progStatus.Visible = true;

                string date = DateTime.Now.ToString("yyyyMMddhhmmss");

                Tournament tourney = new Tournament(form.Rounds, "tourney-table-" + date + ".csv", "tourney-data-" + date + ".csv", engines, form.Engines);
                tourney.OnTournamentCompleted += Tourney_OnTournamentCompleted;
                tourney.OnTournamentStatus += Tourney_OnTournamentStatus;
                tourney.Start();
            }
        }
    }
}

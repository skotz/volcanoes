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
using System.Diagnostics;
using Volcano.Search;

namespace Volcano
{
    public partial class GameForm : Form
    {
        GameGraphics graphics;
        VolcanoGame game;
        EngineHelper engines;

        int totalAutoPlay = 0;
        int autoPlay = 0;

        List<int> transcript;
        int transcriptMove;

        EngineOutputForm outputForm;

        public GameForm()
        {
            InitializeComponent();
            
            graphics = new GameGraphics(gamePanel, GameGraphicsSettings.Default);
            game = new VolcanoGame();
            game.OnGameOver += Game_OnGameOver;
            game.OnMoveMade += Game_OnMoveMade;

            gameTimer.Start();

            engines = new EngineHelper();
            engines.Add<RandomEngine>("Random");
            //engines.Add<SkipTileEngine>("Tile Skipper");
            //engines.Add<KittyCornerEngine>("Kitty Corner");
            //engines.Add<BeeLineEngine>("Bee Line");
            engines.Add<DeepBeelineEngine>("Deep Beeline");
            engines.Add<BarricadeEngine>("Barricade");
            //engines.Add<MonteCarloBeelineEngine>("Monte Carlo Beeline 1");
            //engines.Add<MonteCarloTwoEngine>("Monte Carlo Beeline 2");
            engines.Add<MonteCarloBeelineThreeEngine>("Monte Carlo Beeline 3");
            engines.Add<MonteCarloBeelineFourEngine>("Monte Carlo Beeline 4");
            engines.Add<MonteCarloTreeSearchEngine>("Monte Carlo Tree Search");
            engines.Add<MonteCarloBarricadeEngine>("Monte Carlo Barricade");

            cbPlayerOne.Items.Add("Human");
            cbPlayerTwo.Items.Add("Human");
            cbPlayerOne.SelectedIndex = 0;
            cbPlayerTwo.SelectedIndex = 0;
            foreach (string engine in engines.EngineNames)
            {
                cbPlayerOne.Items.Add(engine);
                cbPlayerTwo.Items.Add(engine);
            }

            cbSeconds.SelectedIndex = 1;

            ConfigureComputer();

            menuStrip1.Renderer = new MenuProfessionalRenderer(new MenuColorTable());
            toolStrip1.Renderer = new MenuProfessionalRenderer(new MenuColorTable());
            toolStripContainer1.ContentPanel.RenderMode = ToolStripRenderMode.System;

            if (!string.IsNullOrEmpty(VolcanoGame.Settings.CustomSettingsFile))
            {
                lblStatusBar.Text = VolcanoGame.Settings.CustomSettingsFile + " loaded " + VolcanoGame.Settings.CustomSettingsTitle;
            }
        }

        private void Game_OnMoveMade(bool growthHappened)
        {
            if (transcriptMove == game.MoveHistory.Count - (growthHappened ? 3 : 2))
            {
                transcriptMove = game.MoveHistory.Count - 1;
            }

            lblTranscriptMove.Text = (transcriptMove + 1) + "/" + game.MoveHistory.Count;
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            Point mouse = gamePanel.PointToClient(Cursor.Position);
            graphics.Draw(game, mouse, transcriptMove, chkHighlightLastMove.Checked);
        }

        private void gamePanel_Click(object sender, EventArgs e)
        {
            bool reviewMode = game.MoveHistory.Count > 0 && transcriptMove != game.MoveHistory.Count - 1;
            if (game.CurrentState.State == GameState.InProgress && !game.Thinking && !reviewMode)
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
            game.SecondsPerEngineMove = int.Parse(cbSeconds.Text);
            ConfigureComputer();

            transcriptMove = 0;
            transcript = new List<int>();
            lblTranscriptMove.Text = "0/0";
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

        private void selfPlayToolStripMenuItem_Click_1(object sender, EventArgs e)
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

        private void Game_OnGameOver(Player winner, VictoryType type)
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

                Tournament tourney = new Tournament(form.Rounds, form.SecondsPerMove, "tourney-table-" + date + ".csv", "tourney-data-" + date + ".csv", engines, form.Engines);
                tourney.OnTournamentCompleted += Tourney_OnTournamentCompleted;
                tourney.OnTournamentStatus += Tourney_OnTournamentStatus;
                tourney.Start();
            }
        }

        private void exportRulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            VolcanoGame.Settings.Save("volcano.json");
            MessageBox.Show("Rules exported as volcano.json!\r\nEdit the settings and reload to play with custom rules.", "Volcanoes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void saveGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog1.FileName, game.GetTranscript(true));
            }
        }

        private void loadTranscriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (game.LoadTranscript(File.ReadAllText(openFileDialog1.FileName)))
                {
                    transcriptMove = game.MoveHistory.Count - 1;
                    lblTranscriptMove.Text = (transcriptMove + 1) + "/" + game.MoveHistory.Count;
                }
                else
                {
                    MessageBox.Show("Failed to load transcript!", "Volcanoes", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnNavBack_Click(object sender, EventArgs e)
        {
            transcriptMove = Math.Max(Math.Min(transcriptMove - 1, game.MoveHistory.Count - 1), 0);
            lblTranscriptMove.Text = (transcriptMove + 1) + "/" + game.MoveHistory.Count;
        }

        private void btnNavNext_Click(object sender, EventArgs e)
        {
            transcriptMove = Math.Max(Math.Min(transcriptMove + 1, game.MoveHistory.Count - 1), 0);
            lblTranscriptMove.Text = (transcriptMove + 1) + "/" + game.MoveHistory.Count;
        }

        private void btnNavStart_Click(object sender, EventArgs e)
        {
            transcriptMove = 0;
            lblTranscriptMove.Text = (transcriptMove + 1) + "/" + game.MoveHistory.Count;
        }

        private void btnNavEnd_Click(object sender, EventArgs e)
        {
            transcriptMove = game.MoveHistory.Count - 1;
            lblTranscriptMove.Text = (transcriptMove + 1) + "/" + game.MoveHistory.Count;
        }

        private void fromStringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (game.LoadTranscript(Clipboard.GetText()))
            {
                transcriptMove = game.MoveHistory.Count - 1;
                lblTranscriptMove.Text = (transcriptMove + 1) + "/" + game.MoveHistory.Count;
            }
            else
            {
                MessageBox.Show("Failed to load transcript!", "Volcanoes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void importRulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists("volcano.json"))
            {
                File.Move("volcano.json", "volcano.json." + DateTime.Now.ToString("yyyyMMddhhmmss") + ".bak");
                MessageBox.Show("Rules will be reset to their defaults when you restart.", "Volcanoes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.D)
            {
                dEBUGToolStripMenuItem.Visible = true;
            }
        }

        private void stressTestPathSearchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var runs = 10000000;
            var timer = Stopwatch.StartNew();
            var pathFinder = new PathFinder();
            var board = new Board();
            var random = new Random();

            for (int i = 0; i < runs; i++)
            {
                int index = random.Next(80);
                pathFinder.FindPath(board, index, Constants.Antipodes[index]);
            }

            decimal knps = (runs / 1000m) / (timer.ElapsedMilliseconds / 1000m);
            string result = runs.ToString("N0") + " searches / " + timer.ElapsedMilliseconds + " milliseconds = " + knps.ToString("N0") + " knps";

            using (StreamWriter w = new StreamWriter("debug-search.txt", true))
            {
                w.WriteLine(result);
            }

            MessageBox.Show(result);
        }

        private void stressTestEngineSearchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var runs = 10;
            var evals = 0L;
            var timer = Stopwatch.StartNew();
            var game = new VolcanoGame();
            game.LoadTranscript("5A 13A G 8B 16A G 2B 15A G 17B 20C G 10B 16D G 18A 7A G 16A 17A G 4B 12C G 11C 10C G 9B 11D G 9A 12B G 13B");
            var engine = new MonteCarloTreeSearchEngine();

            for (int i = 0; i < runs; i++)
            {
                var best = engine.GetBestMove(game.CurrentState, 1, new EngineCancellationToken(() => false));
                evals += best.Evaluations;
            }

            decimal knps = evals / (timer.ElapsedMilliseconds / 1000m);
            string result = evals.ToString("N0") + " positions / " + timer.ElapsedMilliseconds + " milliseconds = " + knps.ToString("N0") + " nps";

            using (StreamWriter w = new StreamWriter("debug-engine.txt", true))
            {
                w.WriteLine(result);
            }

            MessageBox.Show(result);
        }

        private void outputWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (outputForm != null)
            {
                outputForm.Close();
            }

            outputForm = new EngineOutputForm(game);
            outputForm.Show();
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!newTournamentToolStripMenuItem.Enabled)
            {
                // I keep closing the game when a tournament is playing...
                if (MessageBox.Show("Are you sure you want to end the tournament and exit?", "Volcanoes", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}

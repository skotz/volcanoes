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
using static System.Environment;

namespace Volcano
{
    public partial class GameForm : Form
    {
        GameGraphics graphics;
        VolcanoGame game;
        EngineHelper engines;
        GameGraphicsSettings settings;

        List<int> transcript;
        int transcriptMove;

        EngineOutputForm outputForm;

        string gameFolder = $"{Environment.GetFolderPath(SpecialFolder.MyDocuments)}\\My Games\\Volcanoes\\";

        public GameForm()
        {
            InitializeComponent();

            if (!Directory.Exists(gameFolder))
            {
                Directory.CreateDirectory(gameFolder);
            }

            settings = GameGraphicsSettings.LoadOrDefault(gameFolder + "graphics.json");
            chkShowTileLocations.Checked = settings.ShowTileNames;
            graphics = new GameGraphics(gamePanel, settings);
            game = new VolcanoGame();
            game.OnMoveMade += Game_OnMoveMade;

            gameTimer.Start();

            engines = new EngineHelper();
            engines.Add<BarricadeEngine>("Barricade");
            engines.Add<RandomEngine>("Random");
            engines.Add<DeepBeelineEngine>("Deep Beeline");
            engines.Add<MonteCarloBarricadeEngine>("Monte Carlo Barricade");
            engines.Add<MonteCarloBeelineRandParallelEngine>("Parallel MCTS Beeline");
            engines.Add<MonteCarloTreeSearchParallelEngine>("Parallel MCTS");
            engines.Add<MonteCarloBeelineThreeEngine>("Monte Carlo Beeline 3");
            engines.Add<MonteCarloBeelineFourEngine>("Monte Carlo Beeline 4");
            engines.Add<MonteCarloTreeSearchEngine>("Monte Carlo Tree Search");
            //engines.Add<MonteCarloBeelineParallelEngine>("Parallel MCTS Beeline Full");
            //engines.Add<MonteCarloBeelineParallelDeepEngine>("Parallel MCTS Beeline Sim");

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

            FileSystemWatcher graphicsSettingsFsw = new FileSystemWatcher(".", gameFolder + "graphics.json");
            graphicsSettingsFsw.EnableRaisingEvents = true;
            graphicsSettingsFsw.Changed += GraphicsSettingsFsw_Changed;
        }

        private void Game_OnMoveMade(bool growthHappened)
        {
            // If we were not in analysis mode previous to the last move, bump us up to the latest move
            if (transcriptMove == game.MoveHistory.Count - (growthHappened ? 2 : 1))
            {
                // If the last move was a growth move, then show it for a short period of time
                if (growthHappened)
                {
                    transcriptMove = game.MoveHistory.Count - 1;
                    growthMoveTimer.Start();
                }
                else
                {
                    transcriptMove = game.MoveHistory.Count;
                }
            }

            lblTranscriptMove.Text = transcriptMove + "/" + game.MoveHistory.Count;
        }

        private void growthMoveTimer_Tick(object sender, EventArgs e)
        {
            growthMoveTimer.Stop();
            transcriptMove = game.MoveHistory.Count;
            lblTranscriptMove.Text = transcriptMove + "/" + game.MoveHistory.Count;
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            Point mouse = gamePanel.PointToClient(Cursor.Position);
            graphics.Draw(game, mouse, transcriptMove, chkHighlightLastMove.Checked, displayHeatmap.Checked);
        }

        private void gamePanel_Click(object sender, EventArgs e)
        {
            Point mouse = gamePanel.PointToClient(Cursor.Position);
            if (whiteboardModeToolStripMenuItem.Checked)
            {
                try
                {
                    int tileIndex = graphics.GetBoardIndex(mouse);
                    if (game.CurrentState.Tiles[tileIndex] == 0)
                    {
                        game.CurrentState.Tiles[tileIndex] = 1;
                    }
                    else if (game.CurrentState.Tiles[tileIndex] > 0)
                    {
                        game.CurrentState.Tiles[tileIndex] = -1;
                    }
                    else
                    {
                        game.CurrentState.Tiles[tileIndex] = 0;
                    }
                }
                catch { }
            }
            else
            {
                bool reviewMode = game.MoveHistory.Count > 0 && transcriptMove != game.MoveHistory.Count;
                if (game.CurrentState.State == GameState.InProgress && !game.Thinking && !reviewMode)
                {
                    int tileIndex = graphics.GetBoardIndex(mouse);
                    game.MakeMove(tileIndex);
                }
            }
            graphics.Click(mouse);
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

        private void Tourney_OnTournamentStatus(TournamentStatus status)
        {
            lblStatusBar.Text = status.CompletedGames + "/" + status.TotalGames + " games completed";
            progStatus.Value = (int)status.PercentageComplete;
        }

        private void Tourney_OnTournamentCompleted()
        {
            progStatus.Visible = false;
            newTournamentToolStripMenuItem.Enabled = true;
            selfPlayToolStripMenuItem.Enabled = true;
            lblStatusBar.Text += " (finished)";
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Designed by Simon Dorfman\r\nDeveloped by Scott Clayton\r\nhttps://github.com/skotz/volcanoes", "Volcanoes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void selfPlayToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            var selfPlayForm = new SelfPlayForm(engines.EngineNames);
            var dr = selfPlayForm.ShowDialog();
            if (dr == DialogResult.OK)
            {
                progStatus.Value = 0;
                progStatus.Visible = true;
                newTournamentToolStripMenuItem.Enabled = false;
                selfPlayToolStripMenuItem.Enabled = false;

                string date = DateTime.Now.ToString("yyyyMMddhhmmss");

                int games = selfPlayForm.GamesToPlay;
                if (selfPlayForm.Engines.Count == 2)
                {
                    games /= 2;
                }

                Tournament tourney = new Tournament(games, selfPlayForm.SecondsPerMove, gameFolder + "selfplay-table-" + date + ".csv", gameFolder + "selfplay-data-" + date + ".csv", engines, selfPlayForm.Engines, selfPlayForm.Engines.Count == 1);
                tourney.OnTournamentCompleted += Tourney_OnTournamentCompleted;
                tourney.OnTournamentStatus += Tourney_OnTournamentStatus;
                tourney.Start();
            }
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
                selfPlayToolStripMenuItem.Enabled = false;

                string date = DateTime.Now.ToString("yyyyMMddhhmmss");

                Tournament tourney = new Tournament(form.Rounds, form.SecondsPerMove, gameFolder + "tourney-table-" + date + ".csv", gameFolder + "tourney-data-" + date + ".csv", engines, form.Engines, form.SelfPlay || form.Engines.Count == 1);
                tourney.OnTournamentCompleted += Tourney_OnTournamentCompleted;
                tourney.OnTournamentStatus += Tourney_OnTournamentStatus;
                tourney.Start();
            }
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
                    transcriptMove = game.MoveHistory.Count;
                    lblTranscriptMove.Text = transcriptMove + "/" + game.MoveHistory.Count;
                }
                else
                {
                    MessageBox.Show("Failed to load transcript!", "Volcanoes", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnNavBack_Click(object sender, EventArgs e)
        {
            NavBack();
        }

        private void NavBack()
        {
            transcriptMove = Math.Max(Math.Min(transcriptMove - 1, game.MoveHistory.Count), 0);
            lblTranscriptMove.Text = transcriptMove + "/" + game.MoveHistory.Count;
        }

        private void btnNavNext_Click(object sender, EventArgs e)
        {
            NavNext();
        }

        private void NavNext()
        {
            transcriptMove = Math.Max(Math.Min(transcriptMove + 1, game.MoveHistory.Count), 0);
            lblTranscriptMove.Text = transcriptMove + "/" + game.MoveHistory.Count;
        }

        private void btnNavStart_Click(object sender, EventArgs e)
        {
            NavFirst();
        }

        private void NavFirst()
        {
            transcriptMove = 0;
            lblTranscriptMove.Text = transcriptMove + "/" + game.MoveHistory.Count;
        }

        private void btnNavEnd_Click(object sender, EventArgs e)
        {
            NavLast();
        }

        private void NavLast()
        {
            transcriptMove = game.MoveHistory.Count;
            lblTranscriptMove.Text = transcriptMove + "/" + game.MoveHistory.Count;
        }

        private void fromStringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (game.LoadTranscript(Clipboard.GetText()))
            {
                transcriptMove = game.MoveHistory.Count;
                lblTranscriptMove.Text = transcriptMove + "/" + game.MoveHistory.Count;
            }
            else
            {
                MessageBox.Show("Failed to load transcript!", "Volcanoes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void exportRulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            VolcanoGame.Settings.Save(gameFolder + "volcano.json");
            MessageBox.Show("Rules exported as volcano.json!\r\nEdit the settings and reload to play with custom rules.", "Volcanoes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void importRulesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(gameFolder + "volcano.json"))
            {
                File.Move(gameFolder + "volcano.json", gameFolder + "volcano.json." + DateTime.Now.ToString("yyyyMMddhhmmss") + ".bak");
                MessageBox.Show("Rules will be reset to their defaults when you restart.", "Volcanoes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void exportThemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            graphics.GraphicsSettings.Save(gameFolder + "graphics.json");
            MessageBox.Show("Graphics exported as graphics.json!\r\nEdit the settings and reload to see your changes.", "Volcanoes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void resetThemeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(gameFolder + "graphics.json"))
            {
                File.Move(gameFolder + "graphics.json", gameFolder + "graphics.json." + DateTime.Now.ToString("yyyyMMddhhmmss") + ".bak");
                settings = GameGraphicsSettings.Default;
                graphics.GraphicsSettings = settings;
            }
        }

        private void GraphicsSettingsFsw_Changed(object sender, FileSystemEventArgs e)
        {
            if (File.Exists(gameFolder + "graphics.json"))
            {
                settings = GameGraphicsSettings.LoadOrDefault(gameFolder + "graphics.json");
                graphics.GraphicsSettings = settings;
            }
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.D)
            {
                dEBUGToolStripMenuItem.Visible = true;
            }
            //if (e.KeyCode == Keys.R)
            //{
            //    graphics.RotateBoard();
            //}
            if (e.KeyCode == Keys.Home)
            {
                NavFirst();
            }
            if (e.KeyCode == Keys.End)
            {
                NavLast();
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
            var sims = 0L;
            var timer = Stopwatch.StartNew();
            var game = new VolcanoGame();
            game.LoadTranscript("5A 13A G 8B 16A G 2B 15A G 17B 20C G 10B 16D G 18A 7A G 16A 17A G 4B 12C G 11C 10C G 9B 11D G 9A 12B G 13B");
            var engine = new MonteCarloTreeSearchEngine();

            for (int i = 0; i < runs; i++)
            {
                var best = engine.GetBestMove(game.CurrentState, 1, new EngineCancellationToken(() => false));
                evals += best.Evaluations;
                sims += best.Simulations;
            }

            decimal nps = evals / (timer.ElapsedMilliseconds / 1000m);
            decimal sps = sims / (timer.ElapsedMilliseconds / 1000m);
            string result = evals.ToString("N0") + " positions / " + sims.ToString("N0") + " simulations / " + timer.ElapsedMilliseconds + " milliseconds = " + nps.ToString("N0") + " nps / " + sps.ToString("N0") + " sps";

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
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Left)
            {
                NavBack();
            }
            if (keyData == Keys.Right)
            {
                NavNext();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void chkShowTileLocations_Click(object sender, EventArgs e)
        {
            settings.ShowTileNames = chkShowTileLocations.Checked;
        }
    }
}

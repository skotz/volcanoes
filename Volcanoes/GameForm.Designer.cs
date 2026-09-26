namespace Volcano
{
    partial class GameForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameForm));
            gamePanel = new System.Windows.Forms.Panel();
            gameTimer = new System.Windows.Forms.Timer(components);
            toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            progStatus = new System.Windows.Forms.ToolStripProgressBar();
            lblStatusBar = new System.Windows.Forms.ToolStripStatusLabel();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            newGameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            saveGameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadTranscriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            fromFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            fromStringToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            engineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            selfPlayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            outputWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tournamentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            newTournamentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            dEBUGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            stressTestPathSearchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            stressTestEngineSearchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            loadCGStringFromClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            exportRulesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            resetRulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            exportThemeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            resetThemeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            whiteboardModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            generateOpeningBooksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            trainQLearningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            canonicalizeBoardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            btnNewGame = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            cbPlayerOne = new System.Windows.Forms.ToolStripComboBox();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            cbPlayerTwo = new System.Windows.Forms.ToolStripComboBox();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            cbSeconds = new System.Windows.Forms.ToolStripComboBox();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            chkHighlightLastMove = new System.Windows.Forms.ToolStripButton();
            chkShowTileLocations = new System.Windows.Forms.ToolStripButton();
            displayHeatmap = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            btnNavStart = new System.Windows.Forms.ToolStripButton();
            btnNavBack = new System.Windows.Forms.ToolStripButton();
            lblTranscriptMove = new System.Windows.Forms.ToolStripLabel();
            btnNavNext = new System.Windows.Forms.ToolStripButton();
            btnNavEnd = new System.Windows.Forms.ToolStripButton();
            saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            growthMoveTimer = new System.Windows.Forms.Timer(components);
            toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            toolStripContainer1.ContentPanel.SuspendLayout();
            toolStripContainer1.TopToolStripPanel.SuspendLayout();
            toolStripContainer1.SuspendLayout();
            statusStrip1.SuspendLayout();
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // gamePanel
            // 
            gamePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            gamePanel.Location = new System.Drawing.Point(0, 0);
            gamePanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            gamePanel.Name = "gamePanel";
            gamePanel.Size = new System.Drawing.Size(1227, 621);
            gamePanel.TabIndex = 0;
            gamePanel.Click += gamePanel_Click;
            // 
            // gameTimer
            // 
            gameTimer.Interval = 30;
            gameTimer.Tick += gameTimer_Tick;
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            toolStripContainer1.BottomToolStripPanel.Controls.Add(statusStrip1);
            // 
            // toolStripContainer1.ContentPanel
            // 
            toolStripContainer1.ContentPanel.Controls.Add(gamePanel);
            toolStripContainer1.ContentPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(1227, 621);
            toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            toolStripContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            toolStripContainer1.Name = "toolStripContainer1";
            toolStripContainer1.Size = new System.Drawing.Size(1227, 692);
            toolStripContainer1.TabIndex = 8;
            toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            toolStripContainer1.TopToolStripPanel.Controls.Add(menuStrip1);
            toolStripContainer1.TopToolStripPanel.Controls.Add(toolStrip1);
            // 
            // statusStrip1
            // 
            statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { progStatus, lblStatusBar });
            statusStrip1.Location = new System.Drawing.Point(0, 0);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(1227, 22);
            statusStrip1.TabIndex = 0;
            // 
            // progStatus
            // 
            progStatus.Name = "progStatus";
            progStatus.Size = new System.Drawing.Size(100, 16);
            progStatus.Visible = false;
            // 
            // lblStatusBar
            // 
            lblStatusBar.Name = "lblStatusBar";
            lblStatusBar.Size = new System.Drawing.Size(39, 17);
            lblStatusBar.Text = "Ready";
            // 
            // menuStrip1
            // 
            menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, engineToolStripMenuItem, tournamentToolStripMenuItem, aboutToolStripMenuItem, dEBUGToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            menuStrip1.Size = new System.Drawing.Size(1227, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { newGameToolStripMenuItem, toolStripSeparator5, saveGameToolStripMenuItem, loadTranscriptToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            fileToolStripMenuItem.Text = "&Game";
            // 
            // newGameToolStripMenuItem
            // 
            newGameToolStripMenuItem.Image = (System.Drawing.Image)resources.GetObject("newGameToolStripMenuItem.Image");
            newGameToolStripMenuItem.Name = "newGameToolStripMenuItem";
            newGameToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N;
            newGameToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            newGameToolStripMenuItem.Text = "&New Game";
            newGameToolStripMenuItem.Click += btnNewGame_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(189, 6);
            // 
            // saveGameToolStripMenuItem
            // 
            saveGameToolStripMenuItem.Name = "saveGameToolStripMenuItem";
            saveGameToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
            saveGameToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            saveGameToolStripMenuItem.Text = "&Save Transcript";
            saveGameToolStripMenuItem.Click += saveGameToolStripMenuItem_Click;
            // 
            // loadTranscriptToolStripMenuItem
            // 
            loadTranscriptToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { fromFileToolStripMenuItem, fromStringToolStripMenuItem });
            loadTranscriptToolStripMenuItem.Name = "loadTranscriptToolStripMenuItem";
            loadTranscriptToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            loadTranscriptToolStripMenuItem.Text = "&Load Transcript";
            // 
            // fromFileToolStripMenuItem
            // 
            fromFileToolStripMenuItem.Name = "fromFileToolStripMenuItem";
            fromFileToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
            fromFileToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            fromFileToolStripMenuItem.Text = "From &File";
            fromFileToolStripMenuItem.Click += loadTranscriptToolStripMenuItem_Click;
            // 
            // fromStringToolStripMenuItem
            // 
            fromStringToolStripMenuItem.Name = "fromStringToolStripMenuItem";
            fromStringToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V;
            fromStringToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            fromStringToolStripMenuItem.Text = "From &Clipboard";
            fromStringToolStripMenuItem.Click += fromStringToolStripMenuItem_Click;
            // 
            // engineToolStripMenuItem
            // 
            engineToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { selfPlayToolStripMenuItem, outputWindowToolStripMenuItem });
            engineToolStripMenuItem.Name = "engineToolStripMenuItem";
            engineToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            engineToolStripMenuItem.Text = "&Engine";
            // 
            // selfPlayToolStripMenuItem
            // 
            selfPlayToolStripMenuItem.Name = "selfPlayToolStripMenuItem";
            selfPlayToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            selfPlayToolStripMenuItem.Text = "Self &Play";
            selfPlayToolStripMenuItem.Click += selfPlayToolStripMenuItem_Click_1;
            // 
            // outputWindowToolStripMenuItem
            // 
            outputWindowToolStripMenuItem.Name = "outputWindowToolStripMenuItem";
            outputWindowToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            outputWindowToolStripMenuItem.Text = "Output Window";
            outputWindowToolStripMenuItem.Click += outputWindowToolStripMenuItem_Click;
            // 
            // tournamentToolStripMenuItem
            // 
            tournamentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { newTournamentToolStripMenuItem });
            tournamentToolStripMenuItem.Name = "tournamentToolStripMenuItem";
            tournamentToolStripMenuItem.Size = new System.Drawing.Size(83, 20);
            tournamentToolStripMenuItem.Text = "&Tournament";
            // 
            // newTournamentToolStripMenuItem
            // 
            newTournamentToolStripMenuItem.Name = "newTournamentToolStripMenuItem";
            newTournamentToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            newTournamentToolStripMenuItem.Text = "&New Tournament";
            newTournamentToolStripMenuItem.Click += newTournamentToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            aboutToolStripMenuItem.Text = "&About";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // dEBUGToolStripMenuItem
            // 
            dEBUGToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { stressTestPathSearchToolStripMenuItem, stressTestEngineSearchToolStripMenuItem, toolStripSeparator11, loadCGStringFromClipboardToolStripMenuItem, toolStripSeparator7, exportRulesToolStripMenuItem1, resetRulesToolStripMenuItem, toolStripSeparator8, exportThemeToolStripMenuItem, resetThemeToolStripMenuItem, toolStripSeparator9, whiteboardModeToolStripMenuItem, toolStripSeparator10, generateOpeningBooksToolStripMenuItem, toolStripSeparator12, trainQLearningToolStripMenuItem, toolStripSeparator13, canonicalizeBoardToolStripMenuItem });
            dEBUGToolStripMenuItem.Name = "dEBUGToolStripMenuItem";
            dEBUGToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            dEBUGToolStripMenuItem.Text = "DEBUG";
            dEBUGToolStripMenuItem.Visible = false;
            // 
            // stressTestPathSearchToolStripMenuItem
            // 
            stressTestPathSearchToolStripMenuItem.Name = "stressTestPathSearchToolStripMenuItem";
            stressTestPathSearchToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            stressTestPathSearchToolStripMenuItem.Text = "Stress Test Path Search";
            stressTestPathSearchToolStripMenuItem.Click += stressTestPathSearchToolStripMenuItem_Click;
            // 
            // stressTestEngineSearchToolStripMenuItem
            // 
            stressTestEngineSearchToolStripMenuItem.Name = "stressTestEngineSearchToolStripMenuItem";
            stressTestEngineSearchToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            stressTestEngineSearchToolStripMenuItem.Text = "Stress Test Engine Search";
            stressTestEngineSearchToolStripMenuItem.Click += stressTestEngineSearchToolStripMenuItem_Click;
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new System.Drawing.Size(236, 6);
            // 
            // loadCGStringFromClipboardToolStripMenuItem
            // 
            loadCGStringFromClipboardToolStripMenuItem.Name = "loadCGStringFromClipboardToolStripMenuItem";
            loadCGStringFromClipboardToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            loadCGStringFromClipboardToolStripMenuItem.Text = "Load CG String From Clipboard";
            loadCGStringFromClipboardToolStripMenuItem.Click += loadCGStringFromClipboardToolStripMenuItem_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new System.Drawing.Size(236, 6);
            // 
            // exportRulesToolStripMenuItem1
            // 
            exportRulesToolStripMenuItem1.Name = "exportRulesToolStripMenuItem1";
            exportRulesToolStripMenuItem1.Size = new System.Drawing.Size(239, 22);
            exportRulesToolStripMenuItem1.Text = "Export Rules";
            exportRulesToolStripMenuItem1.Click += exportRulesToolStripMenuItem_Click;
            // 
            // resetRulesToolStripMenuItem
            // 
            resetRulesToolStripMenuItem.Name = "resetRulesToolStripMenuItem";
            resetRulesToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            resetRulesToolStripMenuItem.Text = "Reset Rules";
            resetRulesToolStripMenuItem.Click += importRulesToolStripMenuItem_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new System.Drawing.Size(236, 6);
            // 
            // exportThemeToolStripMenuItem
            // 
            exportThemeToolStripMenuItem.Name = "exportThemeToolStripMenuItem";
            exportThemeToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            exportThemeToolStripMenuItem.Text = "Export Theme";
            exportThemeToolStripMenuItem.Click += exportThemeToolStripMenuItem_Click;
            // 
            // resetThemeToolStripMenuItem
            // 
            resetThemeToolStripMenuItem.Name = "resetThemeToolStripMenuItem";
            resetThemeToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            resetThemeToolStripMenuItem.Text = "Reset Theme";
            resetThemeToolStripMenuItem.Click += resetThemeToolStripMenuItem_Click;
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new System.Drawing.Size(236, 6);
            // 
            // whiteboardModeToolStripMenuItem
            // 
            whiteboardModeToolStripMenuItem.CheckOnClick = true;
            whiteboardModeToolStripMenuItem.Name = "whiteboardModeToolStripMenuItem";
            whiteboardModeToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            whiteboardModeToolStripMenuItem.Text = "Whiteboard Mode";
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new System.Drawing.Size(236, 6);
            // 
            // generateOpeningBooksToolStripMenuItem
            // 
            generateOpeningBooksToolStripMenuItem.Name = "generateOpeningBooksToolStripMenuItem";
            generateOpeningBooksToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            generateOpeningBooksToolStripMenuItem.Text = "Generate Opening Books";
            generateOpeningBooksToolStripMenuItem.Click += generateOpeningBooksToolStripMenuItem_Click;
            // 
            // toolStripSeparator12
            // 
            toolStripSeparator12.Name = "toolStripSeparator12";
            toolStripSeparator12.Size = new System.Drawing.Size(236, 6);
            // 
            // trainQLearningToolStripMenuItem
            // 
            trainQLearningToolStripMenuItem.Name = "trainQLearningToolStripMenuItem";
            trainQLearningToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            trainQLearningToolStripMenuItem.Text = "Train AlphaZero";
            trainQLearningToolStripMenuItem.Click += trainQLearningToolStripMenuItem_Click;
            // 
            // toolStripSeparator13
            // 
            toolStripSeparator13.Name = "toolStripSeparator13";
            toolStripSeparator13.Size = new System.Drawing.Size(236, 6);
            // 
            // canonicalizeBoardToolStripMenuItem
            // 
            canonicalizeBoardToolStripMenuItem.Name = "canonicalizeBoardToolStripMenuItem";
            canonicalizeBoardToolStripMenuItem.Size = new System.Drawing.Size(239, 22);
            canonicalizeBoardToolStripMenuItem.Text = "Canonicalize Board";
            canonicalizeBoardToolStripMenuItem.Click += canonicalizeBoardToolStripMenuItem_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.BackColor = System.Drawing.SystemColors.Control;
            toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnNewGame, toolStripSeparator2, toolStripLabel1, cbPlayerOne, toolStripSeparator1, toolStripLabel2, cbPlayerTwo, toolStripSeparator4, toolStripLabel3, cbSeconds, toolStripSeparator3, chkHighlightLastMove, chkShowTileLocations, displayHeatmap, toolStripSeparator6, btnNavStart, btnNavBack, lblTranscriptMove, btnNavNext, btnNavEnd });
            toolStrip1.Location = new System.Drawing.Point(0, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStrip1.Size = new System.Drawing.Size(1227, 25);
            toolStrip1.Stretch = true;
            toolStrip1.TabIndex = 1;
            // 
            // btnNewGame
            // 
            btnNewGame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnNewGame.Image = (System.Drawing.Image)resources.GetObject("btnNewGame.Image");
            btnNewGame.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnNewGame.Name = "btnNewGame";
            btnNewGame.Size = new System.Drawing.Size(23, 22);
            btnNewGame.Text = "New Game";
            btnNewGame.Click += btnNewGame_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new System.Drawing.Size(64, 22);
            toolStripLabel1.Text = "Player One";
            // 
            // cbPlayerOne
            // 
            cbPlayerOne.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbPlayerOne.Name = "cbPlayerOne";
            cbPlayerOne.Size = new System.Drawing.Size(165, 25);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel2
            // 
            toolStripLabel2.Name = "toolStripLabel2";
            toolStripLabel2.Size = new System.Drawing.Size(63, 22);
            toolStripLabel2.Text = "Player Two";
            // 
            // cbPlayerTwo
            // 
            cbPlayerTwo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbPlayerTwo.Name = "cbPlayerTwo";
            cbPlayerTwo.Size = new System.Drawing.Size(165, 25);
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel3
            // 
            toolStripLabel3.Name = "toolStripLabel3";
            toolStripLabel3.Size = new System.Drawing.Size(104, 22);
            toolStripLabel3.Text = "Seconds Per Move";
            // 
            // cbSeconds
            // 
            cbSeconds.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbSeconds.Items.AddRange(new object[] { "2", "5", "10", "20", "30", "60", "120" });
            cbSeconds.Name = "cbSeconds";
            cbSeconds.Size = new System.Drawing.Size(75, 25);
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // chkHighlightLastMove
            // 
            chkHighlightLastMove.CheckOnClick = true;
            chkHighlightLastMove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            chkHighlightLastMove.Image = (System.Drawing.Image)resources.GetObject("chkHighlightLastMove.Image");
            chkHighlightLastMove.ImageTransparentColor = System.Drawing.Color.Magenta;
            chkHighlightLastMove.Name = "chkHighlightLastMove";
            chkHighlightLastMove.Size = new System.Drawing.Size(23, 22);
            chkHighlightLastMove.Text = "Highlight Last Move";
            // 
            // chkShowTileLocations
            // 
            chkShowTileLocations.CheckOnClick = true;
            chkShowTileLocations.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            chkShowTileLocations.Image = (System.Drawing.Image)resources.GetObject("chkShowTileLocations.Image");
            chkShowTileLocations.ImageTransparentColor = System.Drawing.Color.Magenta;
            chkShowTileLocations.Name = "chkShowTileLocations";
            chkShowTileLocations.Size = new System.Drawing.Size(23, 22);
            chkShowTileLocations.Text = "Show Coordinate Labels";
            chkShowTileLocations.Click += chkShowTileLocations_Click;
            // 
            // displayHeatmap
            // 
            displayHeatmap.CheckOnClick = true;
            displayHeatmap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            displayHeatmap.Image = (System.Drawing.Image)resources.GetObject("displayHeatmap.Image");
            displayHeatmap.ImageTransparentColor = System.Drawing.Color.Magenta;
            displayHeatmap.Name = "displayHeatmap";
            displayHeatmap.Size = new System.Drawing.Size(23, 22);
            displayHeatmap.Text = "Display Engine Analysis Overlay";
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
            // 
            // btnNavStart
            // 
            btnNavStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnNavStart.Image = (System.Drawing.Image)resources.GetObject("btnNavStart.Image");
            btnNavStart.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnNavStart.Name = "btnNavStart";
            btnNavStart.Size = new System.Drawing.Size(23, 22);
            btnNavStart.Text = "First";
            btnNavStart.Click += btnNavStart_Click;
            // 
            // btnNavBack
            // 
            btnNavBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnNavBack.Image = (System.Drawing.Image)resources.GetObject("btnNavBack.Image");
            btnNavBack.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnNavBack.Name = "btnNavBack";
            btnNavBack.Size = new System.Drawing.Size(23, 22);
            btnNavBack.Text = "Previous";
            btnNavBack.Click += btnNavBack_Click;
            // 
            // lblTranscriptMove
            // 
            lblTranscriptMove.Name = "lblTranscriptMove";
            lblTranscriptMove.Size = new System.Drawing.Size(24, 22);
            lblTranscriptMove.Text = "0/0";
            // 
            // btnNavNext
            // 
            btnNavNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnNavNext.Image = (System.Drawing.Image)resources.GetObject("btnNavNext.Image");
            btnNavNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnNavNext.Name = "btnNavNext";
            btnNavNext.Size = new System.Drawing.Size(23, 22);
            btnNavNext.Text = "Next";
            btnNavNext.Click += btnNavNext_Click;
            // 
            // btnNavEnd
            // 
            btnNavEnd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            btnNavEnd.Image = (System.Drawing.Image)resources.GetObject("btnNavEnd.Image");
            btnNavEnd.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnNavEnd.Name = "btnNavEnd";
            btnNavEnd.Size = new System.Drawing.Size(23, 22);
            btnNavEnd.Text = "Last";
            btnNavEnd.Click += btnNavEnd_Click;
            // 
            // saveFileDialog1
            // 
            saveFileDialog1.DefaultExt = "*.vgt";
            saveFileDialog1.Filter = "Volcanoes Game Transcript|*.vgt";
            // 
            // openFileDialog1
            // 
            openFileDialog1.Filter = "Volcanoes Game Transcript|*.vgt";
            // 
            // growthMoveTimer
            // 
            growthMoveTimer.Interval = 1000;
            growthMoveTimer.Tick += growthMoveTimer_Tick;
            // 
            // GameForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1227, 692);
            Controls.Add(toolStripContainer1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MainMenuStrip = menuStrip1;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "GameForm";
            Text = "Volcanoes";
            FormClosing += GameForm_FormClosing;
            KeyDown += GameForm_KeyDown;
            toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            toolStripContainer1.BottomToolStripPanel.PerformLayout();
            toolStripContainer1.ContentPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.PerformLayout();
            toolStripContainer1.ResumeLayout(false);
            toolStripContainer1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel gamePanel;
        private System.Windows.Forms.Timer gameTimer;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusBar;
        private System.Windows.Forms.ToolStripProgressBar progStatus;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox cbPlayerOne;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripComboBox cbPlayerTwo;
        private System.Windows.Forms.ToolStripMenuItem newGameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tournamentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newTournamentToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton btnNewGame;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton btnNavStart;
        private System.Windows.Forms.ToolStripButton btnNavBack;
        private System.Windows.Forms.ToolStripButton btnNavNext;
        private System.Windows.Forms.ToolStripButton btnNavEnd;
        private System.Windows.Forms.ToolStripMenuItem saveGameToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem loadTranscriptToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripLabel lblTranscriptMove;
        private System.Windows.Forms.ToolStripMenuItem fromFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromStringToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton chkHighlightLastMove;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem dEBUGToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stressTestPathSearchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stressTestEngineSearchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem engineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selfPlayToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem outputWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem exportRulesToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem resetRulesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripComboBox cbSeconds;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem exportThemeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetThemeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripButton chkShowTileLocations;
        private System.Windows.Forms.Timer growthMoveTimer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem whiteboardModeToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton displayHeatmap;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem generateOpeningBooksToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem loadCGStringFromClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem trainQLearningToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripMenuItem canonicalizeBoardToolStripMenuItem;
    }
}


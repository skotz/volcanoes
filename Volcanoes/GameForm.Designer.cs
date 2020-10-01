﻿namespace Volcano
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameForm));
            this.gamePanel = new System.Windows.Forms.Panel();
            this.gameTimer = new System.Windows.Forms.Timer(this.components);
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.progStatus = new System.Windows.Forms.ToolStripProgressBar();
            this.lblStatusBar = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newGameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.saveGameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadTranscriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fromFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fromStringToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.engineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selfPlayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.outputWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tournamentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newTournamentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dEBUGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stressTestPathSearchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stressTestEngineSearchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.exportRulesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.resetRulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.exportThemeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetThemeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnNewGame = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.cbPlayerOne = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.cbPlayerTwo = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.cbSeconds = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.chkHighlightLastMove = new System.Windows.Forms.ToolStripButton();
            this.chkShowTileLocations = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.btnNavStart = new System.Windows.Forms.ToolStripButton();
            this.btnNavBack = new System.Windows.Forms.ToolStripButton();
            this.lblTranscriptMove = new System.Windows.Forms.ToolStripLabel();
            this.btnNavNext = new System.Windows.Forms.ToolStripButton();
            this.btnNavEnd = new System.Windows.Forms.ToolStripButton();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.growthMoveTimer = new System.Windows.Forms.Timer(this.components);
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.whiteboardModeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gamePanel
            // 
            this.gamePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gamePanel.Location = new System.Drawing.Point(0, 0);
            this.gamePanel.Name = "gamePanel";
            this.gamePanel.Size = new System.Drawing.Size(1052, 529);
            this.gamePanel.TabIndex = 0;
            this.gamePanel.Click += new System.EventHandler(this.gamePanel_Click);
            // 
            // gameTimer
            // 
            this.gameTimer.Interval = 30;
            this.gameTimer.Tick += new System.EventHandler(this.gameTimer_Tick);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusStrip1);
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.gamePanel);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(1052, 529);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(1052, 600);
            this.toolStripContainer1.TabIndex = 8;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.menuStrip1);
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.progStatus,
            this.lblStatusBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 0);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1052, 22);
            this.statusStrip1.TabIndex = 0;
            // 
            // progStatus
            // 
            this.progStatus.Name = "progStatus";
            this.progStatus.Size = new System.Drawing.Size(100, 16);
            this.progStatus.Visible = false;
            // 
            // lblStatusBar
            // 
            this.lblStatusBar.Name = "lblStatusBar";
            this.lblStatusBar.Size = new System.Drawing.Size(39, 17);
            this.lblStatusBar.Text = "Ready";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.engineToolStripMenuItem,
            this.tournamentToolStripMenuItem,
            this.aboutToolStripMenuItem,
            this.dEBUGToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.menuStrip1.Size = new System.Drawing.Size(1052, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newGameToolStripMenuItem,
            this.toolStripSeparator5,
            this.saveGameToolStripMenuItem,
            this.loadTranscriptToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.fileToolStripMenuItem.Text = "&Game";
            // 
            // newGameToolStripMenuItem
            // 
            this.newGameToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newGameToolStripMenuItem.Image")));
            this.newGameToolStripMenuItem.Name = "newGameToolStripMenuItem";
            this.newGameToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newGameToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.newGameToolStripMenuItem.Text = "&New Game";
            this.newGameToolStripMenuItem.Click += new System.EventHandler(this.btnNewGame_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(189, 6);
            // 
            // saveGameToolStripMenuItem
            // 
            this.saveGameToolStripMenuItem.Name = "saveGameToolStripMenuItem";
            this.saveGameToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveGameToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.saveGameToolStripMenuItem.Text = "&Save Transcript";
            this.saveGameToolStripMenuItem.Click += new System.EventHandler(this.saveGameToolStripMenuItem_Click);
            // 
            // loadTranscriptToolStripMenuItem
            // 
            this.loadTranscriptToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fromFileToolStripMenuItem,
            this.fromStringToolStripMenuItem});
            this.loadTranscriptToolStripMenuItem.Name = "loadTranscriptToolStripMenuItem";
            this.loadTranscriptToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.loadTranscriptToolStripMenuItem.Text = "&Load Transcript";
            // 
            // fromFileToolStripMenuItem
            // 
            this.fromFileToolStripMenuItem.Name = "fromFileToolStripMenuItem";
            this.fromFileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.fromFileToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.fromFileToolStripMenuItem.Text = "From &File";
            this.fromFileToolStripMenuItem.Click += new System.EventHandler(this.loadTranscriptToolStripMenuItem_Click);
            // 
            // fromStringToolStripMenuItem
            // 
            this.fromStringToolStripMenuItem.Name = "fromStringToolStripMenuItem";
            this.fromStringToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.fromStringToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.fromStringToolStripMenuItem.Text = "From &Clipboard";
            this.fromStringToolStripMenuItem.Click += new System.EventHandler(this.fromStringToolStripMenuItem_Click);
            // 
            // engineToolStripMenuItem
            // 
            this.engineToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selfPlayToolStripMenuItem,
            this.outputWindowToolStripMenuItem});
            this.engineToolStripMenuItem.Name = "engineToolStripMenuItem";
            this.engineToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            this.engineToolStripMenuItem.Text = "&Engine";
            // 
            // selfPlayToolStripMenuItem
            // 
            this.selfPlayToolStripMenuItem.Name = "selfPlayToolStripMenuItem";
            this.selfPlayToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.selfPlayToolStripMenuItem.Text = "Self &Play";
            this.selfPlayToolStripMenuItem.Click += new System.EventHandler(this.selfPlayToolStripMenuItem_Click_1);
            // 
            // outputWindowToolStripMenuItem
            // 
            this.outputWindowToolStripMenuItem.Name = "outputWindowToolStripMenuItem";
            this.outputWindowToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.outputWindowToolStripMenuItem.Text = "Output Window";
            this.outputWindowToolStripMenuItem.Click += new System.EventHandler(this.outputWindowToolStripMenuItem_Click);
            // 
            // tournamentToolStripMenuItem
            // 
            this.tournamentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newTournamentToolStripMenuItem});
            this.tournamentToolStripMenuItem.Name = "tournamentToolStripMenuItem";
            this.tournamentToolStripMenuItem.Size = new System.Drawing.Size(83, 20);
            this.tournamentToolStripMenuItem.Text = "&Tournament";
            // 
            // newTournamentToolStripMenuItem
            // 
            this.newTournamentToolStripMenuItem.Name = "newTournamentToolStripMenuItem";
            this.newTournamentToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.newTournamentToolStripMenuItem.Text = "&New Tournament";
            this.newTournamentToolStripMenuItem.Click += new System.EventHandler(this.newTournamentToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // dEBUGToolStripMenuItem
            // 
            this.dEBUGToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stressTestPathSearchToolStripMenuItem,
            this.stressTestEngineSearchToolStripMenuItem,
            this.toolStripSeparator7,
            this.exportRulesToolStripMenuItem1,
            this.resetRulesToolStripMenuItem,
            this.toolStripSeparator8,
            this.exportThemeToolStripMenuItem,
            this.resetThemeToolStripMenuItem,
            this.toolStripSeparator9,
            this.whiteboardModeToolStripMenuItem});
            this.dEBUGToolStripMenuItem.Name = "dEBUGToolStripMenuItem";
            this.dEBUGToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.dEBUGToolStripMenuItem.Text = "DEBUG";
            this.dEBUGToolStripMenuItem.Visible = false;
            // 
            // stressTestPathSearchToolStripMenuItem
            // 
            this.stressTestPathSearchToolStripMenuItem.Name = "stressTestPathSearchToolStripMenuItem";
            this.stressTestPathSearchToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.stressTestPathSearchToolStripMenuItem.Text = "Stress Test Path Search";
            this.stressTestPathSearchToolStripMenuItem.Click += new System.EventHandler(this.stressTestPathSearchToolStripMenuItem_Click);
            // 
            // stressTestEngineSearchToolStripMenuItem
            // 
            this.stressTestEngineSearchToolStripMenuItem.Name = "stressTestEngineSearchToolStripMenuItem";
            this.stressTestEngineSearchToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.stressTestEngineSearchToolStripMenuItem.Text = "Stress Test Engine Search";
            this.stressTestEngineSearchToolStripMenuItem.Click += new System.EventHandler(this.stressTestEngineSearchToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(201, 6);
            // 
            // exportRulesToolStripMenuItem1
            // 
            this.exportRulesToolStripMenuItem1.Name = "exportRulesToolStripMenuItem1";
            this.exportRulesToolStripMenuItem1.Size = new System.Drawing.Size(204, 22);
            this.exportRulesToolStripMenuItem1.Text = "Export Rules";
            this.exportRulesToolStripMenuItem1.Click += new System.EventHandler(this.exportRulesToolStripMenuItem_Click);
            // 
            // resetRulesToolStripMenuItem
            // 
            this.resetRulesToolStripMenuItem.Name = "resetRulesToolStripMenuItem";
            this.resetRulesToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.resetRulesToolStripMenuItem.Text = "Reset Rules";
            this.resetRulesToolStripMenuItem.Click += new System.EventHandler(this.importRulesToolStripMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(201, 6);
            // 
            // exportThemeToolStripMenuItem
            // 
            this.exportThemeToolStripMenuItem.Name = "exportThemeToolStripMenuItem";
            this.exportThemeToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.exportThemeToolStripMenuItem.Text = "Export Theme";
            this.exportThemeToolStripMenuItem.Click += new System.EventHandler(this.exportThemeToolStripMenuItem_Click);
            // 
            // resetThemeToolStripMenuItem
            // 
            this.resetThemeToolStripMenuItem.Name = "resetThemeToolStripMenuItem";
            this.resetThemeToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.resetThemeToolStripMenuItem.Text = "Reset Theme";
            this.resetThemeToolStripMenuItem.Click += new System.EventHandler(this.resetThemeToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnNewGame,
            this.toolStripSeparator2,
            this.toolStripLabel1,
            this.cbPlayerOne,
            this.toolStripSeparator1,
            this.toolStripLabel2,
            this.cbPlayerTwo,
            this.toolStripSeparator4,
            this.toolStripLabel3,
            this.cbSeconds,
            this.toolStripSeparator3,
            this.chkHighlightLastMove,
            this.chkShowTileLocations,
            this.toolStripSeparator6,
            this.btnNavStart,
            this.btnNavBack,
            this.lblTranscriptMove,
            this.btnNavNext,
            this.btnNavEnd});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(1052, 25);
            this.toolStrip1.Stretch = true;
            this.toolStrip1.TabIndex = 1;
            // 
            // btnNewGame
            // 
            this.btnNewGame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNewGame.Image = ((System.Drawing.Image)(resources.GetObject("btnNewGame.Image")));
            this.btnNewGame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNewGame.Name = "btnNewGame";
            this.btnNewGame.Size = new System.Drawing.Size(23, 22);
            this.btnNewGame.Text = "New Game";
            this.btnNewGame.Click += new System.EventHandler(this.btnNewGame_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(64, 22);
            this.toolStripLabel1.Text = "Player One";
            // 
            // cbPlayerOne
            // 
            this.cbPlayerOne.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPlayerOne.Name = "cbPlayerOne";
            this.cbPlayerOne.Size = new System.Drawing.Size(165, 25);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(63, 22);
            this.toolStripLabel2.Text = "Player Two";
            // 
            // cbPlayerTwo
            // 
            this.cbPlayerTwo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPlayerTwo.Name = "cbPlayerTwo";
            this.cbPlayerTwo.Size = new System.Drawing.Size(165, 25);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(104, 22);
            this.toolStripLabel3.Text = "Seconds Per Move";
            // 
            // cbSeconds
            // 
            this.cbSeconds.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSeconds.Items.AddRange(new object[] {
            "2",
            "5",
            "10",
            "20",
            "30",
            "60",
            "120"});
            this.cbSeconds.Name = "cbSeconds";
            this.cbSeconds.Size = new System.Drawing.Size(75, 25);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // chkHighlightLastMove
            // 
            this.chkHighlightLastMove.CheckOnClick = true;
            this.chkHighlightLastMove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.chkHighlightLastMove.Image = ((System.Drawing.Image)(resources.GetObject("chkHighlightLastMove.Image")));
            this.chkHighlightLastMove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.chkHighlightLastMove.Name = "chkHighlightLastMove";
            this.chkHighlightLastMove.Size = new System.Drawing.Size(23, 22);
            this.chkHighlightLastMove.Text = "Highlight Last Move";
            // 
            // chkShowTileLocations
            // 
            this.chkShowTileLocations.CheckOnClick = true;
            this.chkShowTileLocations.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.chkShowTileLocations.Image = ((System.Drawing.Image)(resources.GetObject("chkShowTileLocations.Image")));
            this.chkShowTileLocations.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.chkShowTileLocations.Name = "chkShowTileLocations";
            this.chkShowTileLocations.Size = new System.Drawing.Size(23, 22);
            this.chkShowTileLocations.Text = "Show Coordinate Labels";
            this.chkShowTileLocations.Click += new System.EventHandler(this.chkShowTileLocations_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
            // 
            // btnNavStart
            // 
            this.btnNavStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNavStart.Image = ((System.Drawing.Image)(resources.GetObject("btnNavStart.Image")));
            this.btnNavStart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNavStart.Name = "btnNavStart";
            this.btnNavStart.Size = new System.Drawing.Size(23, 22);
            this.btnNavStart.Text = "First";
            this.btnNavStart.Click += new System.EventHandler(this.btnNavStart_Click);
            // 
            // btnNavBack
            // 
            this.btnNavBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNavBack.Image = ((System.Drawing.Image)(resources.GetObject("btnNavBack.Image")));
            this.btnNavBack.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNavBack.Name = "btnNavBack";
            this.btnNavBack.Size = new System.Drawing.Size(23, 22);
            this.btnNavBack.Text = "Previous";
            this.btnNavBack.Click += new System.EventHandler(this.btnNavBack_Click);
            // 
            // lblTranscriptMove
            // 
            this.lblTranscriptMove.Name = "lblTranscriptMove";
            this.lblTranscriptMove.Size = new System.Drawing.Size(24, 22);
            this.lblTranscriptMove.Text = "0/0";
            // 
            // btnNavNext
            // 
            this.btnNavNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNavNext.Image = ((System.Drawing.Image)(resources.GetObject("btnNavNext.Image")));
            this.btnNavNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNavNext.Name = "btnNavNext";
            this.btnNavNext.Size = new System.Drawing.Size(23, 22);
            this.btnNavNext.Text = "Next";
            this.btnNavNext.Click += new System.EventHandler(this.btnNavNext_Click);
            // 
            // btnNavEnd
            // 
            this.btnNavEnd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNavEnd.Image = ((System.Drawing.Image)(resources.GetObject("btnNavEnd.Image")));
            this.btnNavEnd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNavEnd.Name = "btnNavEnd";
            this.btnNavEnd.Size = new System.Drawing.Size(23, 22);
            this.btnNavEnd.Text = "Last";
            this.btnNavEnd.Click += new System.EventHandler(this.btnNavEnd_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "*.vgt";
            this.saveFileDialog1.Filter = "Volcanoes Game Transcript|*.vgt";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Volcanoes Game Transcript|*.vgt";
            // 
            // growthMoveTimer
            // 
            this.growthMoveTimer.Interval = 1000;
            this.growthMoveTimer.Tick += new System.EventHandler(this.growthMoveTimer_Tick);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(201, 6);
            // 
            // whiteboardModeToolStripMenuItem
            // 
            this.whiteboardModeToolStripMenuItem.CheckOnClick = true;
            this.whiteboardModeToolStripMenuItem.Name = "whiteboardModeToolStripMenuItem";
            this.whiteboardModeToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.whiteboardModeToolStripMenuItem.Text = "Whiteboard Mode";
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1052, 600);
            this.Controls.Add(this.toolStripContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "GameForm";
            this.Text = "Volcanoes";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GameForm_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.GameForm_KeyDown);
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

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
    }
}


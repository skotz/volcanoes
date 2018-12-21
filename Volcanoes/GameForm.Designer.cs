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
            this.selfPlayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tournamentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newTournamentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnNewGame = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.cbPlayerOne = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.cbPlayerTwo = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.exportRulesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.tournamentToolStripMenuItem,
            this.aboutToolStripMenuItem});
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
            this.selfPlayToolStripMenuItem,
            this.toolStripSeparator3,
            this.exportRulesToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.fileToolStripMenuItem.Text = "&Game";
            // 
            // newGameToolStripMenuItem
            // 
            this.newGameToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newGameToolStripMenuItem.Image")));
            this.newGameToolStripMenuItem.Name = "newGameToolStripMenuItem";
            this.newGameToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.newGameToolStripMenuItem.Text = "&New Game";
            this.newGameToolStripMenuItem.Click += new System.EventHandler(this.btnNewGame_Click);
            // 
            // selfPlayToolStripMenuItem
            // 
            this.selfPlayToolStripMenuItem.Name = "selfPlayToolStripMenuItem";
            this.selfPlayToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.selfPlayToolStripMenuItem.Text = "&Self Play";
            this.selfPlayToolStripMenuItem.Click += new System.EventHandler(this.selfPlayToolStripMenuItem_Click);
            // 
            // tournamentToolStripMenuItem
            // 
            this.tournamentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newTournamentToolStripMenuItem});
            this.tournamentToolStripMenuItem.Name = "tournamentToolStripMenuItem";
            this.tournamentToolStripMenuItem.Size = new System.Drawing.Size(84, 20);
            this.tournamentToolStripMenuItem.Text = "&Tournament";
            // 
            // newTournamentToolStripMenuItem
            // 
            this.newTournamentToolStripMenuItem.Name = "newTournamentToolStripMenuItem";
            this.newTournamentToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
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
            this.cbPlayerTwo});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(1052, 25);
            this.toolStrip1.Stretch = true;
            this.toolStrip1.TabIndex = 1;
            // 
            // btnNewGame
            // 
            this.btnNewGame.Image = ((System.Drawing.Image)(resources.GetObject("btnNewGame.Image")));
            this.btnNewGame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNewGame.Name = "btnNewGame";
            this.btnNewGame.Size = new System.Drawing.Size(85, 22);
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
            this.cbPlayerOne.Size = new System.Drawing.Size(121, 25);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(64, 22);
            this.toolStripLabel2.Text = "Player Two";
            // 
            // cbPlayerTwo
            // 
            this.cbPlayerTwo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPlayerTwo.Name = "cbPlayerTwo";
            this.cbPlayerTwo.Size = new System.Drawing.Size(121, 25);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(177, 6);
            // 
            // exportRulesToolStripMenuItem
            // 
            this.exportRulesToolStripMenuItem.Name = "exportRulesToolStripMenuItem";
            this.exportRulesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exportRulesToolStripMenuItem.Text = "&Export Rules";
            this.exportRulesToolStripMenuItem.Click += new System.EventHandler(this.exportRulesToolStripMenuItem_Click);
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1052, 600);
            this.Controls.Add(this.toolStripContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "GameForm";
            this.Text = "Volcanoes";
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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem newGameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tournamentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newTournamentToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton btnNewGame;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem selfPlayToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem exportRulesToolStripMenuItem;
    }
}


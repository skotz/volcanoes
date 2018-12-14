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
            this.ddlPlayerOne = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ddlPlayerTwo = new System.Windows.Forms.ComboBox();
            this.btnNewGame = new System.Windows.Forms.Button();
            this.btnRunTests = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // gamePanel
            // 
            this.gamePanel.Location = new System.Drawing.Point(12, 41);
            this.gamePanel.Name = "gamePanel";
            this.gamePanel.Size = new System.Drawing.Size(1016, 494);
            this.gamePanel.TabIndex = 0;
            this.gamePanel.Click += new System.EventHandler(this.gamePanel_Click);
            // 
            // gameTimer
            // 
            this.gameTimer.Interval = 30;
            this.gameTimer.Tick += new System.EventHandler(this.gameTimer_Tick);
            // 
            // ddlPlayerOne
            // 
            this.ddlPlayerOne.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlPlayerOne.FormattingEnabled = true;
            this.ddlPlayerOne.Items.AddRange(new object[] {
            "Human"});
            this.ddlPlayerOne.Location = new System.Drawing.Point(80, 11);
            this.ddlPlayerOne.Name = "ddlPlayerOne";
            this.ddlPlayerOne.Size = new System.Drawing.Size(215, 21);
            this.ddlPlayerOne.TabIndex = 1;
            this.ddlPlayerOne.SelectedIndexChanged += new System.EventHandler(this.ddlPlayerOne_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(301, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Player Two:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Player One:";
            // 
            // ddlPlayerTwo
            // 
            this.ddlPlayerTwo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlPlayerTwo.FormattingEnabled = true;
            this.ddlPlayerTwo.Items.AddRange(new object[] {
            "Human"});
            this.ddlPlayerTwo.Location = new System.Drawing.Point(370, 11);
            this.ddlPlayerTwo.Name = "ddlPlayerTwo";
            this.ddlPlayerTwo.Size = new System.Drawing.Size(215, 21);
            this.ddlPlayerTwo.TabIndex = 1;
            this.ddlPlayerTwo.SelectedIndexChanged += new System.EventHandler(this.ddlPlayerTwo_SelectedIndexChanged);
            // 
            // btnNewGame
            // 
            this.btnNewGame.Location = new System.Drawing.Point(905, 9);
            this.btnNewGame.Name = "btnNewGame";
            this.btnNewGame.Size = new System.Drawing.Size(123, 23);
            this.btnNewGame.TabIndex = 4;
            this.btnNewGame.Text = "New Game";
            this.btnNewGame.UseVisualStyleBackColor = true;
            this.btnNewGame.Click += new System.EventHandler(this.btnNewGame_Click);
            // 
            // btnRunTests
            // 
            this.btnRunTests.Location = new System.Drawing.Point(12, 598);
            this.btnRunTests.Name = "btnRunTests";
            this.btnRunTests.Size = new System.Drawing.Size(123, 23);
            this.btnRunTests.TabIndex = 5;
            this.btnRunTests.Text = "Play 1K";
            this.btnRunTests.UseVisualStyleBackColor = true;
            this.btnRunTests.Click += new System.EventHandler(this.btnRunTests_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 582);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(271, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Self-play 1,000 games and record the results in data.csv";
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1038, 547);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnRunTests);
            this.Controls.Add(this.btnNewGame);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ddlPlayerTwo);
            this.Controls.Add(this.ddlPlayerOne);
            this.Controls.Add(this.gamePanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GameForm";
            this.Text = "Volcanoes";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel gamePanel;
        private System.Windows.Forms.Timer gameTimer;
        private System.Windows.Forms.ComboBox ddlPlayerOne;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox ddlPlayerTwo;
        private System.Windows.Forms.Button btnNewGame;
        private System.Windows.Forms.Button btnRunTests;
        private System.Windows.Forms.Label label3;
    }
}


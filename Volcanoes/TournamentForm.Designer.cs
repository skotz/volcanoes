namespace Volcano
{
    partial class TournamentForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TournamentForm));
            numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            label1 = new System.Windows.Forms.Label();
            button1 = new System.Windows.Forms.Button();
            checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            numSecondsPerMove = new System.Windows.Forms.NumericUpDown();
            cbSelfPlay = new System.Windows.Forms.CheckBox();
            comboType = new System.Windows.Forms.ComboBox();
            label4 = new System.Windows.Forms.Label();
            cbParallel = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSecondsPerMove).BeginInit();
            SuspendLayout();
            // 
            // numericUpDown1
            // 
            numericUpDown1.Location = new System.Drawing.Point(140, 14);
            numericUpDown1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numericUpDown1.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numericUpDown1.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDown1.Name = "numericUpDown1";
            numericUpDown1.Size = new System.Drawing.Size(327, 23);
            numericUpDown1.TabIndex = 4;
            numericUpDown1.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(14, 14);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(47, 15);
            label1.TabIndex = 3;
            label1.Text = "Rounds";
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(379, 465);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(88, 27);
            button1.TabIndex = 5;
            button1.Text = "Start";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // checkedListBox1
            // 
            checkedListBox1.CheckOnClick = true;
            checkedListBox1.FormattingEnabled = true;
            checkedListBox1.Location = new System.Drawing.Point(140, 105);
            checkedListBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkedListBox1.Name = "checkedListBox1";
            checkedListBox1.Size = new System.Drawing.Size(326, 346);
            checkedListBox1.TabIndex = 6;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(14, 105);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(44, 15);
            label2.TabIndex = 3;
            label2.Text = "Players";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(14, 44);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(104, 15);
            label3.TabIndex = 3;
            label3.Text = "Seconds Per Move";
            // 
            // numSecondsPerMove
            // 
            numSecondsPerMove.Location = new System.Drawing.Point(140, 44);
            numSecondsPerMove.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numSecondsPerMove.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numSecondsPerMove.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSecondsPerMove.Name = "numSecondsPerMove";
            numSecondsPerMove.Size = new System.Drawing.Size(327, 23);
            numSecondsPerMove.TabIndex = 4;
            numSecondsPerMove.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // cbSelfPlay
            // 
            cbSelfPlay.AutoSize = true;
            cbSelfPlay.Location = new System.Drawing.Point(140, 470);
            cbSelfPlay.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cbSelfPlay.Name = "cbSelfPlay";
            cbSelfPlay.Size = new System.Drawing.Size(103, 19);
            cbSelfPlay.TabIndex = 7;
            cbSelfPlay.Text = "Allow Self Play";
            cbSelfPlay.UseVisualStyleBackColor = true;
            // 
            // comboType
            // 
            comboType.FormattingEnabled = true;
            comboType.Items.AddRange(new object[] { "Round Robin", "Swiss Pairing" });
            comboType.Location = new System.Drawing.Point(140, 74);
            comboType.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            comboType.Name = "comboType";
            comboType.Size = new System.Drawing.Size(326, 23);
            comboType.TabIndex = 8;
            comboType.SelectedIndexChanged += comboType_SelectedIndexChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(14, 74);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(31, 15);
            label4.TabIndex = 3;
            label4.Text = "Type";
            // 
            // cbParallel
            // 
            cbParallel.AutoSize = true;
            cbParallel.Location = new System.Drawing.Point(251, 470);
            cbParallel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cbParallel.Name = "cbParallel";
            cbParallel.Size = new System.Drawing.Size(64, 19);
            cbParallel.TabIndex = 9;
            cbParallel.Text = "Parallel";
            cbParallel.UseVisualStyleBackColor = true;
            cbParallel.CheckedChanged += cbParallel_CheckedChanged;
            // 
            // TournamentForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(481, 505);
            Controls.Add(cbParallel);
            Controls.Add(comboType);
            Controls.Add(cbSelfPlay);
            Controls.Add(checkedListBox1);
            Controls.Add(button1);
            Controls.Add(numSecondsPerMove);
            Controls.Add(numericUpDown1);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TournamentForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Volcanoes - Tournament";
            ((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSecondsPerMove).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numSecondsPerMove;
        private System.Windows.Forms.CheckBox cbSelfPlay;
        private System.Windows.Forms.ComboBox comboType;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox cbParallel;
    }
}
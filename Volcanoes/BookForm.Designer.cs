namespace Volcano
{
    partial class BookForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BookForm));
            button1 = new System.Windows.Forms.Button();
            numIterations = new System.Windows.Forms.NumericUpDown();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            numDepth = new System.Windows.Forms.NumericUpDown();
            progressBar2 = new System.Windows.Forms.ProgressBar();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            labelStatus = new System.Windows.Forms.Label();
            numGap = new System.Windows.Forms.NumericUpDown();
            label3 = new System.Windows.Forms.Label();
            cbParallel = new System.Windows.Forms.CheckBox();
            cbResume = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)numIterations).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDepth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numGap).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(131, 127);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(88, 27);
            button1.TabIndex = 2;
            button1.Text = "Generate";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // numIterations
            // 
            numIterations.Location = new System.Drawing.Point(96, 44);
            numIterations.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numIterations.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
            numIterations.Name = "numIterations";
            numIterations.Size = new System.Drawing.Size(123, 23);
            numIterations.TabIndex = 3;
            numIterations.Value = new decimal(new int[] { 1000000, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(13, 46);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(69, 15);
            label1.TabIndex = 4;
            label1.Text = "Simulations";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(13, 16);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(39, 15);
            label2.TabIndex = 5;
            label2.Text = "Depth";
            // 
            // numDepth
            // 
            numDepth.Enabled = false;
            numDepth.Location = new System.Drawing.Point(96, 14);
            numDepth.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numDepth.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numDepth.Name = "numDepth";
            numDepth.ReadOnly = true;
            numDepth.Size = new System.Drawing.Size(123, 23);
            numDepth.TabIndex = 6;
            numDepth.Value = new decimal(new int[] { 7, 0, 0, 0 });
            // 
            // progressBar2
            // 
            progressBar2.Location = new System.Drawing.Point(13, 160);
            progressBar2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            progressBar2.Name = "progressBar2";
            progressBar2.Size = new System.Drawing.Size(206, 27);
            progressBar2.TabIndex = 8;
            // 
            // backgroundWorker1
            // 
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.Location = new System.Drawing.Point(13, 133);
            labelStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new System.Drawing.Size(39, 15);
            labelStatus.TabIndex = 9;
            labelStatus.Text = "Ready";
            // 
            // numGap
            // 
            numGap.Location = new System.Drawing.Point(96, 73);
            numGap.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numGap.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
            numGap.Name = "numGap";
            numGap.Size = new System.Drawing.Size(123, 23);
            numGap.TabIndex = 3;
            numGap.Value = new decimal(new int[] { 10000, 0, 0, 0 });
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(13, 75);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(28, 15);
            label3.TabIndex = 4;
            label3.Text = "Gap";
            // 
            // cbParallel
            // 
            cbParallel.AutoSize = true;
            cbParallel.Location = new System.Drawing.Point(156, 102);
            cbParallel.Name = "cbParallel";
            cbParallel.Size = new System.Drawing.Size(64, 19);
            cbParallel.TabIndex = 10;
            cbParallel.Text = "Parallel";
            cbParallel.UseVisualStyleBackColor = true;
            // 
            // cbResume
            // 
            cbResume.AutoSize = true;
            cbResume.Location = new System.Drawing.Point(81, 102);
            cbResume.Name = "cbResume";
            cbResume.Size = new System.Drawing.Size(68, 19);
            cbResume.TabIndex = 10;
            cbResume.Text = "Resume";
            cbResume.UseVisualStyleBackColor = true;
            // 
            // BookForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(232, 199);
            Controls.Add(cbResume);
            Controls.Add(cbParallel);
            Controls.Add(labelStatus);
            Controls.Add(progressBar2);
            Controls.Add(numDepth);
            Controls.Add(label2);
            Controls.Add(label3);
            Controls.Add(label1);
            Controls.Add(numGap);
            Controls.Add(numIterations);
            Controls.Add(button1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "BookForm";
            Text = "Opening Book Generator";
            FormClosing += BookForm_FormClosing;
            ((System.ComponentModel.ISupportInitialize)numIterations).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDepth).EndInit();
            ((System.ComponentModel.ISupportInitialize)numGap).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.NumericUpDown numIterations;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numDepth;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.NumericUpDown numGap;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbParallel;
        private System.Windows.Forms.CheckBox cbResume;
    }
}
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
            ((System.ComponentModel.ISupportInitialize)numIterations).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDepth).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(132, 74);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(88, 27);
            button1.TabIndex = 2;
            button1.Text = "Generate";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // numSeconds
            // 
            numIterations.Location = new System.Drawing.Point(79, 44);
            numIterations.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numIterations.Maximum = new decimal(new int[] { 1000000000, 0, 0, 0 });
            numIterations.Name = "numSeconds";
            numIterations.Size = new System.Drawing.Size(140, 23);
            numIterations.TabIndex = 3;
            numIterations.Value = new decimal(new int[] { 100000, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 46);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(56, 15);
            label1.TabIndex = 4;
            label1.Text = "Iterations";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 16);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(39, 15);
            label2.TabIndex = 5;
            label2.Text = "Depth";
            // 
            // numDepth
            // 
            numDepth.Enabled = false;
            numDepth.Location = new System.Drawing.Point(79, 14);
            numDepth.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numDepth.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            numDepth.Name = "numDepth";
            numDepth.ReadOnly = true;
            numDepth.Size = new System.Drawing.Size(140, 23);
            numDepth.TabIndex = 6;
            numDepth.Value = new decimal(new int[] { 7, 0, 0, 0 });
            // 
            // progressBar2
            // 
            progressBar2.Location = new System.Drawing.Point(14, 107);
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
            labelStatus.Location = new System.Drawing.Point(14, 80);
            labelStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new System.Drawing.Size(39, 15);
            labelStatus.TabIndex = 9;
            labelStatus.Text = "Ready";
            // 
            // BookForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(234, 145);
            Controls.Add(labelStatus);
            Controls.Add(progressBar2);
            Controls.Add(numDepth);
            Controls.Add(label2);
            Controls.Add(label1);
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
    }
}
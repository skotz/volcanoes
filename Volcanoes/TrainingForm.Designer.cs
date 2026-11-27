namespace Volcano
{
    partial class TrainingForm
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
            rtbStatus = new System.Windows.Forms.RichTextBox();
            SuspendLayout();
            // 
            // rtbStatus
            // 
            rtbStatus.BackColor = System.Drawing.Color.Black;
            rtbStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            rtbStatus.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            rtbStatus.ForeColor = System.Drawing.Color.LimeGreen;
            rtbStatus.Location = new System.Drawing.Point(0, 0);
            rtbStatus.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rtbStatus.Name = "rtbStatus";
            rtbStatus.ReadOnly = true;
            rtbStatus.Size = new System.Drawing.Size(977, 662);
            rtbStatus.TabIndex = 0;
            rtbStatus.Text = "";
            // 
            // TrainingForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(977, 662);
            Controls.Add(rtbStatus);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "TrainingForm";
            Text = "Engine Trainer";
            Load += TrainingForm_Load;
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbStatus;
    }
}
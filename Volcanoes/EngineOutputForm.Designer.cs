namespace Volcano
{
    partial class EngineOutputForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EngineOutputForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dvEngineOne = new System.Windows.Forms.DataGridView();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.dvEngineTwo = new System.Windows.Forms.DataGridView();
            this.MoveName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MoveRating = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PrimaryVariation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dvEngineOne)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dvEngineTwo)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 4);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer1.Size = new System.Drawing.Size(717, 295);
            this.splitContainer1.SplitterDistance = 143;
            this.splitContainer1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dvEngineOne);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(717, 143);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Engine 1";
            // 
            // dvEngineOne
            // 
            this.dvEngineOne.AllowUserToAddRows = false;
            this.dvEngineOne.AllowUserToDeleteRows = false;
            this.dvEngineOne.AllowUserToResizeRows = false;
            this.dvEngineOne.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dvEngineOne.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.MoveName,
            this.MoveRating,
            this.PrimaryVariation});
            this.dvEngineOne.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dvEngineOne.Location = new System.Drawing.Point(3, 16);
            this.dvEngineOne.Name = "dvEngineOne";
            this.dvEngineOne.ReadOnly = true;
            this.dvEngineOne.Size = new System.Drawing.Size(711, 124);
            this.dvEngineOne.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.dvEngineTwo);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(717, 148);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Engine 2";
            // 
            // dvEngineTwo
            // 
            this.dvEngineTwo.AllowUserToAddRows = false;
            this.dvEngineTwo.AllowUserToDeleteRows = false;
            this.dvEngineTwo.AllowUserToResizeRows = false;
            this.dvEngineTwo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dvEngineTwo.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataGridViewTextBoxColumn1,
            this.dataGridViewTextBoxColumn2,
            this.dataGridViewTextBoxColumn3});
            this.dvEngineTwo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dvEngineTwo.Location = new System.Drawing.Point(3, 16);
            this.dvEngineTwo.Name = "dvEngineTwo";
            this.dvEngineTwo.ReadOnly = true;
            this.dvEngineTwo.Size = new System.Drawing.Size(711, 129);
            this.dvEngineTwo.TabIndex = 1;
            // 
            // MoveName
            // 
            this.MoveName.DataPropertyName = "MoveName";
            this.MoveName.HeaderText = "Move";
            this.MoveName.Name = "MoveName";
            this.MoveName.ReadOnly = true;
            // 
            // MoveRating
            // 
            this.MoveRating.DataPropertyName = "MoveRating";
            this.MoveRating.HeaderText = "Evaluation";
            this.MoveRating.Name = "MoveRating";
            this.MoveRating.ReadOnly = true;
            // 
            // PrimaryVariation
            // 
            this.PrimaryVariation.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.PrimaryVariation.DataPropertyName = "ExtraInfo";
            this.PrimaryVariation.HeaderText = "Primary Variation";
            this.PrimaryVariation.Name = "PrimaryVariation";
            this.PrimaryVariation.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "MoveName";
            this.dataGridViewTextBoxColumn1.HeaderText = "Move";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "MoveRating";
            this.dataGridViewTextBoxColumn2.HeaderText = "Evaluation";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.ReadOnly = true;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn3.DataPropertyName = "ExtraInfo";
            this.dataGridViewTextBoxColumn3.HeaderText = "Primary Variation";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.ReadOnly = true;
            // 
            // EngineOutputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(717, 299);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "EngineOutputForm";
            this.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Volcanoes - Engine Output";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dvEngineOne)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dvEngineTwo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView dvEngineOne;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataGridView dvEngineTwo;
        private System.Windows.Forms.DataGridViewTextBoxColumn MoveName;
        private System.Windows.Forms.DataGridViewTextBoxColumn MoveRating;
        private System.Windows.Forms.DataGridViewTextBoxColumn PrimaryVariation;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
    }
}
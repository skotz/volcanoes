using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Volcano.Engine;

namespace Volcano
{
    public partial class BookForm : Form
    {
        public string BookLocation { get; set; }

        public BookForm(string location)
        {
            InitializeComponent();

            BookLocation = location;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                button1.Enabled = false;
                numSeconds.Enabled = false;
                backgroundWorker1.RunWorkerAsync((int)numSeconds.Value);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (File.Exists(BookLocation))
            {
                File.Move(BookLocation, BookLocation + DateTime.Now.ToString("yyyyMMddhhmmss") + ".bak.");
            }

            var bookGenerator = new OpeningBook(BookLocation);
            bookGenerator.OnStatusUpdate += BookGenerator_OnStatusUpdate;
            bookGenerator.Generate(7, (int)e.Argument);
        }

        private void BookGenerator_OnStatusUpdate(int completed, int total)
        {
            var percent = (int)(100.0 * completed / total);

            backgroundWorker1.ReportProgress(percent, completed.ToString("N0") + "/" + total.ToString("N0"));
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar2.Value = e.ProgressPercentage;
            labelStatus.Text = (string)e.UserState;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            numSeconds.Enabled = true;
            MessageBox.Show("Done");
        }

        private void BookForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                if (MessageBox.Show("Do you want to cancel the book generation?", "Book Generator", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
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

            cbResume.Enabled = File.Exists("book.temp");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                button1.Enabled = false;
                numIterations.Enabled = false;
                numGap.Enabled = false;
                cbParallel.Enabled = false;
                cbResume.Enabled = false;
                backgroundWorker1.RunWorkerAsync(new int[] { (int)numIterations.Value, (int)numGap.Value, cbParallel.Checked ? 1 : 0, cbResume.Checked ? 1 : 0 });
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var args = e.Argument as int[];
            var resume = args[3] == 1;

            if (File.Exists(BookLocation))
            {
                if (resume)
                {
                    File.Copy(BookLocation, BookLocation + "." + DateTime.Now.ToString("yyyyMMddhhmmss") + ".resume.bak.");
                }
                else
                {
                    File.Move(BookLocation, BookLocation + "." + DateTime.Now.ToString("yyyyMMddhhmmss") + ".bak.");
                }
            }

            if (File.Exists("book.temp"))
            {
                if (resume)
                {
                    File.Copy("book.temp", "book.temp" + "." + DateTime.Now.ToString("yyyyMMddhhmmss") + ".resume.bak.");
                }
                else
                {
                    File.Move("book.temp", "book.temp" + "." + DateTime.Now.ToString("yyyyMMddhhmmss") + ".bak.");
                }
            }

            var bookGenerator = new OpeningBook(BookLocation);
            bookGenerator.OnStatusUpdate += BookGenerator_OnStatusUpdate;
            bookGenerator.Generate(7, args[0], args[1], args[2] == 1, resume);
        }

        private void BookGenerator_OnStatusUpdate(int completed, int total)
        {
            var percent = (int)(100.0 * completed / total);

            backgroundWorker1.ReportProgress(percent, completed == 0 ? "Initializing" : (completed.ToString("N0") + "/" + total.ToString("N0")));
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar2.Value = e.ProgressPercentage;
            labelStatus.Text = (string)e.UserState;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            numIterations.Enabled = true;
            numGap.Enabled = true;
            cbParallel.Enabled = true;
            cbResume.Enabled = File.Exists("book.temp");
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
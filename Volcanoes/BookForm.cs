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

        private OpeningBook _bookGenerator;

        public BookForm(string location)
        {
            InitializeComponent();

            BookLocation = location;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                numDepth.Enabled = false;
                button1.Enabled = false;
                numIterations.Enabled = false;
                numGap.Enabled = false;
                cbParallel.Enabled = false;
                cbResume.Enabled = false;
                cbExtend.Enabled = false;
                btnStop.Enabled = true;
                txtExtend.Enabled = false;
                backgroundWorker1.RunWorkerAsync(new int[] { (int)numIterations.Value, (int)numGap.Value, cbParallel.Checked ? 1 : 0, cbResume.Checked ? 1 : 0, cbExtend.Checked ? 1 : 0, (int)numDepth.Value });
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var args = e.Argument as int[];
            var resume = args[3] == 1;
            var extend = args[4] == 1;

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

            _bookGenerator = new OpeningBook(BookLocation);
            _bookGenerator.OnStatusUpdate += BookGenerator_OnStatusUpdate;

            if (extend)
            {
                _bookGenerator.Extend(args[5], args[0], args[1], args[2] == 1, resume, txtExtend.Text);
            }
            else
            {
                _bookGenerator.Generate(args[5], args[0], args[1], args[2] == 1, resume);
            }
        }

        private void BookGenerator_OnStatusUpdate(int completed, int total, string message)
        {
            var percent = (int)(100.0 * completed / total);

            if (percent > 100)
            {
                percent = 100;
            }

            var status = completed == 0 ? "Initializing" : (completed.ToString("N0") + "/" + total.ToString("N0"));
            if (!string.IsNullOrEmpty(message))
            {
                status = message;
            }

            backgroundWorker1.ReportProgress(percent, status);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar2.Value = e.ProgressPercentage;
            labelStatus.Text = (string)e.UserState;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            numDepth.Enabled = true;
            button1.Enabled = true;
            numIterations.Enabled = true;
            numGap.Enabled = true;
            cbParallel.Enabled = true;
            cbExtend.Enabled = true;
            cbResume.Enabled = true; // File.Exists("book.temp");
            btnStop.Enabled = false;
            txtExtend.Enabled = true;
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

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_bookGenerator != null)
            {
                _bookGenerator.Cancel();
            }
        }
    }
}
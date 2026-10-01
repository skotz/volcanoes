using System;
using System.ComponentModel;
using System.Windows.Forms;
using Volcano.Engine;
using Volcano.Search;

namespace Volcano
{
    public partial class TrainingForm : Form
    {
        private ILearn _engine;
        private BackgroundWorker _worker;

        public TrainingForm()
        {
            InitializeComponent();

            _engine = new AlphaZeroEngine(true);
            _engine.OnDebug += engineToTrain_OnDebug;

            _worker = new BackgroundWorker();
            _worker.WorkerReportsProgress = true;
            _worker.DoWork += worker_DoWork;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            _worker.ProgressChanged += worker_ProgressChanged;
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var status = e.UserState as LearnStatus;

            try
            {
                rtbStatus.AppendText("\r\n" + status?.Summary);
                rtbStatus.SelectionStart = rtbStatus.Text.Length;
                rtbStatus.ScrollToCaret();
            }
            catch
            {
                Console.WriteLine(status?.Summary);
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnTrain.Enabled = true;
            rtbStatus.AppendText("\r\n[DONE]");

            StaticWinCache.Enabled = false;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            _engine.Train();
        }

        private void engineToTrain_OnDebug(object sender, LearnStatus e)
        {
            _worker.ReportProgress(0, e);
        }

        private void btnTrain_Click(object sender, EventArgs e)
        {
            if (!_worker.IsBusy)
            {
                StaticWinCache.Enabled = true;

                rtbStatus.Text = "[START]";
                btnTrain.Enabled = false;
                _worker.RunWorkerAsync();
            }
        }
    }
}
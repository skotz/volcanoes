using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Volcano.Engine;

namespace Volcano
{
    public partial class TournamentForm : Form
    {
        public List<string> Engines { get; set; }

        public int Rounds { get; set; }

        public int SecondsPerMove { get; set; }

        public bool ForceSimulations { get; set; }

        public bool SelfPlay { get; set; }

        public bool Parallel { get; set; }

        public TournamentType TournamentType { get; set; }

        private EngineHelper _engines;

        internal TournamentForm(EngineHelper engines)
        {
            InitializeComponent();

            _engines = engines;

            foreach (string engine in engines.EngineNames)
            {
                checkedListBox1.Items.Add(engine);
            }

            DialogResult = DialogResult.Cancel;

            comboType.SelectedIndex = 0;
        }

        private void CheckAll(bool value)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, value);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var invalid = false;
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i) && !_engines.IsValid(checkedListBox1.Items[i].ToString()))
                {
                    checkedListBox1.SetItemChecked(i, false);
                    invalid = true;
                }
            }
            if (invalid)
            {
                MessageBox.Show("Some engines appear to be invalid and were unselected.", "Volcanoes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Rounds = (int)numericUpDown1.Value;

            SecondsPerMove = (int)numSecondsPerMove.Value;

            ForceSimulations = rbSimulations.Checked;

            Engines = checkedListBox1.CheckedItems.Cast<string>().ToList();

            SelfPlay = cbSelfPlay.Checked;

            Parallel = cbParallel.Checked;

            TournamentType = comboType.SelectedIndex == 1 ? TournamentType.Swiss : TournamentType.RoundRobin;

            if (Parallel && checkedListBox1.CheckedItems.Cast<string>().Any(x => x.ToLower().Contains("parallel")))
            {
                MessageBox.Show("Parallel engines do not run well in parallel tournaments and will time out. Please unselect the parallel engines or run the tournament sequentially.", "Volcanoes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (ForceSimulations && checkedListBox1.CheckedItems.Cast<string>().Any(x => !_engines.SupportsSimulations(x)))
            {
                var result = MessageBox.Show("Only MCTS and VolcanoZero engines support fixed simulation counts. Please unselect other engines or limit by seconds instead. Would you like to automatically unselect these engines?", "Volcanoes", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    for (int i = 0; i < checkedListBox1.Items.Count; i++)
                    {
                        if (checkedListBox1.GetItemChecked(i) && !_engines.SupportsSimulations(checkedListBox1.Items[i].ToString()))
                        {
                            checkedListBox1.SetItemChecked(i, false);
                        }
                    }
                }

                return;
            }

            if (Engines.Count >= 2 || (SelfPlay && Engines.Count >= 1))
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Please select 2 or more engines.", "Volcanoes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void comboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboType.SelectedIndex == 1)
            {
                cbSelfPlay.Checked = false;
                cbSelfPlay.Enabled = false;
            }
            else
            {
                cbSelfPlay.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CheckAll(true);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CheckAll(false);
        }

        private void rbSimulations_CheckedChanged(object sender, EventArgs e)
        {
            rbSeconds.Checked = !rbSimulations.Checked;
        }

        private void rbSeconds_CheckedChanged(object sender, EventArgs e)
        {
            rbSimulations.Checked = !rbSeconds.Checked;
        }
    }
}
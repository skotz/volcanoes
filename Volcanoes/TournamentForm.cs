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

        public bool SelfPlay { get; set; }

        public bool Parallel { get; set; }

        public TournamentType TournamentType { get; set; }

        public TournamentForm(List<string> engines)
        {
            InitializeComponent();

            foreach (string engine in engines)
            {
                checkedListBox1.Items.Add(engine);
            }

            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, true);
            }

            DialogResult = DialogResult.Cancel;

            comboType.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Rounds = (int)numericUpDown1.Value;

            SecondsPerMove = (int)numSecondsPerMove.Value;

            Engines = checkedListBox1.CheckedItems.Cast<string>().ToList();

            SelfPlay = cbSelfPlay.Checked;

            Parallel = cbParallel.Checked;

            TournamentType = comboType.SelectedIndex == 1 ? TournamentType.Swiss : TournamentType.RoundRobin;

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

        private void cbParallel_CheckedChanged(object sender, EventArgs e)
        {
            if (cbParallel.Checked && checkedListBox1.CheckedItems.Cast<string>().Any(x => x.ToLower().Contains("parallel")))
            {
                MessageBox.Show("Parallel engines do not run well in parallel tournaments and will time out! Please unselect the parallel engines or run the tournament sequentially.", "Volcanoes", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
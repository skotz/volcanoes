using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Volcano
{
    public partial class TournamentForm : Form
    {
        public List<string> Engines { get; set; }

        public int Rounds { get; set; }

        public int SecondsPerMove { get; set; }

        public bool SelfPlay { get; set; }

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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Rounds = (int)numericUpDown1.Value;

            SecondsPerMove = (int)numSecondsPerMove.Value;

            Engines = checkedListBox1.CheckedItems.Cast<string>().ToList();

            SelfPlay = cbSelfPlay.Checked;

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
    }
}
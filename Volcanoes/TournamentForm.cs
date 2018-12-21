using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Volcano
{
    public partial class TournamentForm : Form
    {
        public List<string> Engines { get; set; }

        public int Rounds { get; set; }

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

            Engines = checkedListBox1.CheckedItems.Cast<string>().ToList();

            if (Engines.Count >= 2)
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

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
    public partial class SelfPlayForm : Form
    {
        public int GamesToPlay
        {
            get
            {
                return (int)numericUpDown1.Value;
            }
        }

        public string EngineOne
        {
            get
            {
                return comboBox1.Text;
            }
        }

        public string EngineTwo
        {
            get
            {
                return comboBox2.Text;
            }
        }

        public SelfPlayForm(List<string> engines)
        {
            InitializeComponent();

            foreach (string engine in engines)
            {
                comboBox1.Items.Add(engine);
                comboBox2.Items.Add(engine);
            }
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;

            DialogResult = DialogResult.Cancel;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

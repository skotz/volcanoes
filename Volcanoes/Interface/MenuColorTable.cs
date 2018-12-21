using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Volcano.Interface
{
    class MenuColorTable : ProfessionalColorTable
    {
        public MenuColorTable()
        {
            UseSystemColors = false;
        }

        public override Color MenuStripGradientBegin
        {
            get { return SystemColors.Control; }
        }

        public override Color MenuStripGradientEnd
        {
            get { return SystemColors.Control; }
        }
    }
}

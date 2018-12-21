using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Volcano.Interface
{
    class MenuProfessionalRenderer : ToolStripProfessionalRenderer
    {
        public MenuProfessionalRenderer(ProfessionalColorTable table)
            :base (table)
        {
            RoundedEdges = false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Volcano.Engine;
using Volcano.Game;

namespace Volcano
{
    partial class EngineOutputForm : Form
    {
        public EngineOutputForm(VolcanoGame game)
        {
            InitializeComponent();

            game.OnEngineStatus += Game_OnEngineStatus;

            List<EngineDetails> details = new List<EngineDetails>();
            dvEngineOne.DataSource = details;
        }

        private void Game_OnEngineStatus(Player player, EngineStatus status)
        {
            List<EngineDetails> details = new List<EngineDetails>();
            foreach (var line in status.Details)
            {
                details.Add(new EngineDetails
                {
                    MoveName = line.MoveIndex >= 0 && line.MoveIndex < 80 ? Constants.TileNames[line.MoveIndex] : "",
                    MoveRating = line.Evaluation,
                    ExtraInfo = line.ExtraInformation
                });
            }

            if (player == Player.One)
            {
                dvEngineOne.DataSource = details;
            }
            else if (player == Player.Two)
            {
                dvEngineTwo.DataSource = details;
            }
        }
    }

    class EngineDetails
    {
        public string MoveName { get; set; }
        public double MoveRating { get; set; }
        public string ExtraInfo { get; set; }
    }
}

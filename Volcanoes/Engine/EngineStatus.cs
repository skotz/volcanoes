using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Engine
{
    class EngineStatus
    {
        public List<EngineStatusLine> Details { get; private set; }

        public EngineStatus()
        {
            Details = new List<EngineStatusLine>();
        }

        public void Add(int moveIndex, double evaluation, string extraInfo, double visits)
        {
            Details.Add(new EngineStatusLine
            {
                MoveIndex = moveIndex,
                Evaluation = evaluation,
                ExtraInformation = extraInfo,
                Visits = visits
            });
        }

        public void Sort()
        {
            Details.Sort((c, n) => n.Visits.CompareTo(c.Visits));
        }
    }

    class EngineStatusLine
    {
        public int MoveIndex { get; set; }

        public double Evaluation { get; set; }

        public string ExtraInformation { get; set; }

        public double Visits { get; set; }
    }
}

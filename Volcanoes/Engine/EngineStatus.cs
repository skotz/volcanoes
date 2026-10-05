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

        public void Add(int moveIndex, double evaluation, string extraInfo, double sort)
        {
            Details.Add(new EngineStatusLine
            {
                MoveIndex = moveIndex,
                Evaluation = evaluation,
                ExtraInformation = extraInfo,
                Sort = sort
            });
        }

        public void Sort()
        {
            Details.Sort((c, n) => n.Sort.CompareTo(c.Sort));
        }
    }

    class EngineStatusLine
    {
        public int MoveIndex { get; set; }

        public double Evaluation { get; set; }

        public string ExtraInformation { get; set; }

        public double Sort { get; set; }
    }
}

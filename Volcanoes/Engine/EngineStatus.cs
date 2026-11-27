using System.Collections.Generic;

namespace Volcano.Engine
{
    internal class EngineStatus
    {
        public List<EngineStatusLine> Details { get; private set; }

        public EngineStatus()
        {
            Details = new List<EngineStatusLine>();
        }

        public void Add(int moveIndex, double evaluation, string extraInfo, double sort, bool unevaluated = false)
        {
            Details.Add(new EngineStatusLine
            {
                MoveIndex = moveIndex,
                Evaluation = evaluation,
                ExtraInformation = extraInfo,
                Sort = sort,
                Unevaluated = unevaluated,
            });
        }

        public void Sort()
        {
            Details.Sort((c, n) => n.Sort.CompareTo(c.Sort));
        }
    }

    internal class EngineStatusLine
    {
        public int MoveIndex { get; set; }

        public double Evaluation { get; set; }

        public string ExtraInformation { get; set; }

        public double Sort { get; set; }

        public bool Unevaluated { get; set; }
    }
}
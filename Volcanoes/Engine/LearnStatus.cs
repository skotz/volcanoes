using System;

namespace Volcano.Engine
{
    public class LearnStatus
    {
        public LearnStatus(string summary)
        {
            Timestamp = DateTime.Now;
            Summary = summary;
        }

        public DateTime Timestamp { get; set; }
        public string Summary { get; set; }
    }
}
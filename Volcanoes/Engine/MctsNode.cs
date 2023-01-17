namespace Volcano.Engine
{
    public class MctsNode
    {
        public int Wins { get; set; }
        public int Visits { get; set; }
        public int Move { get; set; }

        public double WinRate
        {
            get
            {
                return Visits > 0 ? (double)Wins / Visits : 0;
            }
        }

        public double Score
        {
            get
            {
                return (Visits > 0 ? 200.0 * Wins / Visits : 0) - 100.0;
            }
        }
    }
}
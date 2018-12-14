using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Engine
{
    class SearchResult
    {
        public int Score { get; set; }

        public long Evaluations { get; set; }
        
        public Move BestMove { get; set; }
    }
}

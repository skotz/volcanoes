using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Engine
{
    class ExceptionEngine : IEngine
    {
        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            throw new Exception("Just making sure that an exception results in a resignation, not a crash!");
        }
    }
}

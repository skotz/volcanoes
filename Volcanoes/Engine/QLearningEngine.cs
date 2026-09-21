using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Engine
{
    internal class QLearningEngine : IEngine
    {
        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Engine
{
    interface IEngine
    {
        SearchResult GetBestMove(Board state, EngineCancellationToken token);
    }
}

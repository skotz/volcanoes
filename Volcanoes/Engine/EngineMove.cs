using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;
using Volcano.Search;

namespace Volcano.Engine
{
    class EngineMove
    {
        public Move Move { get; set; }
        public PathResult Path { get; set; }

        public EngineMove(Move move, PathResult path)
        {
            Move = move;
            Path = path;
        }
    }
}

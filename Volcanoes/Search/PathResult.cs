using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Search
{
    class PathResult
    {
        public List<int> Path { get; set; }
        public int Distance { get; set; }
        public bool Found { get; private set; }

        public PathResult()
            : this(new List<int>())
        {
        }

        public PathResult(List<int> path)
            : this(path, path.Count)
        {
        }

        public PathResult(List<int> path, int distance)
        {
            Path = path;
            Distance = distance;
            Found = (Path?.Count ?? 0) > 0;
        }

        public PathResult(bool found)
        {
            Found = found;
        }
    }
}

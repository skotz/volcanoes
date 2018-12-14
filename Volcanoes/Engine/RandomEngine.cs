using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Engine
{
    class RandomEngine : IEngine
    {
        private static Random _random = new Random();
        
        public SearchResult GetBestMove(Board state)
        {
            List<Move> moves = state.GetMoves();
            return new SearchResult
            {
                BestMove = moves[_random.Next(moves.Count)]
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;

namespace Volcano.Engine
{
    class AlphaEngine : IEngine
    {
        private static Random random = new Random();

        public SearchResult GetBestMove(Board state)
        {
            List<Move> moves = state.GetMoves();
            List<Move> alphaMoves = moves.Where(x => Constants.TileNames[x.TileIndex].EndsWith("A")).ToList();

            if (alphaMoves.Count > 0)
            {
                return new SearchResult
                {
                    BestMove = alphaMoves[random.Next(alphaMoves.Count)]
                };
            }
            else
            {
                return new SearchResult
                {
                    BestMove = moves[random.Next(moves.Count)]
                };
            }
        }
    }
}

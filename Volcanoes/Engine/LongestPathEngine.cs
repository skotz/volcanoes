using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volcano.Game;
using Volcano.Search;

namespace Volcano.Engine
{
    class LongestPathEngine : IEngine
    {
        private PathFinder pathFinder;
        private Stopwatch timer;
        private long evaluations;

        public LongestPathEngine()
        {
            pathFinder = new LongestPathFinder();
        }
        
        public SearchResult GetBestMove(Board state, EngineCancellationToken token)
        {
            timer = Stopwatch.StartNew(); 
            evaluations = 0;

            SearchResult result = new SearchResult();
            result.Score = int.MinValue;

            Player player = state.Player;

            // Get a list of all possible moves, randomly shuffled
            List<Move> moves = Shuffle(state.GetMoves());

            foreach (Move move in moves)
            {
                Board copy = new Board(state);
                copy.MakeMove(move);

                move.Evaluation = EvaluateLongestPath(copy, player, move.TileIndex);

                if (move.Evaluation > result.Score)
                {
                    result.BestMove = move;
                    result.Score = move.Evaluation;
                }
            }

            result.Evaluations = evaluations;
            result.Milliseconds = timer.ElapsedMilliseconds;

            return result;
        }

        private int EvaluateLongestPath(Board state, Player player, int lastIndex)
        {
            evaluations++;

            // Get a list of tiles owned by the player
            List<int> tiles = new List<int>();
            for (int i = 0; i < 80; i++)
            {
                if (state.Tiles[i].Owner == player)
                {
                    tiles.Add(i);
                }
            }

            // Find the longest possible path
            int longest = 0;
            //for (int i = 0; i < tiles.Count; i++)
            //{
            //    for (int j = i + 1; j < tiles.Count; j++)
            //    {
            //        int length = FindPath(state, i, j).Count;
            //        longest = Math.Max(length, longest);
            //    }
            //}
            for (int i = 0; i < tiles.Count; i++)
            {
                int length = pathFinder.FindPath(state, lastIndex, i).Path.Count;
                longest = Math.Max(length, longest);
            }

            return longest;
        }

        private static Random random = new Random();

        private List<Move> Shuffle(List<Move> list)
        {
            int n = list.Count;

            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                Move value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            return list;
        }
    }
}

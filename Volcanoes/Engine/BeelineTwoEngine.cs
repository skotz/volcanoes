using System;
using System.Collections.Generic;
using Volcano.Game;
using Volcano.Search;

namespace Volcano.Engine
{
    internal class BeelineTwoEngine : IEngine
    {
        private static Random random = new Random();

        private PathFinder pathFinder = new BeelinePathFinder();

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            var valid = state.GetMoves();
            var moves = new List<int>();

            for (var i = 0; i < 80; i++)
            {
                if ((state.Tiles[i] > 0 && state.Player == Player.One) || (state.Tiles[i] < 0 && state.Player == Player.Two))
                {
                    var path = pathFinder.FindPath(state, i, Constants.Antipodes[i]).Path;
                    foreach (var tile in path)
                    {
                        if (valid.Contains(tile) && !moves.Contains(tile))
                        {
                            moves.Add(tile);
                        }
                    }
                }
            }

            if (moves.Count == 0)
            {
                moves = valid;
            }

            var best = moves[random.Next(moves.Count)];
            return new SearchResult(best);
        }
    }
}
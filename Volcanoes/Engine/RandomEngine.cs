using System;
using System.Collections.Generic;
using Volcano.Game;

namespace Volcano.Engine
{
    internal class RandomEngine : IEngine
    {
        private static Random _random = new Random();

        private OpeningBook _book;

        public RandomEngine()
            : this("")
        {
        }

        public RandomEngine(OpeningBook book)
        {
            _book = book;
        }

        public RandomEngine(string book)
        {
            if (!string.IsNullOrEmpty(book))
            {
                _book = new OpeningBook(book);
            }
        }

        public SearchResult GetBestMove(Board state, int maxSeconds, EngineCancellationToken token)
        {
            if (_book != null)
            {
                var bookMove = _book.GetMove(state.Transcript);
                if (bookMove >= 0)
                {
                    return new SearchResult
                    {
                        BestMove = bookMove
                    };
                }
            }

            List<int> moves = state.GetMoves();
            return new SearchResult
            {
                BestMove = moves[_random.Next(moves.Count)]
            };
        }
    }
}
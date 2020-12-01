using System.Collections.Generic;
using Volcano.Game;

namespace Volcano.Engine
{
    internal class MonteCarloPlayoutEngine : MonteCarloTreeSearchEngine
    {
        protected override List<int> GetMoves(Board position)
        {
            var candidates = position.GetMoves();

            // Add all 3-tile-away moves as duplicates to increase their likliness of being chosen
            for (int i = 0; i < 80; i++)
            {
                if ((position.Tiles[i] > 0 && position.Player == Player.One) || position.Tiles[i] < 0 && position.Player == Player.Two)
                {
                    foreach (var tile in Constants.ThreeAway[i])
                    {
                        if (position.Tiles[tile] == 0)
                        {
                            candidates.Add(tile);
                        }
                    }
                }
            }

            return candidates;
        }
    }
}
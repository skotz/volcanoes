using System.Collections.Generic;
using Volcano.Game;

namespace Volcano.Search
{
    internal class NearbyMoveFilter : IMoveFilter
    {
        public NearbyMoveFilter()
        {
        }

        public List<int> GetMoves(Board state)
        {
            if (state.Turn == 1)
            {
                return [Constants.TileIndexes["N26"]];
            }

            var candidates = new List<int>();

            if (state.Turn == 2)
            {
                for (var i = 0; i < 80; i++)
                {
                    if (state.Tiles[i] > 0)
                    {
                        foreach (var m in Constants.PointNeighbors[i])
                        {
                            candidates.Add(m);
                        }

                        break;
                    }
                }
            }
            else if (state.Turn <= 12)
            {
                var done = new bool[80];

                for (var i = 0; i < 80; i++)
                {
                    if ((state.Tiles[i] < 0 && state.Player == Player.One) || (state.Tiles[i] > 0 && state.Player == Player.Two))
                    {
                        foreach (var m in Constants.PointNeighbors[i])
                        {
                            if (!done[i] && state.Tiles[i] == 0)
                            {
                                candidates.Add(m);
                                done[i] = true;
                            }
                        }
                    }
                    else if ((state.Tiles[i] > 0 && state.Player == Player.One) || (state.Tiles[i] < 0 && state.Player == Player.Two))
                    {
                        foreach (var m in Constants.ThreeAway[i])
                        {
                            if (!done[i] && state.Tiles[i] == 0)
                            {
                                candidates.Add(m);
                                done[i] = true;
                            }
                        }
                    }
                }
            }

            return candidates.Count > 0 ? candidates : state.GetMoves();
        }
    }
}
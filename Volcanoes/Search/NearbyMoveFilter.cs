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
            if (state.Turn == 1) // blue 1
            {
                return [Constants.TileIndexes["N26"]];
            }

            var candidates = new List<int>();
            var done = new bool[80];

            if (state.Turn == 2) // orange 1
            {
                for (var i = 0; i < 80; i++)
                {
                    // point neighbor of opponent
                    if (state.Tiles[i] > 0)
                    {
                        foreach (var m in Constants.PointNeighbors[i])
                        {
                            candidates.Add(m);
                        }

                        // only one opponent
                        break;
                    }
                }
            }
            else if (state.Turn == 4) // orange 2
            {
                // orange's previous move
                var first = 0;
                for (var i = 0; i < 80; i++)
                {
                    if (state.Tiles[i] < 0)
                    {
                        first = i;
                        break;
                    }
                }

                for (var i = 0; i < 80; i++)
                {
                    // point neighbor of opponent that's also 3 away from our first tile
                    if (state.Tiles[i] > 0)
                    {
                        foreach (var m in Constants.PointNeighbors[i])
                        {
                            if (Constants.Distances[m][first] == 3 && state.Tiles[m] == 0)
                            {
                                candidates.Add(m);
                            }
                        }

                        // only one opponent
                        break;
                    }
                }
            }
            else if (state.Turn == 5) // blue 2
            {
                // my previous move
                var first = 0;
                for (var i = 0; i < 80; i++)
                {
                    if (state.Tiles[i] > 0)
                    {
                        first = i;
                        break;
                    }
                }

                for (var i = 0; i < 80; i++)
                {
                    // point neighbor of opponent that's at least 3 away from our first tile and not directly adjacent to any other tile
                    if (state.Tiles[i] < 0)
                    {
                        foreach (var m in Constants.PointNeighbors[i])
                        {
                            var ok = true;
                            foreach (var n in Constants.AdjacentIndexes[m])
                            {
                                if (state.Tiles[n] != 0)
                                {
                                    ok = false;
                                    break;
                                }
                            }

                            if (ok && Constants.Distances[m][first] >= 3 && state.Tiles[m] == 0 && !done[m])
                            {
                                candidates.Add(m);
                                done[m] = true;
                            }
                        }
                    }
                }
            }

            if (candidates.Count == 0 && state.Turn <= 14)
            {
                var distance = 3;
                var avoid = true;

                if (state.Turn >= 13)
                {
                    distance = 2;
                }
                if (state.Turn >= 19)
                {
                    distance = 1;
                    avoid = false;
                }

                // my tiles
                var mine = new List<int>();
                for (var i = 0; i < 80; i++)
                {
                    if (state.Player == Player.One && state.Tiles[i] > 0)
                    {
                        mine.Add(i);
                    }
                    else if (state.Player == Player.Two && state.Tiles[i] < 0)
                    {
                        mine.Add(i);
                    }
                }

                for (var i = 0; i < 80; i++)
                {
                    // point neighbor of opponent that's at least 3 away from our first tile and not directly adjacent to any other tile
                    if ((state.Player == Player.One && state.Tiles[i] < 0) || (state.Player == Player.Two && state.Tiles[i] > 0))
                    {
                        foreach (var m in Constants.PointNeighbors[i])
                        {
                            if (!done[m])
                            {
                                var ok = true;

                                if (avoid)
                                {
                                    foreach (var n in Constants.AdjacentIndexes[m])
                                    {
                                        if (state.Tiles[n] != 0)
                                        {
                                            ok = false;
                                            break;
                                        }
                                    }
                                }

                                if (ok && distance > 1)
                                {
                                    foreach (var t in mine)
                                    {
                                        if (Constants.Distances[m][t] < distance)
                                        {
                                            ok = false;
                                            break;
                                        }
                                    }
                                }

                                if (ok && state.Tiles[m] == 0)
                                {
                                    candidates.Add(m);
                                    done[m] = true;
                                }
                            }
                        }
                    }
                }
            }

            //if (candidates.Count == 0 && state.Turn <= 12)
            //{
            //    for (var i = 0; i < 80; i++)
            //    {
            //        // point neighbor of opponent that's not an immediate neighbor of any other tile
            //        if ((state.Tiles[i] < 0 && state.Player == Player.One) || (state.Tiles[i] > 0 && state.Player == Player.Two))
            //        {
            //            foreach (var m in Constants.PointNeighbors[i])
            //            {
            //                foreach (var a in Constants.AdjacentIndexes[m])
            //                {
            //                    if (state.Tiles[a] == 0 && state.Tiles[m] == 0 && !done[m])
            //                    {
            //                        candidates.Add(m);
            //                        done[m] = true;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            //if (candidates.Count == 0 && state.Turn <= 12)
            //{
            //    var pathfinder = new TotalPathFinder();
            //    var paths = pathfinder.GetAllPaths(state);
            //    var done = new bool[80];

            //    var myBest = paths.MyPaths.OrderBy(x => x.Distance).FirstOrDefault();
            //    var oppBest = paths.OpponentPaths.OrderBy(x => x.Distance).FirstOrDefault();

            //    if (myBest != null)
            //    {
            //        foreach (var m in myBest.Path)
            //        {
            //            if (!done[m] && state.Tiles[m] == 0)
            //            {
            //                candidates.Add(m);
            //            }
            //        }
            //    }
            //    if (oppBest != null)
            //    {
            //        foreach (var m in oppBest.Path)
            //        {
            //            if (!done[m] && state.Tiles[m] == 0)
            //            {
            //                candidates.Add(m);
            //            }
            //        }
            //    }
            //}

            //if (candidates.Count == 0 && state.Turn <= 12)
            //{
            //    var done = new bool[80];

            //    for (var i = 0; i < 80; i++)
            //    {
            //        if ((state.Tiles[i] < 0 && state.Player == Player.One) || (state.Tiles[i] > 0 && state.Player == Player.Two))
            //        {
            //            foreach (var m in Constants.PointNeighbors[i])
            //            {
            //                if (!done[m] && state.Tiles[m] == 0)
            //                {
            //                    candidates.Add(m);
            //                    done[m] = true;
            //                }
            //            }
            //        }
            //        else if ((state.Tiles[i] > 0 && state.Player == Player.One) || (state.Tiles[i] < 0 && state.Player == Player.Two))
            //        {
            //            foreach (var m in Constants.ThreeAway[i])
            //            {
            //                if (!done[m] && state.Tiles[m] == 0)
            //                {
            //                    candidates.Add(m);
            //                    done[m] = true;
            //                }
            //            }
            //        }
            //    }
            //}

            return candidates.Count > 0 ? candidates : state.GetMoves();
        }
    }
}
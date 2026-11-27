using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Volcano.Game
{
    public class SymmetryGenerator
    {
        public static List<int[]> GeneratePermutations()
        {
            // Use your pre-existing neighbor list directly
            int[][] neighbors = Constants.AdjacentIndexes;
            int numTiles = neighbors.Length; // Expects 80

            var validPermutations = new List<int[]>();

            // Set up our static seed: Tile 0 and its 3 neighbors
            int sourceTile = 0;
            int[] srcNeighbors = neighbors[sourceTile];

            // Map Tile 0 to every possible target on the board
            for (int targetTile = 0; targetTile < numTiles; targetTile++)
            {
                int[] tgtNeighbors = neighbors[targetTile];
                var targetNeighborPermutations = GetPermutations3(tgtNeighbors);

                foreach (var orientedTgtNeighbors in targetNeighborPermutations)
                {
                    int[] perm = new int[numTiles];
                    Array.Fill(perm, -1);
                    bool[] usedInTarget = new bool[numTiles];

                    // Seed the mapping configuration
                    perm[sourceTile] = targetTile;
                    usedInTarget[targetTile] = true;

                    for (int i = 0; i < 3; i++)
                    {
                        perm[srcNeighbors[i]] = orientedTgtNeighbors[i];
                        usedInTarget[orientedTgtNeighbors[i]] = true;
                    }

                    // Verify the initial 4-tile cluster connectivity matches structurally
                    bool initialSetupValid = true;
                    for (int i = 0; i < 3; i++)
                    {
                        int srcN = srcNeighbors[i];
                        int nextSrcN = srcNeighbors[(i + 1) % 3];
                        if (neighbors[srcN].Contains(nextSrcN))
                        {
                            if (!neighbors[perm[srcN]].Contains(perm[nextSrcN]))
                            {
                                initialSetupValid = false;
                                break;
                            }
                        }
                    }

                    if (!initialSetupValid) continue;

                    // Begin adaptive topological backtracking
                    FindSymmetriesAdaptive(neighbors, numTiles, perm, usedInTarget, validPermutations, 4);
                }
            }

            // De-duplicate the final symmetry configurations
            return validPermutations
                .GroupBy(p => string.Join(",", p))
                .Select(g => g.First())
                .ToList();
        }

        private static void FindSymmetriesAdaptive(
            int[][] neighbors,
            int numTiles,
            int[] perm,
            bool[] usedInTarget,
            List<int[]> results,
            int mappedCount)
        {
            // Base Case: Every tile has been successfully mapped without conflict
            if (mappedCount == numTiles)
            {
                int[] validPerm = new int[numTiles];
                Array.Copy(perm, validPerm, numTiles);
                results.Add(validPerm);
                return;
            }

            // Dynamically find the best unmapped source tile (the one with the most mapped neighbors)
            int bestSrcTile = -1;
            int maxMappedNeighbors = -1;
            List<int> bestSrcTileMappedNeighbors = null;

            for (int i = 0; i < numTiles; i++)
            {
                if (perm[i] != -1) continue;

                var mappedNeighborsOfI = new List<int>();
                foreach (int n in neighbors[i])
                {
                    if (perm[n] != -1) mappedNeighborsOfI.Add(n);
                }

                if (mappedNeighborsOfI.Count > maxMappedNeighbors)
                {
                    maxMappedNeighbors = mappedNeighborsOfI.Count;
                    bestSrcTile = i;
                    bestSrcTileMappedNeighbors = mappedNeighborsOfI;
                }
            }

            if (bestSrcTile == -1 || maxMappedNeighbors == 0) return;

            // Candidate targets must be chosen from the neighbors of an already-mapped neighbor
            int firstMappedNeighbor = bestSrcTileMappedNeighbors[0];
            int firstMappedNeighborTarget = perm[firstMappedNeighbor];
            int[] targetOptions = neighbors[firstMappedNeighborTarget];

            foreach (int targetOption in targetOptions)
            {
                if (usedInTarget[targetOption]) continue;

                perm[bestSrcTile] = targetOption;
                usedInTarget[targetOption] = true;

                // Structural Validation Step
                if (VerifyTileConnections(neighbors, bestSrcTile, perm))
                {
                    FindSymmetriesAdaptive(neighbors, numTiles, perm, usedInTarget, results, mappedCount + 1);
                }

                // Backtrack
                perm[bestSrcTile] = -1;
                usedInTarget[targetOption] = false;
            }
        }

        private static bool VerifyTileConnections(int[][] neighbors, int srcTile, int[] perm)
        {
            int tgtTile = perm[srcTile];
            if (tgtTile == -1) return true;

            foreach (int srcNeighbor in neighbors[srcTile])
            {
                int tgtNeighbor = perm[srcNeighbor];
                if (tgtNeighbor != -1)
                {
                    if (!neighbors[tgtTile].Contains(tgtNeighbor))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static List<int[]> GetPermutations3(int[] arr)
        {
            return new List<int[]>
            {
                new int[] { arr[0], arr[1], arr[2] },
                new int[] { arr[0], arr[2], arr[1] },
                new int[] { arr[1], arr[0], arr[2] },
                new int[] { arr[1], arr[2], arr[0] },
                new int[] { arr[2], arr[0], arr[1] },
                new int[] { arr[2], arr[1], arr[0] }
            };
        }

        public static void Generate()
        {
            var permutations = GeneratePermutations();

            var ic = new StringBuilder();
            ic.AppendLine("        private static readonly int[,] _indexToCanonical =");
            ic.AppendLine("        {");

            var ci = new StringBuilder();
            ci.AppendLine("        private static readonly int[,] _canonicalToIndex =");
            ci.AppendLine("        {");

            for (var p = 0; p < permutations.Count; p++)
            {
                ic.Append($"            {{ ");
                ci.Append($"            {{ ");

                var inverse = ComputeInverse(permutations[p]);

                for (var i = 0; i < 80; i++)
                {
                    var forward = permutations[p][i];
                    var backward = inverse[i];

                    ic.Append(forward + ", ");
                    ci.Append(backward + ", ");

                    if (inverse[forward] != i)
                    {
                        throw new Exception("bug");
                    }
                }

                ic.AppendLine($"}},");
                ci.AppendLine($"}},");
            }

            ic.AppendLine("        };");
            ci.AppendLine("        };");

            File.WriteAllLines("canonical.txt", [ic.ToString(), ci.ToString()]);
        }

        private static int[] ComputeInverse(int[] forwardPerm)
        {
            int[] inverse = new int[forwardPerm.Length];
            for (int i = 0; i < forwardPerm.Length; i++)
            {
                int x = forwardPerm[i];
                inverse[x] = i;
            }
            return inverse;
        }
    }
}
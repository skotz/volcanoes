using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using TorchSharp;
using Volcano.Game;

namespace Volcano.Neural
{
    internal static class EncoderBenchmark
    {
        public const int PositionCount = 10000;

        public static string Run(string outputPath)
        {
            var random = new Random(42);
            var positions = Enumerable.Range(0, PositionCount).Select(_ => RandomBoard(random)).ToList();

            var encoders = typeof(IBoardEncoder).Assembly.GetTypes()
                .Where(t => typeof(IBoardEncoder).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && !t.IsInterface)
                .OrderBy(t => t.Name)
                .ToList();

            var lines = new List<string> { "Encoder,ExecutionTimeSeconds,EncodingsPerSecond" };

            foreach (var type in encoders)
            {
                var encoder = (IBoardEncoder)Activator.CreateInstance(type);

                encoder.Encode(positions.Take(10).ToList(), torch.CPU);

                var stopwatch = Stopwatch.StartNew();
                foreach (var position in positions)
                {
                    encoder.Encode(position, torch.CPU);
                }
                stopwatch.Stop();

                var seconds = stopwatch.Elapsed.TotalSeconds;
                var perSecond = PositionCount / seconds;
                lines.Add(string.Join(",", type.Name, seconds.ToString("F4", CultureInfo.InvariantCulture), perSecond.ToString("F2", CultureInfo.InvariantCulture)));
            }

            File.WriteAllLines(outputPath, lines);
            return outputPath;
        }

        private static Board RandomBoard(Random random)
        {
            var board = new Board();
            for (var i = 0; i < 80; i++)
            {
                board.Tiles[i] = random.Next(-4, 5);
            }
            return board;
        }
    }
}
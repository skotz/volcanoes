using System;
using System.Linq;
#if WINDOWS
using System.Windows.Forms;
#else
using Volcano.Engine;
using Volcano.Neural;
using Volcano.Search;
#endif

namespace Volcano
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
#if WINDOWS
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GameForm(args));
        }
#else
        private static void Main(string[] args)
        {
            if (!args.Any(a => string.Equals(a, "train", StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("This headless build only supports training. Run with the 'train' argument.");
                return;
            }

            AlphaZeroLog.WriteLine = Console.WriteLine;
            StaticWinSearch.Enabled = true;
            StaticBoardSlim.Enabled = true;

            var engine = new AlphaZeroGraphEngine(true);
            engine.OnDebug += (sender, status) => Console.WriteLine(status?.Summary);
            engine.Train();
        }
#endif
    }
}
using System;
using System.IO;
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
            if (args.Any(a => string.Equals(a, "book", StringComparison.OrdinalIgnoreCase)))
            {
                BuildBook();
                return;
            }

            if (!args.Any(a => string.Equals(a, "train", StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("This headless build only supports training. Run with the 'train' or 'book' argument.");
                return;
            }

            AlphaZeroLog.WriteLine = Console.WriteLine;
            StaticWinSearch.Enabled = true;
            StaticBoardSlim.Enabled = true;

            var engine = new AlphaZeroGraphEngine(true);
            engine.OnDebug += (sender, status) => Console.WriteLine(status?.Summary);
            engine.Train();
        }

        private static void BuildBook()
        {
            const string bookLocation = "linux-book.dat";

            AlphaZeroLog.WriteLine = Console.WriteLine;
            StaticWinSearch.Enabled = true;
            StaticBoardSlim.Enabled = true;

            var book = new OpeningBook(bookLocation);
            book.OnStatusUpdate += (completed, total, message) =>
            {
                Console.WriteLine(string.IsNullOrEmpty(message) ? completed + " / " + total : message);
            };

            book.Generate(50, 100000, 100000, false, true);

            File.Copy(bookLocation, bookLocation + "." + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bak");

            book.Extend(50, 100000, 100000, false, true, null);

            Console.WriteLine("done");
        }
#endif
    }
}
using System.Collections.Concurrent;
using Volcano.Game;

namespace Volcano.Search
{
    /// <summary>
    /// Used for training. This should be disabled for any normal play since it's shared and would benefit your opponent in unpredictable ways.
    /// </summary>
    internal class StaticWinCache
    {
        public static bool Enabled = false;

        public static ConcurrentDictionary<long, Player> Winners = new ConcurrentDictionary<long, Player>();
    }

    internal class StaticWinSearch
    {
        public static bool Enabled = false;
    }
}
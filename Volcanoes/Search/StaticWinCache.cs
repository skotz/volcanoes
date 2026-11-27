using LiteDB;
using System.Collections.Concurrent;
using System.Linq;
using Volcano.Game;
using static Volcano.Engine.MonteCarloTreeSearchEngine;

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

    internal class StaticBoardSlim
    {
        public static bool Enabled = false;
    }

    internal class StaticRolloutDatabase
    {
        private const string _location = "G:\\volcanoes\\rollout.db";

        public static bool Enabled = false;

        public static RolloutRecord Find(long hash)
        {
            using (var db = new LiteDatabase(_location))
            {
                var collection = db.GetCollection<RolloutRecord>("rollouts");
                return collection.FindById(hash);
            }
        }

        public static void Update(MonteCarloTreeSearchNode node)
        {
            var rollout = new RolloutRecord
            {
                Hash = node.hash,
                Wins = new double[80],
                Visits = new double[80],
            };

            foreach (var child in node.Children)
            {
                if (child.Move >= 0 && child.Move < 80)
                {
                    rollout.Wins[child.Move] = child.Wins;
                    rollout.Visits[child.Move] = child.Visits;
                }
            }

            Update(rollout);
        }

        public static void Update(RolloutRecord rollout)
        {
            using (var db = new LiteDatabase(_location))
            {
                var collection = db.GetCollection<RolloutRecord>("rollouts");

                var existing = collection.FindById(rollout.Hash);
                var allowUpsert = true;
                if (existing != null)
                {
                    allowUpsert = rollout.Visits.Sum() > existing.Visits.Sum();
                }

                if (allowUpsert)
                {
                    collection.Upsert(rollout);
                }
            }
        }
    }

    internal class RolloutRecord
    {
        [BsonId]
        public long Hash { get; set; }

        public double[] Wins { get; set; }
        public double[] Visits { get; set; }
    }
}
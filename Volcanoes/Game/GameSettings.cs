using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Game
{
    class GameSettings
    {
        public int MaxMagmaChamberLevel { get; set; } = 4;
        public int MaxVolcanoLevel { get; set; } = 8;

        public int EruptOverflowEmptyTileAmount { get; set; } = 1;
        public int EruptOverflowFriendlyTileAmount { get; set; } = 1;
        public int EruptOverflowEnemyTileAmount { get; set; } = -1;
        public bool EruptOverflowAllowCapture { get; set; } = true;

        public bool AllowMagmaChamberCaptures { get; set; } = false;
        public bool AllowVolcanoCaptures { get; set; } = false;

        public int TournamentAdjudicateMaxTurns { get; set; } = 500;
        public int TournamentAdjudicateMaxSeconds { get; set; } = 60 * 30;

        [JsonIgnore]
        public string CustomSettingsFile { get; set; }
        public string CustomSettingsTitle { get; set; } = "";

        public static GameSettings Load(string file)
        {
            try
            {
                string json = File.ReadAllText(file);
                return JsonConvert.DeserializeObject<GameSettings>(json);
            }
            catch (Exception ex)
            {
                throw new IOException("Could not load game settings from \"" + file + "\"", ex);
            }
        }

        public static GameSettings LoadOrDefault(string file)
        {
            if (File.Exists(file))
            {
                var json = File.ReadAllText(file);
                var settings = JsonConvert.DeserializeObject<GameSettings>(json);

                if (settings != null)
                {
                    settings.CustomSettingsFile = file;
                    return settings;
                }
            }

            return new GameSettings();
        }

        public void Save(string file)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(file, json);
        }
    }
}

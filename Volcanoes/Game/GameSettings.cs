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
        public int MaxMagmaChamberLevel = 4;
        public int MaxVolcanoLevel = 8;

        public int EruptOverflowEmptyTileAmount = 1;
        public int EruptOverflowFriendlyTileAmount = 1;
        public int EruptOverflowEnemyTileAmount = -1;
        public bool EruptOverflowAllowCapture = true;

        public bool AllowMagmaChamberCaptures = false;
        public bool AllowVolcanoCaptures = false;

        public int TournamentAdjudicateMaxTurns = 500;
        public int TournamentAdjudicateMaxSeconds = 60 * 30;

        [JsonIgnore]
        public string CustomSettingsFile;
        public string CustomSettingsTitle = "";

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

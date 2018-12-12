using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volcano.Interface
{
    class GameGraphicsSettings
    {
        public int TileSize { get; set; } = 80;
        public int TileSpacing { get; set; } = 8;

        public int TileWidth { get { return TileSize; } }
        public int TileHeight { get { return (int)(TileWidth * Math.Sqrt(3) / 2); } }
        public int TileHorizontalSpacing { get { return (int)(TileSpacing * Math.Sqrt(3) / 2); } }

        public int FontSize { get; set; } = 14;

        public Color PlayerOneTileColor { get; set; } = Color.Blue;
        public Color PlayerTwoTileColor { get; set; } = Color.Orange;
        public Color EmptyTileColor { get; set; } = Color.Gray;
        public Color BackgroundColor { get; set; } = Color.White;
        public Color HoverTileBorderColor { get; set; } = Color.Magenta;
        public Color HoverAdjacentTileBorderColor { get; set; } = Color.LightGray;
        public Color HoverAntipodeTileBorderColor { get; set; } = Color.Lime;

        public int IdealPanelWidth { get { return TileWidth * 11 + TileHorizontalSpacing * 20 + TileSpacing * 2; } }
        public int IdealPanelHeight { get { return TileHeight * 6 + TileHorizontalSpacing * 4 + TileSpacing * 7; } }

        private GameGraphicsSettings()
        {
        }

        public static GameGraphicsSettings Default
        {
            get
            {
                return new GameGraphicsSettings();
            }
        }

        public static GameGraphicsSettings Load(string file)
        {
            try
            {
                string json = File.ReadAllText(file);
                return JsonConvert.DeserializeObject<GameGraphicsSettings>(json, new JsonColorConverter());
            }
            catch (Exception ex)
            {
                throw new IOException("Could not load game graphic settings from \"" + file + "\"", ex);
            }
        }

        public void Save(string file)
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonColorConverter());
            File.WriteAllText(file, json);
        }
    }
}

﻿using Newtonsoft.Json;
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

        public int MainFontSize { get; set; } = 14;
        public int SubTextFontSize { get; set; } = 8;

        public Color PlayerOneVolcanoTileColor { get; set; } = Color.FromArgb(18, 11, 134);
        public Color PlayerOneMagmaChamberTileColor { get; set; } = Color.FromArgb(39, 29, 211);
        public Color PlayerTwoVolcanoTileColor { get; set; } = Color.FromArgb(192, 114, 0);
        public Color PlayerTwoMagmaChamberTileColor { get; set; } = Color.FromArgb(255, 151, 0);

        public Color EmptyTileColor { get; set; } = Color.Gray;
        public Color BackgroundColor { get; set; } = Color.White;
        public Color HoverTileBorderColor { get; set; } = Color.FromArgb(230, 0, 113);
        public Color HoverAdjacentTileBorderColor { get; set; } = Color.LightGray;
        public Color HoverAntipodeTileBorderColor { get; set; } = Color.FromArgb(0, 219, 48);

        public int IdealPanelWidth { get { return TileWidth * 11 + TileHorizontalSpacing * 20 + TileSpacing * 2; } }
        public int IdealPanelHeight { get { return TileHeight * 6 + TileHorizontalSpacing * 4 + TileSpacing * 7; } }

        public bool ShowTileNames { get; set; } = false;

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

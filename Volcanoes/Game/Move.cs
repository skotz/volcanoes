namespace Volcano.Game
{
    class Move
    {
        public int Location { get; set; }
        public bool Addition { get; set; }
        public string Tile { get; set; }

        public Move(int location, bool addition)
        {
            Location = location;
            Addition = addition;
            Tile = location >= 0 && location < Constants.TileNames.Length ? Constants.TileNames[location] : "";
        }
    }
}
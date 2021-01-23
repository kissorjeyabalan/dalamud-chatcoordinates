namespace ChatCoordinates.Models
{
    public class TerritoryDetail
    {
        public uint TerritoryType { get; set; }
        public uint MapId { get; set; }
        public ushort MapSizeFactor { get; set; }
        public string PlaceName { get; set; }
    }
}
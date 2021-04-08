namespace ChatCoordinates.Models
{
    public class TerritoryDetail
    {
        public string Name { get; set; } = null!;
        public uint TerritoryType { get; set; }
        public uint MapId { get; set; }
        public ushort SizeFactor { get; set; }
    }
}
namespace ChatCoordinates.Models
{
    public class Coordinate
    {
        public float NiceX { get; set; }
        public float NiceY { get; set; }

        public string? Zone { get; set; }
        public bool ZoneSpecified { get; set; } = false;

        public bool Teleport { get; set; } = false;
        public bool UseTicket { get; set; } = false;
        public TerritoryDetail? TerritoryDetail { get; set; }

        public bool HasCoordinates()
        {
            return NiceX > float.MinValue && NiceY > float.MinValue;
        }
    }
}
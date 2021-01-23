using System;
using Lumina.Excel.GeneratedSheets;

namespace ChatCoordinates.Models
{
    public class AetheryteDetail
    {
        public Aetheryte Aetheryte { get; set; }
        public MapMarker MapMarker { get; set; }

        public float Distance(Tuple<float, float> coordinates, ushort mapSizeFactor)
        {
            var diffX = CoordinateHelper.MapMarkerToMapPos(MapMarker.X, mapSizeFactor) - CoordinateHelper.ConvertMapCoordinateToRawPosition(coordinates.Item1, mapSizeFactor);
            var diffY = CoordinateHelper.MapMarkerToMapPos(MapMarker.Y, mapSizeFactor) - CoordinateHelper.ConvertMapCoordinateToRawPosition(coordinates.Item2, mapSizeFactor);
            return (float) Math.Sqrt(diffX * diffX + diffY * diffY);
        }
    }
}
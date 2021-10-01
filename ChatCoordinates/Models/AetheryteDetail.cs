using System;
using System.Numerics;

namespace ChatCoordinates.Models
{
    public class AetheryteDetail
    {
        public string? Name { get; set; } = null!;
        public ushort SizeFactor { get; set; }
        public Vector2 RawCoordinates { get; set; }

        private float MapMarkerToMapCoord(float pos, float scale)
        {
            var num = scale / 100f;
            var rawPos = (int) ((float) (pos - 1024.0) / num * 1000f);
            return RawCoordToMapCoord(rawPos, scale);
        }

        private float RawCoordToMapCoord(int pos, float scale)
        {
            var num = scale / 100f;
            return (float) ((pos / 1000f * num + 1024.0) / 2048.0 * 41 / num + 1.0);
        }

        public static int NiceCoordToMapCoord(float pos, float scale)
        {
            var num = scale / 100f;
            return (int) ((float) ((pos - 1.0) * num / 41.0 * 2048.0 - 1024.0) / num * 1000f);
        }

        public float Distance(Coordinate coordinates)
        {
            var aetheryteX = MapMarkerToMapCoord(RawCoordinates.X, SizeFactor);
            var aetheryteY = MapMarkerToMapCoord(RawCoordinates.Y, SizeFactor);
            var mapX = NiceCoordToMapCoord(coordinates.NiceX, SizeFactor);
            var mapY = NiceCoordToMapCoord(coordinates.NiceY, SizeFactor);

            var diffX = aetheryteX - mapX;
            var diffY = aetheryteY - mapY;

            return (float) Math.Sqrt(diffX * diffX + diffY * diffY);
        }
    }
}
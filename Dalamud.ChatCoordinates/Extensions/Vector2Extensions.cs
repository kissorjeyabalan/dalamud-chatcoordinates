using System.Numerics;

namespace ChatCoordinates.Extensions
{
    public static class Vector2Extensions
    {
        public static Vector2 ToRawCoordinates(this Vector2 vector, ushort sizeFactor, float fudge = 0.05f)
        {
            var fudgedX = vector.X + fudge;
            var fudgedY = vector.Y + fudge;
            var num = sizeFactor / 100f;
            
            return new Vector2
            {
                X = (int) ((float) ((fudgedX - 1.0) * num / 41.0 * 2048.0 - 1024.0) / num * 1000f),
                Y = (int) ((float) ((fudgedY - 1.0) * num / 41.0 * 2048.0 - 1024.0) / num * 1000f),
            };
        }
    }
}
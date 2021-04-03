using System;

namespace ChatCoordinates.Extensions
{
    public static class FloatExtensions
    {
        public static int ToRawCoordinates(this float niceCoordinate, ushort sizeFactor, float fudge = 0.05f)
        {
            niceCoordinate += fudge;
            var x = sizeFactor / 100.0f;

            return (int) ((float) ((niceCoordinate - 1.0) * x / 41.0 * 2048.0 - 1024.0) / x * 1000f);
        }

        public static float Sanitize(this float num)
        {
            if (Math.Abs(num - 26.7) < 1)
            {
                num += 0.1f;
            }

            return num;
        }
    }
}
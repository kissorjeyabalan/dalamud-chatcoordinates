using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace ChatCoordinates.Extensions
{
    public static class StringExtensions
    {
        public static Vector2? GetCoordinates(this string str)
        {
            var coordinates = Regex.Matches(str, "(\\d*\\.?\\d*)");
            var xSet = false;
            var ySet = false;
            var x = 0.0f;
            var y = 0.0f;

            foreach (Match coordinate in coordinates)
            {
                if (string.IsNullOrWhiteSpace(coordinate.Value)) continue;
                if (!float.TryParse(coordinate.Value, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out var coord)) continue;

                if (!xSet)
                {
                    x = coord;
                    xSet = true;
                    continue;
                }
                
                if (ySet) continue;
                y = coord;
                ySet = true;
            }

            if (!xSet || !ySet) return null;

            return new Vector2(x, y);
        }
    }
}
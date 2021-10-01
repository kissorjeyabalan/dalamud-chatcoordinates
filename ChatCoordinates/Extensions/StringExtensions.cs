using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ChatCoordinates.Models;

namespace ChatCoordinates.Extensions
{
    public static class StringExtensions
    {
        public static Coordinate ParseCoordinate(this string arg, CCPlugin plugin)
        {
            arg = arg.Trim().Replace("\"", "");

            var coordinates = Regex.Matches(arg, "(\\d*\\.?\\d*)");

            var x = 0.0f;
            var y = 0.0f;

            var xSet = false;
            var ySet = false;
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

            var args = arg.Split(new []{ plugin.Configuration.ZoneDelimiter}, StringSplitOptions.None);
            var zone = args.Last()?.Trim();
            var zoneSpecified = !float.TryParse(zone, out _);

            return new Coordinate
            {
                NiceX = x,
                NiceY = y,
                Zone = zoneSpecified ? zone : null,
                ZoneSpecified = zoneSpecified,
                Teleport = false,
                UseTicket = false,
                TerritoryDetail = zoneSpecified && zone != null && args.Length > 1
                    ? plugin.TerritoryManager.GetByZoneName(zone)
                    : plugin.TerritoryManager.GetByTerritoryType(plugin.ClientState.TerritoryType)
            };
        }
    }
}
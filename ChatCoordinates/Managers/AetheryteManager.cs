using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ChatCoordinates.Models;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace ChatCoordinates.Managers
{
    public class AetheryteManager
    {
        private readonly Dictionary<uint, List<AetheryteDetail>> _aetherytes;
        private readonly IDataManager _data;

        public AetheryteManager(IDataManager data)
        {
            _data = data;
            _aetherytes = LoadAetherytes();
        }

        public AetheryteDetail? GetClosestAetheryte(Coordinate coordinate)
        {
            if (!_aetherytes.TryGetValue(coordinate.TerritoryDetail!.MapId, out var aetherytes)) return null;

            return aetherytes.Aggregate((min, x) =>
                x.Distance(coordinate) < min.Distance(coordinate) ? x : min);
        }

        private Dictionary<uint, List<AetheryteDetail>> LoadAetherytes()
        {

            var mapMarkers = _data.GetSubrowExcelSheet<MapMarker>()
                .SelectMany(m => m).Cast<MapMarker?>()
                .Where(m => m!.Value.DataType == 3).ToList();
            var aetheryteSheet = _data.GetExcelSheet<Aetheryte>();
            var aetherytes = new Dictionary<uint, List<AetheryteDetail>>();

            foreach (var aetheryte in aetheryteSheet)
            {
                if (aetheryte.RowId <= 0) continue;
                if (!aetheryte.IsAetheryte) continue;

                var marker = mapMarkers.FirstOrDefault(x => x!.Value.DataKey.RowId == aetheryte.RowId);
                if (marker == null) continue;

                if (!aetherytes.ContainsKey(aetheryte.Map.Value.RowId))
                    aetherytes.Add(aetheryte.Map.Value.RowId, new List<AetheryteDetail>());

                aetherytes[aetheryte.Map.Value.RowId].Add(new AetheryteDetail
                {
                    Name = aetheryte.PlaceName.Value.Name.ToString(),
                    SizeFactor = aetheryte.Map.Value.SizeFactor,
                    RawCoordinates = new Vector2(marker.Value.X, marker.Value.Y)
                });
            }

            return aetherytes;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ChatCoordinates.Models;
using Dalamud.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace ChatCoordinates.Managers
{
    public class AetheryteManager
    {
        private readonly Dictionary<uint, List<AetheryteDetail>> _aetherytes;
        private readonly DataManager _data;

        public AetheryteManager(DataManager data)
        {
            _data = data;
            _aetherytes = LoadAetherytes();
        }

        public AetheryteDetail? GetClosestAetheryte(Coordinate coordinate)
        {
            if (!_aetherytes.ContainsKey(coordinate.TerritoryDetail!.MapId)) return null;
            var aetherytes = _aetherytes[coordinate.TerritoryDetail!.MapId];

            return aetherytes.Aggregate((min, x) =>
                min == null || x.Distance(coordinate) < min.Distance(coordinate) ? x : min);
        }

        private Dictionary<uint, List<AetheryteDetail>> LoadAetherytes()
        {
            var mapMarkers = _data.GetExcelSheet<MapMarker>()
                .Where(x => x.DataType == 3).ToList();
            var aetheryteSheet = _data.GetExcelSheet<Aetheryte>();
            var aetherytes = new Dictionary<uint, List<AetheryteDetail>>();

            foreach (var aetheryte in aetheryteSheet)
            {
                if (aetheryte.RowId <= 0) continue;
                if (!aetheryte.IsAetheryte) continue;

                var marker = mapMarkers.FirstOrDefault(x => x.DataKey == aetheryte.RowId);
                if (marker == null) continue;

                if (!aetherytes.ContainsKey(aetheryte.Map.Value.RowId))
                    aetherytes.Add(aetheryte.Map.Value.RowId, new List<AetheryteDetail>());

                aetherytes[aetheryte.Map.Value.RowId].Add(new AetheryteDetail
                {
                    Name = aetheryte.PlaceName.Value.Name.RawString,
                    SizeFactor = aetheryte.Map.Value.SizeFactor,
                    RawCoordinates = new Vector2(marker.X, marker.Y)
                });
            }

            return aetherytes;
        }
    }
}
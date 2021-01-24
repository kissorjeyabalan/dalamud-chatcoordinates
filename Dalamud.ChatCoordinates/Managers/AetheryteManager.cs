using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ChatCoordinates.Models;
using Lumina.Excel.GeneratedSheets;

namespace ChatCoordinates.Managers
{
    public class AetheryteManager
    {
        private DalamudWrapper _dalamudWrapper;
        private Dictionary<uint, List<AetheryteDetail>> Aetherytes;

        public AetheryteManager(DalamudWrapper dalamudWrapper)
        {
            _dalamudWrapper = dalamudWrapper;
            Aetherytes = GetAetherytes();
        }

        public AetheryteDetail GetClosestAetheryte(Vector2 niceCoordinates, TerritoryDetail territoryDetail)
        {
            var at = GetAetherytes();
            if (!Aetherytes.ContainsKey(territoryDetail.MapId)) return null;
            var aetherytes = Aetherytes[territoryDetail.MapId];

            return aetherytes.Aggregate((min, x) =>
                min == null || x.Distance(niceCoordinates) < min.Distance(niceCoordinates) ? x : min);
        }

        public Dictionary<uint, List<AetheryteDetail>> GetAetherytes()
        {
            var mapMarkers = _dalamudWrapper.GetExcelSheet<MapMarker>()
                .Where(x => x.DataType == 3).ToList();
            var aetheryteSheet = _dalamudWrapper.GetExcelSheet<Aetheryte>();
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
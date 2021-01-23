using System;
using System.Collections.Generic;
using System.Linq;
using ChatCoordinates.Models;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;

namespace ChatCoordinates.Managers
{
    public class AetheryteManager
    {
        private readonly DalamudPluginInterface _pi;
        private Dictionary<uint, List<AetheryteDetail>> Aetherytes => GetAetherytes();

        public AetheryteManager(DalamudPluginInterface dalamudPluginInterface)
        {
            _pi = dalamudPluginInterface;
        }

        public AetheryteDetail GetClosestAetheryte(Tuple<float, float> coordinates, TerritoryDetail territoryDetail)
        {
            if (!Aetherytes.ContainsKey(territoryDetail.MapId)) return null;
            var aetherytes = Aetherytes[territoryDetail.MapId];

            var closest = aetherytes.Aggregate((min, x) =>
                min == null || x.Distance(coordinates, territoryDetail.MapSizeFactor) <
                min.Distance(coordinates, territoryDetail.MapSizeFactor)
                    ? x
                    : min);
            return closest;
        }


        private Dictionary<uint, List<AetheryteDetail>> GetAetherytes()
        {
            var mapMarkers = _pi.Data.GetExcelSheet<MapMarker>().Where(x => x.DataType == 3).ToList();
            var aetheryteSheet = _pi.Data.GetExcelSheet<Aetheryte>();
            var aetherytes = new Dictionary<uint, List<AetheryteDetail>>();
            foreach (var aetheryte in aetheryteSheet)
            {
                if (aetheryte.RowId <= 0) continue;
                if (!aetheryte.IsAetheryte) continue;

                var marker = mapMarkers.FirstOrDefault(x => x.DataKey == aetheryte.RowId);
                if (marker == null) continue;

                if (aetherytes.ContainsKey(aetheryte.Map.Value.RowId))
                    aetherytes[aetheryte.Map.Value.RowId].Add(new AetheryteDetail
                    {
                        Aetheryte = aetheryte,
                        MapMarker = marker
                    });
                else
                    aetherytes.Add(aetheryte.Map.Value.RowId, new List<AetheryteDetail>
                    {
                        new AetheryteDetail
                        {
                            Aetheryte = aetheryte,
                            MapMarker = marker
                        }
                    });
            }

            return aetherytes;
        }
    }
}
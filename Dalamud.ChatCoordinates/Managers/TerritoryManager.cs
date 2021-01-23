using System;
using System.Collections.Generic;
using System.Linq;
using ChatCoordinates.Models;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;

namespace ChatCoordinates.Managers
{
    public class TerritoryManager
    {
        private readonly DalamudPluginInterface _pi;
        private IEnumerable<TerritoryDetail> TerritoryDetails => GetTerritoryDetails();

        public TerritoryManager(DalamudPluginInterface dalamudPluginInterface)
        {
            _pi = dalamudPluginInterface;
        }

        public TerritoryDetail GetTerritoryDetailsByPlaceName(string placeName, bool matchPartial = true)
        {
            var territoryDetail = TerritoryDetails.FirstOrDefault(x =>
                x.PlaceName.Equals(placeName, StringComparison.OrdinalIgnoreCase) ||
                matchPartial && x.PlaceName.ToUpper().Contains(placeName.ToUpper()));
            return territoryDetail;
        }

        public IEnumerable<TerritoryDetail> GetTerritoryDetails()
        {
            return (from territoryType in _pi.Data.GetExcelSheet<TerritoryType>()
                let type = territoryType.Bg.RawString.Split('/')
                where type.Length >= 3
                where type[2] == "twn" || type[2] == "fld" || type[2] == "hou"
                where !string.IsNullOrWhiteSpace(territoryType.Map.Value.PlaceName.Value.Name)
                select new TerritoryDetail
                {
                    TerritoryType = territoryType.RowId, MapId = territoryType.Map.Value.RowId,
                    MapSizeFactor = territoryType.Map.Value.SizeFactor,
                    PlaceName = territoryType.Map.Value.PlaceName.Value.Name,
                }).ToList();
        }
    }
}
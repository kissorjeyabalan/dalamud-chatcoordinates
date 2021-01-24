using System;
using System.Collections.Generic;
using System.Linq;
using ChatCoordinates.Models;
using Lumina.Excel.GeneratedSheets;

namespace ChatCoordinates.Managers
{
    public class TerritoryManager
    {
        private DalamudWrapper _dalamudPlugin;
        private IEnumerable<TerritoryDetail> TerritoryDetails;

        public TerritoryManager(DalamudWrapper dalamudPlugin)
        {
            _dalamudPlugin = dalamudPlugin;
            TerritoryDetails = GetTerritoryDetails();
        }

        public TerritoryDetail GetTerritoryDetailsByZoneName(string zone, bool matchPartial = true)
        {
            var territoryDetail = TerritoryDetails.FirstOrDefault(x =>
                x.Name.Equals(zone, StringComparison.OrdinalIgnoreCase) ||
                matchPartial && x.Name.ToUpper().Contains(zone.ToUpper()));
            return territoryDetail;
        }

        private IEnumerable<TerritoryDetail> GetTerritoryDetails()
        {
            return (from territoryType in _dalamudPlugin.GetExcelSheet<TerritoryType>()
                let type = territoryType.Bg.RawString.Split('/')
                where type.Length >= 3
                where type[2] == "twn" || type[2] == "fld" || type[2] == "hou"
                where !string.IsNullOrWhiteSpace(territoryType.Map.Value.PlaceName.Value.Name)
                select new TerritoryDetail
                {
                    TerritoryType = territoryType.RowId, 
                    MapId = territoryType.Map.Value.RowId,
                    SizeFactor = territoryType.Map.Value.SizeFactor,
                    Name = territoryType.Map.Value.PlaceName.Value.Name,
                }).ToList();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using ChatCoordinates.Models;
using Dalamud.Plugin.Services;
using Lumina.Excel.GeneratedSheets;

namespace ChatCoordinates.Managers
{
    public class TerritoryManager
    {
        private readonly IDataManager _data;
        private readonly IEnumerable<TerritoryDetail> _territoryDetails;

        public TerritoryManager(IDataManager data)
        {
            _data = data;
            _territoryDetails = LoadTerritoryDetails();
        }

        public TerritoryDetail? GetByZoneName(string zone, bool matchPartial = true)
        {
            if (!_territoryDetails.Any()) LoadTerritoryDetails();

            var territoryDetails =
                _territoryDetails
                    .Where(x => x.Name.Equals(zone, StringComparison.OrdinalIgnoreCase) ||
                                matchPartial && x.Name.ToLower().Contains(zone.ToLower())).OrderBy(x => x.Name.Length);

            var territoryDetail = territoryDetails.FirstOrDefault();

            return territoryDetail!;
        }

        private IEnumerable<TerritoryDetail> LoadTerritoryDetails()
        {
            return (from territoryType in _data.GetExcelSheet<TerritoryType>()
                let type = territoryType.Bg.RawString.Split('/')
                where type.Length >= 3
                where type[2] == "twn" || type[2] == "fld" || type[2] == "hou"
                where !string.IsNullOrWhiteSpace(territoryType.Map.Value.PlaceName.Value.Name)
                select new TerritoryDetail
                {
                    TerritoryType = territoryType.RowId,
                    MapId = territoryType.Map.Value.RowId,
                    SizeFactor = territoryType.Map.Value.SizeFactor,
                    Name = territoryType.Map.Value.PlaceName.Value.Name
                }).ToList();
        }

        public TerritoryDetail? GetByTerritoryType(ushort territoryType)
        {
            return _territoryDetails.FirstOrDefault(x => x.TerritoryType == territoryType);
        }
    }
}
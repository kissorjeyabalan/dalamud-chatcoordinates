using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;

namespace ChatCoordinates
{
    public class TerritoryManager
    {
        private readonly DalamudPluginInterface _pi;
        private IEnumerable<TerritoryDetail> _territoryDetails => GetTerritoryDetails();

        public TerritoryManager(DalamudPluginInterface dalamudPluginInterface)
        {
            _pi = dalamudPluginInterface;
        }

        public TerritoryDetail GetTerritoryDetailsByPlaceName(string placeName, bool matchPartial = true)
        {
            _pi.Framework.Gui.Chat.Print("Looking up details for " + placeName);
            var territoryDetail = _territoryDetails.FirstOrDefault(x =>
                x.PlaceName.Equals(placeName, StringComparison.OrdinalIgnoreCase) ||
                matchPartial && x.PlaceName.ToUpper().Contains(placeName.ToUpper()));
            return territoryDetail;
        }

        public IEnumerable<TerritoryDetail> GetTerritoryDetails()
        {
            var territoryDetails = new List<TerritoryDetail>();
            foreach (var territoryType in _pi.Data.GetExcelSheet<TerritoryType>())
            {
                var type = territoryType.Bg.Split('/');
                if (type.Length >= 3)
                {
                    if (type[2] == "twn" || type[2] == "fld" || type[2] == "hou")
                    {
                        if (!string.IsNullOrWhiteSpace(territoryType.Map.Value.PlaceName.Value.Name))
                        {
                            territoryDetails.Add(new TerritoryDetail
                            {
                                TerritoryType = territoryType.RowId, 
                                MapId = territoryType.Map.Value.RowId,
                                MapSizeFactor = territoryType.Map.Value.SizeFactor,
                                PlaceName = territoryType.Map.Value.PlaceName.Value.Name,
                            });   
                        }
                    }
                }
            }

            return territoryDetails;
        }
    }
}
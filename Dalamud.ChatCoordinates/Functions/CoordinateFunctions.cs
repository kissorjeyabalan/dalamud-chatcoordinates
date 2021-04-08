using System;
using ChatCoordinates.Models;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace ChatCoordinates.Functions
{
    public class CoordinateFunctions : IDisposable
    {
        private readonly ChatCoordinates _plugin;

        public CoordinateFunctions(ChatCoordinates plugin)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin), "ChatCoordinates cannot be null");
        }

        public void PlaceMarker(Coordinate coordinate)
        {
            if (!coordinate.HasCoordinates()) return;

            if (coordinate.ZoneSpecified && coordinate.TerritoryDetail == null)
                _plugin.Interface.Framework.Gui.Chat.PrintError($"No match found for {coordinate.Zone}.");

            if (coordinate.TerritoryDetail == null)
                _plugin.Interface.Framework.Gui.Chat.PrintError("Failed to determine zone.");

            var mapLink = new MapLinkPayload(
                _plugin.Interface.Data,
                coordinate.TerritoryDetail!.TerritoryType,
                coordinate.TerritoryDetail!.MapId,
                coordinate.NiceX,
                coordinate.NiceY,
                0f
            );

            _plugin.Interface.Framework.Gui.OpenMapWithMapLink(mapLink);
            _plugin.Interface.Framework.Gui.Chat.PrintChat(new XivChatEntry
            {
                MessageBytes = _plugin.Interface.SeStringManager.CreateMapLink(coordinate.TerritoryDetail.TerritoryType,
                    coordinate.TerritoryDetail.MapId, coordinate.NiceX, coordinate.NiceY, 0f).Encode()
            });
        }
        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
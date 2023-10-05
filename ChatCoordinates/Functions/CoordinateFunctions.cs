using System;
using ChatCoordinates.Models;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;

namespace ChatCoordinates.Functions
{
    public class CoordinateFunctions : IDisposable
    {
        private readonly CCPlugin _plugin;
        private IGameGui _gameGui;

        public CoordinateFunctions(CCPlugin plugin, IGameGui gameGui)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin), "ChatCoordinates cannot be null");
            _gameGui = gameGui;
        }

        public void PlaceMarker(Coordinate coordinate)
        {
            if (!coordinate.HasCoordinates()) return;

            if (coordinate.ZoneSpecified && coordinate.TerritoryDetail == null)
                _plugin.PrintError($"No match found for {coordinate.Zone}.");

            if (coordinate.TerritoryDetail == null)
                _plugin.PrintError("Failed to determine zone.");

            var mapLink = new MapLinkPayload(
                coordinate.TerritoryDetail!.TerritoryType,
                coordinate.TerritoryDetail!.MapId,
                coordinate.NiceX,
                coordinate.NiceY,
                0f
            );

            _gameGui.OpenMapWithMapLink(mapLink);
            _plugin.PrintChat(new XivChatEntry
            {
                Message = SeString.CreateMapLink(coordinate.TerritoryDetail.TerritoryType,
                    coordinate.TerritoryDetail.MapId, coordinate.NiceX, coordinate.NiceY, 0f),
                Type = _plugin.Configuration.GeneralChatType
            });
        }
        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
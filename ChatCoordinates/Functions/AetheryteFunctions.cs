using System;
using ChatCoordinates.Models;

namespace ChatCoordinates.Functions
{
    public class AetheryteFunctions
    {
        private readonly CCPlugin _plugin;

        public AetheryteFunctions(CCPlugin plugin)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin), "ChatCoordinates cannot be null");
        }

        public void Teleport(Coordinate coordinate)
        {
            if (!coordinate.Teleport) return;

            var tpCmd = _plugin.CommandManager.Commands.ContainsKey("/tp")
                ? _plugin.CommandManager.Commands["/tp"]
                : null;

            if (tpCmd != null)
            {
                var aetheryte = _plugin.AetheryteManager.GetClosestAetheryte(coordinate);
                if (aetheryte != null && !string.IsNullOrWhiteSpace(aetheryte.Name))
                {
                    tpCmd.Handler.Invoke("/tp", aetheryte.Name);
                }
                else
                {
                    _plugin.PrintChat("Failed to find closest aetheryte.");
                }
            }
            else
            {
                _plugin.PrintError(
                    "Teleporting requires Teleporter plugin to be installed.");
            }
        }
    }
}
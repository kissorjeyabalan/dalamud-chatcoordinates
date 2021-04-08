using System;
using ChatCoordinates.Models;

namespace ChatCoordinates.Functions
{
    public class AetheryteFunctions
    {
        private readonly ChatCoordinates _plugin;

        public AetheryteFunctions(ChatCoordinates plugin)
        {
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin), "ChatCoordinates cannot be null");
        }

        public void Teleport(Coordinate coordinate)
        {
            if (!coordinate.Teleport) return;

            var tpCmd = _plugin.Interface.CommandManager.Commands.ContainsKey("/tp")
                ? _plugin.Interface.CommandManager.Commands["/tp"]
                : null;

            var tpTicketCmd = _plugin.Interface.CommandManager.Commands.ContainsKey("/tpt")
                ? _plugin.Interface.CommandManager.Commands["/tpt"]
                : null;

            if (tpCmd != null && tpTicketCmd != null)
            {
                var aetheryte = _plugin.AetheryteManager.GetClosestAetheryte(coordinate);
                if (aetheryte != null)
                {
                    if (coordinate.UseTicket)
                        tpTicketCmd.Handler.Invoke("/tpt", aetheryte.Name);
                    else
                        tpCmd.Handler.Invoke("/tp", aetheryte.Name);
                }
                else
                {
                    _plugin.Interface.Framework.Gui.Chat.Print("Failed to find closest aetheryte.");
                }
            }
            else
            {
                _plugin.Interface.Framework.Gui.Chat.PrintError(
                    "Teleporting requires Teleporter plugin to be installed.");
            }
        }
    }
}
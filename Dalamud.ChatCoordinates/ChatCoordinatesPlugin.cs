using System;
using System.Text;
using System.Text.RegularExpressions;
using Dalamud.Game.Chat;
using Dalamud.Game.Chat.SeStringHandling.Payloads;
using Dalamud.Game.Command;
using Dalamud.Game.Internal.Gui;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;


namespace ChatCoordinates
{
    public class ChatCoordinatesPlugin : IDalamudPlugin
    {
        public string Name => "ChatCoordinates Plugin";
        private DalamudPluginInterface _pi;
        
        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            _pi = pluginInterface;

            _pi.CommandManager.AddHandler("/coord", new CommandInfo(CommandHandler)
            {
                HelpMessage = "/coord <x> <y>"
            });

#if DEBUG
            _pi.CommandManager.AddHandler("/territory", new CommandInfo(PrintTerritory)
            {
                HelpMessage = "Shows current territory ushort"
            });
            _pi.ClientState.TerritoryChanged += (sender, e) => { _pi.Framework.Gui.Chat.Print($"Territory: {e}"); };
#endif
        }

        public void PrintTerritory(string command, string args)
        {
            var unsignedTerritoryType = Convert.ToUInt32(_pi.ClientState.TerritoryType);

            var territorySheet = _pi.Data.GetExcelSheet<TerritoryType>().GetRow(_pi.ClientState.TerritoryType);
              _pi.Framework.Gui.Chat.Print($"------ {territorySheet.Name} ------");
              _pi.Framework.Gui.Chat.Print($"Map: {territorySheet.Map.ToString()}");
              _pi.Framework.Gui.Chat.Print($"Territory: {unsignedTerritoryType}");
        }
        public void CommandHandler(string command, string arguments)
        {
            var arg = arguments.Trim().Replace("\"", "");
            if (string.IsNullOrEmpty(arg) || arg.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                ShowHelp(command);
                return;
            }

            var coordinates = Regex.Matches(arg, "(\\d*\\.?\\d*)");
            var xSet = false;
            var ySet = false;
            var x = 0.0f;
            var y = 0.0f;
            foreach (Match coordinate in coordinates)
            {
                if (string.IsNullOrWhiteSpace(coordinate.Value)) continue;
                if (!float.TryParse(coordinate.Value, out var coord)) continue;
                if (!xSet)
                {
                    x = coord;
                    xSet = true;
                    continue;
                }

                if (ySet) continue;
                y = coord;
                ySet = true;
            }

            if (!xSet || !ySet)
                ShowHelp(command);
            
            var unsignedTerritoryType = Convert.ToUInt32(_pi.ClientState.TerritoryType);
            var territorySheet = _pi.Data.GetExcelSheet<TerritoryType>().GetRow(_pi.ClientState.TerritoryType);
            var mapLink = new MapLinkPayload(unsignedTerritoryType, territorySheet.Map, x, y);
            _pi.Framework.Gui.OpenMapWithMapLink(mapLink);
        }

        private void ShowHelp(string command)
        {
            var helpText =
                $"{Name} Help:\n" +
                $"{command} help - Show this message \n" +
                $"{command} 8.8,11.5\n" +
                $"{command} (x8.8,y11.5)\n" +
                $"{command} 8.8 11.5\n";
            _pi.Framework.Gui.Chat.Print(helpText);
        }
        
        public void Dispose()
        {
            _pi.CommandManager.RemoveHandler("/coords");
            _pi.Dispose();
        }
    }
}
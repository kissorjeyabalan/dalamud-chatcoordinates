using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ChatCoordinates.Managers;
using Dalamud.Game;
using Dalamud.Game.Chat;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;

namespace ChatCoordinates
{
    public class ChatCoordinatesPlugin : IDalamudPlugin
    {
        public string Name => "ChatCoordinates Plugin";
        private DalamudPluginInterface _pi;

        private Lazy<TerritoryManager> _territoryManager;
        private Lazy<AetheryteManager> _aetheryteManager;

        private GetUIObjectDelegate getUIObject;
        private GetUIMapObjectDelegate getUIMapObject;
        private OpenMapWithFlagDelegate openMapWithFlag;

        public ChatCoordinatesPlugin()
        {
            _territoryManager = new Lazy<TerritoryManager>(() => new TerritoryManager(_pi));
            _aetheryteManager = new Lazy<AetheryteManager>(() => new AetheryteManager(_pi));
        }

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            _pi = pluginInterface;

            var targetModule = Process.GetCurrentProcess().MainModule;
            var sig = new SigScanner(targetModule, true);
            getUIObject = Marshal.GetDelegateForFunctionPointer<GetUIObjectDelegate>(
                sig.ScanText("E8 ?? ?? ?? ?? 48 8B C8 48 8B 10 FF 52 40 80 88 ?? ?? ?? ?? 01 E9"));

            _pi.CommandManager.AddHandler("/coord", new CommandInfo(CommandHandler)
            {
                HelpMessage = "/coord <x> <y>"
            });

            _pi.CommandManager.AddHandler("/ctp", new CommandInfo(CommandHandler)
            {
                HelpMessage = "/ctp <x> <y> : <partial zone name>"
            });
            
            _pi.CommandManager.AddHandler("/ctpt", new CommandInfo(CommandHandler)
            {
                HelpMessage = "/ctpt <x> <y> : <partial zone name> - Uses Aetheryte Ticket if possible"
            });

#if DEBUG
            _pi.CommandManager.AddHandler("/territory", new CommandInfo(PrintTerritory)
            {
                HelpMessage = "Shows current territory ushort"
            });
#endif
        }

        public void PrintTerritory(string command, string args)
        {
            _territoryManager.Value.GetTerritoryDetails();
            if (_pi.ClientState.TerritoryType == 0)
            {
                _pi.Framework.Gui.Chat.Print(
                    "Unable to get territory info, please switch your territory to initialize.");
                return;
            }

            var unsignedTerritoryType = Convert.ToUInt32(_pi.ClientState.TerritoryType);
            var territorySheet = _pi.Data.GetExcelSheet<TerritoryType>().GetRow(_pi.ClientState.TerritoryType);
            var map = _pi.Data.GetExcelSheet<Map>().GetRow(territorySheet.Map.Value.RowId);
            _pi.Framework.Gui.Chat.Print(
                $"------ {territorySheet.Name} - {territorySheet.Map.Value.PlaceName.Value.Name} ------");
            _pi.Framework.Gui.Chat.Print($"Map: {territorySheet.Map.Value.RowId}");
            _pi.Framework.Gui.Chat.Print($"Territory: {unsignedTerritoryType}");
            _pi.Framework.Gui.Chat.Print("---------");
        }

        public void CommandHandler(string command, string arguments)
        {
            var arg = arguments.Trim().Replace("\"", "");
            if (string.IsNullOrEmpty(arg) || arg.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                ShowHelp(command);
                return;
            }

            if (arguments.Contains(":"))
            {
                PlaceCoordinateByZone(command, arg, command.ToLower().Contains("/ctp"), command.ToLower().Equals("/ctpt"));
            }
            else
            {
                PlaceCoordinate(command, arg);
            }
        }

        private void PlaceCoordinateByZone(string command, string arg, bool teleport, bool ticket = false)
        {
            var coordinates = GetRawXAndRawYByInput(arg);
            if (coordinates == null)
            {
                ShowHelp(command);
                return;
            }

            var zone = arg.Split(':').Last();
            var territoryDetails = _territoryManager.Value.GetTerritoryDetailsByPlaceName(zone.Trim());
            if (territoryDetails == null)
            {
                if (arg.Count(x => x == ':') == 1)
                {
                    _pi.Framework.Gui.Chat.Print($"No match found for {zone.Trim()}.");
                    return;   
                }
                
                PlaceCoordinate(command, arg);
                return;
            }

            OpenMapWithFlag(territoryDetails.TerritoryType, territoryDetails.MapId, territoryDetails.MapSizeFactor,
                coordinates.Item1, coordinates.Item2);
            _pi.Framework.Gui.Chat.PrintChat(new XivChatEntry
            {
                MessageBytes = _pi.SeStringManager
                    .CreateMapLink(territoryDetails.PlaceName, coordinates.Item1, coordinates.Item2).Encode()
            });

            if (!teleport) return;
            var tpCommand = _pi.CommandManager.Commands.ContainsKey("/tp")
                ? _pi.CommandManager.Commands["/tp"]
                : null;
            var tpTicketCommand = _pi.CommandManager.Commands.ContainsKey("/tptm")
                ? _pi.CommandManager.Commands["/tptm"]
                : null;
            if (tpCommand != null && tpTicketCommand != null)
            {
                var aetheryte = _aetheryteManager.Value.GetClosestAetheryte(coordinates, territoryDetails);
                if (aetheryte?.Aetheryte != null || aetheryte?.MapMarker != null)
                {
                    if (ticket)
                        tpTicketCommand.Handler.Invoke("/tptm", $"{aetheryte.Aetheryte?.PlaceName.Value.Name.RawString}");
                    else
                        tpCommand.Handler.Invoke("/tp", $"{aetheryte.Aetheryte?.PlaceName.Value.Name.RawString}");
                }
                else
                {
                    _pi.Framework.Gui.Chat.PrintError("Failed to find aetheryte");
                }
            }
            else
            {
                _pi.Framework.Gui.Chat.PrintError("Teleporting requires Teleporter plugin to be installed.");
            }
        }

        private void PlaceCoordinate(string command, string arg)
        {
            var coordinates = GetRawXAndRawYByInput(arg);
            if (coordinates == null)
            {
                ShowHelp(command);
                return;
            }

            if (_pi.ClientState.TerritoryType == 0)
            {
                _pi.Framework.Gui.Chat.Print(
                    "Unable to get territory info, please switch your territory to initialize.");
                return;
            }

            var unsignedTerritoryType = Convert.ToUInt32(_pi.ClientState.TerritoryType);
            var territorySheet = _pi.Data.GetExcelSheet<TerritoryType>().GetRow(_pi.ClientState.TerritoryType);
            OpenMapWithFlag(unsignedTerritoryType, territorySheet.Map.Value.RowId, territorySheet.Map.Value.SizeFactor,
                coordinates.Item1, coordinates.Item2);
            _pi.Framework.Gui.Chat.PrintChat(new XivChatEntry
            {
                MessageBytes = _pi.SeStringManager.CreateMapLink(territorySheet.Map.Value.PlaceName.Value.Name,
                    coordinates.Item1, coordinates.Item2).Encode()
            });
        }

        private Tuple<float, float> GetRawXAndRawYByInput(string coordinateString)
        {
            var coordinates = Regex.Matches(coordinateString, "(\\d*\\.?\\d*)");
            var xSet = false;
            var ySet = false;
            var x = 0.0f;
            var y = 0.0f;
            foreach (Match coordinate in coordinates)
            {
                if (string.IsNullOrWhiteSpace(coordinate.Value)) continue;
                if (!float.TryParse(coordinate.Value, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out var coord)) continue;
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
                return null;

            return new Tuple<float, float>(x, y);
        }

        private void ShowHelp(string command)
        {
            var helpText =
                $"{Name} Help:\n" +
                $"First set of numbers are treated as coordinates. Any combination of letters or symbols (except colon) acts as delimiter for X and Y. Accepts optional colon (:) as delimiter for zone.\n" +
                $"Placed marker can be shared by typing <flag>\n" +
                $"/coord 8.8,11.5\n" +
                $"/coord (x8.8,y11.5)\n" +
                $"/coord 8.8 11.5\n" +
                $"/coord 10.7 11.7 : Lakeland\n" +
                $"/ctp 10.7,11.7 : Lakeland";
            _pi.Framework.Gui.Chat.Print(helpText);
        }

        public void Dispose()
        {
            _pi.CommandManager.RemoveHandler("/coords");
            _pi.CommandManager.RemoveHandler("/ctp");
            _pi.CommandManager.RemoveHandler("/ctpt");

#if DEBUG
            _pi.CommandManager.RemoveHandler("/territory");
#endif
            _pi.Dispose();
        }

        private void OpenMapWithFlag(uint territoryType, uint mapId, ushort sizeFactor, float niceX, float niceY,
            float fudge = 0.05f)
        {
            var rawX = CoordinateHelper.ConvertMapCoordinateToRawPosition(niceX + fudge, sizeFactor);
            var rawY = CoordinateHelper.ConvertMapCoordinateToRawPosition(niceY + fudge, sizeFactor);

            var num1 = getUIObject();
            getUIMapObject = _pi.Framework.Address.GetVirtualFunction<GetUIMapObjectDelegate>(num1, 0, 8);
            var num2 = getUIMapObject(num1);
            openMapWithFlag = _pi.Framework.Address.GetVirtualFunction<OpenMapWithFlagDelegate>(num2, 0, 63);
            var dataString =
                $"m:{(object) territoryType},{(object) mapId},{(object) rawX},{(object) rawY}";
            openMapWithFlag(num2, dataString);
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetUIObjectDelegate();

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetUIMapObjectDelegate(IntPtr UIObject);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate bool OpenMapWithFlagDelegate(IntPtr UIMapObject, string flag);
    }
}
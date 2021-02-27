using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using ChatCoordinates.Extensions;
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
        private DalamudWrapper _dalamudPlugin;

        private Lazy<TerritoryManager> _lazyTerritoryManager;
        private Lazy<AetheryteManager> _aetheryteManager;
        private TerritoryManager TerritoryManager => _lazyTerritoryManager.Value;
        private AetheryteManager AetheryteManager => _aetheryteManager.Value;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            _pi = pluginInterface;
            _dalamudPlugin = new DalamudWrapper(_pi);

            _lazyTerritoryManager = new Lazy<TerritoryManager>(() => new TerritoryManager(_dalamudPlugin));
            _aetheryteManager = new Lazy<AetheryteManager>(() => new AetheryteManager(_dalamudPlugin));

            LoadMapDelegates();

            _pi.CommandManager.AddHandler("/coord", new CommandInfo(CommandHandler)
            {
                HelpMessage = "/coord <x> <y> -- Places map marker at given coordinates"
            });

            _pi.CommandManager.AddHandler("/ctp", new CommandInfo(CommandHandler)
            {
                HelpMessage =
                    "/ctp <x> <y> : <partial zone name> -- Places map marker and teleports to closest aetheryte"
            });

            _pi.CommandManager.AddHandler("/ctpt", new CommandInfo(CommandHandler)
            {
                HelpMessage =
                    "/ctpt <x> <y> : <partial <one name> -- Places map marker and teleports to closest aetheryte with ticket"
            });
        }

        public void CommandHandler(string command, string arguments)
        {
            var arg = arguments.Trim().Replace("\"", "");
            if (string.IsNullOrWhiteSpace(arg) || arg.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                ShowHelp(command);
                return;
            }

            if (arguments.Contains(":"))
            {
                PlaceMapMarkerByZone(command, arg, command.ToLower().Contains("/ctp"),
                    command.ToLower().Equals("/ctpt"));
            }
            else
            {
                PlaceMapMarkerByZone(command, arg);
            }
        }

        private void PlaceMapMarkerByZone(string command, string arg, bool teleport = false, bool ticket = false)
        {
            var coordinates = arg.GetCoordinates();
            if (coordinates == null)
            {
                ShowHelp(command);
                return;
            }

            var zone = arg.Split(':').Last();
            var territoryDetails = TerritoryManager.GetTerritoryDetailsByZoneName(zone.Trim());
            if (territoryDetails == null)
            {
                if (arg.Count(x => x == ':') == 1)
                {
                    _dalamudPlugin.Print($"No match found for {zone.Trim()}");
                    return;
                }

                PlaceMapMarker(command, arg);
                return;
            }

            OpenMapWithFlag(
                territoryDetails.TerritoryType,
                territoryDetails.MapId,
                territoryDetails.SizeFactor,
                coordinates.Value);

            _dalamudPlugin.PrintChat(new XivChatEntry
            {
                MessageBytes = _pi.SeStringManager
                    .CreateMapLink(
                        territoryDetails.Name,
                        coordinates.Value.X,
                        coordinates.Value.Y)
                    .Encode()
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
                var aetheryte = AetheryteManager.GetClosestAetheryte(coordinates.Value, territoryDetails);
                if (aetheryte != null)
                {
                    if (ticket) 
                        tpTicketCommand.Handler.Invoke("/tptm", aetheryte.Name);
                    else
                        tpCommand.Handler.Invoke("/tp", aetheryte.Name);
                }
                else
                {
                    _dalamudPlugin.Print("Failed to find closest aetheryte.");
                }
            }
            else
            {
                _dalamudPlugin.PrintError("Teleporting requires Teleporter plugin to be installed.");
            }
        }

        private void PlaceMapMarker(string command, string arg)
        {
            var coordinates = arg.GetCoordinates();
            if (coordinates == null)
            {
                ShowHelp(command);
                return;
            }

            if (_pi.ClientState.TerritoryType == 0)
            {
                _dalamudPlugin.Print("Unable to get territory info. Please switch zone to initialize plugin.");
                return;
            }

            var unsignedTerritoryType = Convert.ToUInt32(_pi.ClientState.TerritoryType);
            var territorySheet = _pi.Data.GetExcelSheet<TerritoryType>().GetRow(_pi.ClientState.TerritoryType);
            OpenMapWithFlag(unsignedTerritoryType, territorySheet.Map.Value.RowId, territorySheet.Map.Value.SizeFactor,
                coordinates.Value);
            _dalamudPlugin.PrintChat(new XivChatEntry
            {
                MessageBytes = _pi.SeStringManager.CreateMapLink(
                    territorySheet.Map.Value.PlaceName.Value.Name,
                    coordinates.Value.X, coordinates.Value.Y).Encode()
            });
        }

        private void OpenMapWithFlag(uint territoryType, uint mapId, ushort sizeFactor, Vector2 coordinates)
        {
            var rawCoordinates = coordinates.ToRawCoordinates(sizeFactor);

            var num1 = getUIObject();
            getUIMapObject = _pi.Framework.Address.GetVirtualFunction<GetUIMapObjectDelegate>(num1, 0, 8);
            var num2 = getUIMapObject(num1);
            openMapWithFlag = _pi.Framework.Address.GetVirtualFunction<OpenMapWithFlagDelegate>(num2, 0, 63);
            var dataString =
                $"m:{(object) territoryType},{(object) mapId},{(object) rawCoordinates.X},{(object) rawCoordinates.Y}";
            openMapWithFlag(num2, dataString);
        }

        private void ShowHelp(string cmd)
        {
            switch (cmd)
            {
                case "/coord":
                    _dalamudPlugin.Print("Places a map marker at given coordinates. Colon (:) can be used a delimiter to place marker at given zone. Placed marker can be shared by typing <flag>.");
                    _dalamudPlugin.Print("/coord <x> <y> [: <zone>]");
                    _dalamudPlugin.Print("/coord 8.8 11.5");
                    _dalamudPlugin.Print("/coord 8.8,11.5");
                    _dalamudPlugin.Print("/coord X: 10.7 Y: 11.7 : Lakeland");
                    _dalamudPlugin.Print("/coord 10.7 11.7 : Lakeland");
                    break;
                case "/ctp":
                    _dalamudPlugin.Print("Places a map marker at given coordinate and teleports to the closest aetheryte. Colon (:) is used as delimiter for zone.");
                    _dalamudPlugin.Print("/ctp 10.7 11.7 : Lakeland");
                    _dalamudPlugin.Print("/ctp 10.7 11.7 : Lakeland");
                    _dalamudPlugin.Print("/ctp X: 10.7 Y: 11.7 : Lakeland");
                    break;
            }
        }

        public void Dispose()
        {
            _pi.CommandManager.RemoveHandler("/coord");
            _pi.CommandManager.RemoveHandler("/ctp");
            _pi.CommandManager.RemoveHandler("/ctpt");
            _pi.Dispose();
        }

        private void LoadMapDelegates()
        {
            var targetModule = Process.GetCurrentProcess().MainModule;
            var sigScanner = new SigScanner(targetModule, true);
            getUIObject = Marshal.GetDelegateForFunctionPointer<GetUIObjectDelegate>(
                sigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B C8 48 8B 10 FF 52 40 80 88 ?? ?? ?? ?? 01 E9"));
        }


        private GetUIObjectDelegate getUIObject;
        private GetUIMapObjectDelegate getUIMapObject;
        private OpenMapWithFlagDelegate openMapWithFlag;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr GetUIObjectDelegate();

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetUIMapObjectDelegate(IntPtr UIObject);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        private delegate bool OpenMapWithFlagDelegate(IntPtr UIMapObject, string flag);
    }
}
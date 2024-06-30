using System;
using ChatCoordinates.Configuration;
using ChatCoordinates.Extensions;
using ChatCoordinates.Functions;
using ChatCoordinates.Managers;
using ChatCoordinates.Models;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace ChatCoordinates
{
    public class CCPlugin : IDalamudPlugin
    {
        public string Name => "ChatCoordinates";
        public Config Configuration { get; init; }
        public ConfigUi ConfigUi { get; init; }
        public IDalamudPluginInterface Interface { get; init; }
        public ICommandManager CommandManager { get; init; }
        public IClientState ClientState { get; init; }
        public IChatGui ChatGui { get; init; }

        private Lazy<AetheryteManager> _aetheryteManager = null!;
        private Lazy<TerritoryManager> _territoryManager = null!;
        public TerritoryManager TerritoryManager => _territoryManager.Value;
        public AetheryteManager AetheryteManager => _aetheryteManager.Value;
        public CoordinateFunctions CoordinateFunctions { get; private set; } = null!;
        public AetheryteFunctions AetheryteFunctions { get; private set; } = null!;

        public CCPlugin(
            IDalamudPluginInterface pluginInterface,
            ICommandManager commandManager,
            IDataManager dataManager,
            IGameGui gameGui,
            IClientState clientState,
            IChatGui chatGui
        )
        {
            Interface = pluginInterface;
            CommandManager = commandManager;
            ClientState = clientState;
            ChatGui = chatGui;

            Configuration = Interface.GetPluginConfig() as Config ?? new Config();
            Configuration.Initialize(Interface);
            ConfigUi = new ConfigUi(this);

            CoordinateFunctions = new CoordinateFunctions(this, gameGui);
            AetheryteFunctions = new AetheryteFunctions(this);
            _territoryManager = new Lazy<TerritoryManager>(() => new TerritoryManager(dataManager));
            _aetheryteManager = new Lazy<AetheryteManager>(() => new AetheryteManager(dataManager));

            CommandManager.AddHandler("/coord", new CommandInfo(OnCoordinateCommand)
            {
                HelpMessage =
                    $"/coord <x> <y> [{Configuration.ZoneDelimiter} <partial zone name>] -- Places map marker at given coordinates"
            });

            CommandManager.AddHandler("/ctp", new CommandInfo(OnCoordinateTeleportCommand)
            {
                HelpMessage =
                    $"/ctp <x> <y> [{Configuration.ZoneDelimiter} <partial zone name>] -- Places map marker and teleports to closest aetheryte"
            });
        }

        private void OnCoordinateCommand(string cmd, string args)
        {
            ProcessCoordinate(cmd, args);
        }

        private void OnCoordinateTeleportCommand(string cmd, string args)
        {
            var coordinate = ProcessCoordinate(cmd, args);
            if (coordinate == null) return;

            coordinate.Teleport = true;

            AetheryteFunctions.Teleport(coordinate);
        }

        private Coordinate? ProcessCoordinate(string cmd, string args)
        {
            if (ClientState.TerritoryType == 0)
            {
                PrintChat(
                    "Unable to get territory info. Please switch zone to initialize plugin.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(args))
            {
                ConfigUi.Visible = true;
                return null;
            }

            if (args.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                ShowHelp(cmd);
                return null;
            }

            var coordinate = args.ParseCoordinate(this);
            CoordinateFunctions.PlaceMarker(coordinate);

            return coordinate;
        }
        
        private void ShowHelp(string cmd)
        {
            switch (cmd)
            {
                case "/coord":
                    PrintChat(
                        $"Places a map marker at given coordinates. {Configuration.ZoneDelimiter} can be used a delimiter to place marker at given zone. Placed marker can be shared by typing <flag>.");
                    PrintChat($"/coord <x> <y> [{Configuration.ZoneDelimiter} <zone>]");
                    PrintChat($"/coord 8.8 11.5");
                    PrintChat("/coord 8.8,11.5");
                    PrintChat("/coord X: 10.7 Y: 11.7 : Lakeland");
                    PrintChat("/coord 10.7 11.7 : Lakeland");
                    break;
                case "/ctp":
                    PrintChat(
                        $"Places a map marker at given coordinate and teleports to the closest aetheryte. {Configuration.ZoneDelimiter} is used as delimiter for zone.");
                    PrintChat("/ctp 10.7 11.7 : Lakeland");
                    PrintChat("/ctp 10.7 11.7 : Lakeland");
                    PrintChat("/ctp X: 10.7 Y: 11.7 : Lakeland");
                    break;
            }
        }
        
        public void PrintChat(string msg)
        {
            ChatGui.Print(new XivChatEntry()
            {
                Message = msg,
                Type = Configuration.GeneralChatType
            });
        }

        public void PrintError(string msg)
        {
            ChatGui.Print(new XivChatEntry()
            {
                Message = msg,
                Type = Configuration.ErrorChatType
            });
        }
        
        public void PrintChat(XivChatEntry msg)
        {
            ChatGui.Print(msg);
        }

        public void Dispose()
        {
            CommandManager.RemoveHandler("/coord");
            CommandManager.RemoveHandler("/ctp");
            ConfigUi.Dispose();
        }
    }
}
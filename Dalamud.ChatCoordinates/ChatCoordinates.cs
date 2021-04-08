using System;
using ChatCoordinates.Extensions;
using ChatCoordinates.Functions;
using ChatCoordinates.Managers;
using ChatCoordinates.Models;
using Dalamud.Game.Command;
using Dalamud.Plugin;

namespace ChatCoordinates
{
    public class ChatCoordinates : IDalamudPlugin

    {
        private Lazy<AetheryteManager> _aetheryteManager = null!;
        private bool _disposed;

        private Lazy<TerritoryManager> _territoryManager = null!;

        public DalamudPluginInterface Interface { get; private set; } = null!;
        public CoordinateFunctions CoordinateFunctions { get; private set; } = null!;
        public AetheryteFunctions AetheryteFunctions { get; private set; } = null!;
        public TerritoryManager TerritoryManager => _territoryManager.Value;
        public AetheryteManager AetheryteManager => _aetheryteManager.Value;
        public string Name => nameof(ChatCoordinates);

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            Interface = pluginInterface ??
                        throw new ArgumentNullException(nameof(pluginInterface), "Dalamud interface cannot be null");
            CoordinateFunctions = new CoordinateFunctions(this);
            AetheryteFunctions = new AetheryteFunctions(this);

            _territoryManager = new Lazy<TerritoryManager>(() => new TerritoryManager(this));
            _aetheryteManager = new Lazy<AetheryteManager>(() => new AetheryteManager(this));


            Interface.CommandManager.AddHandler("/coord", new CommandInfo(OnCoordinateCommand)
            {
                HelpMessage = "/coord <x> <y> [: <partial zone name>] -- Places map marker at given coordinates"
            });

            Interface.CommandManager.AddHandler("/ctp", new CommandInfo(OnCoordinateTeleportCommand)
            {
                HelpMessage =
                    "/ctp <x> <y> [: <partial zone name>] -- Places map marker and teleports to closest aetheryte"
            });

            Interface.CommandManager.AddHandler("/ctpt", new CommandInfo(OnCoordinateTeleportCommand)
            {
                HelpMessage =
                    "/ctpt <x> <y> [: <partial zone name>] -- Places map marker and teleports to closets aetheryte with ticket"
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

            if (cmd.ToLower().Equals("/ctpt")) coordinate.UseTicket = true;

            AetheryteFunctions.Teleport(coordinate);
        }

        private Coordinate? ProcessCoordinate(string cmd, string args)
        {
            if (Interface.ClientState.TerritoryType == 0)
            {
                Interface.Framework.Gui.Chat.Print(
                    "Unable to get territory info. Please switch zone to initialize plugin.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(args) || args.Equals("help", StringComparison.OrdinalIgnoreCase))
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
                    Interface.Framework.Gui.Chat.Print(
                        "Places a map marker at given coordinates. Colon (:) can be used a delimiter to place marker at given zone. Placed marker can be shared by typing <flag>.");
                    Interface.Framework.Gui.Chat.Print("/coord <x> <y> [: <zone>]");
                    Interface.Framework.Gui.Chat.Print("/coord 8.8 11.5");
                    Interface.Framework.Gui.Chat.Print("/coord 8.8,11.5");
                    Interface.Framework.Gui.Chat.Print("/coord X: 10.7 Y: 11.7 : Lakeland");
                    Interface.Framework.Gui.Chat.Print("/coord 10.7 11.7 : Lakeland");
                    break;
                case "/ctp":
                    Interface.Framework.Gui.Chat.Print(
                        "Places a map marker at given coordinate and teleports to the closest aetheryte. Colon (:) is used as delimiter for zone.");
                    Interface.Framework.Gui.Chat.Print("/ctp 10.7 11.7 : Lakeland");
                    Interface.Framework.Gui.Chat.Print("/ctp 10.7 11.7 : Lakeland");
                    Interface.Framework.Gui.Chat.Print("/ctp X: 10.7 Y: 11.7 : Lakeland");
                    break;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                CoordinateFunctions.Dispose();

                Interface.CommandManager.RemoveHandler("/coord");
                Interface.CommandManager.RemoveHandler("/ctp");
                Interface.CommandManager.RemoveHandler("/ctpt");
                Interface.Dispose();
            }

            _disposed = true;
        }
    }
}
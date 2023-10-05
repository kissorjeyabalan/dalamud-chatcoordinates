using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using ImGuiNET;

namespace ChatCoordinates.Configuration
{
    public class ConfigUi : IDisposable
    {
        private CCPlugin _plugin;

        private bool _visible = false;

        public bool Visible
        {
            get => _visible;
            set => _visible = value;
        }

        public ConfigUi(CCPlugin plugin)
        {
            _plugin = plugin;
            _plugin.Interface.UiBuilder.Draw += DrawUI;
            _plugin.Interface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void DrawConfigUI()
        {
            Visible = true;
        }
        public void DrawUI()
        {
            if (!Visible) return;

            var zoneDelimiter = _plugin.Configuration.ZoneDelimiter;
            var xivChatTypes = Enum.GetValues(typeof(XivChatType)).Cast<XivChatType>().ToList();
            
            ImGui.SetNextWindowSize(new Vector2(320, 320), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(320, 320), new Vector2(float.MaxValue, float.MaxValue));

            if (ImGui.Begin($"{_plugin.Name} - Settings", ref _visible))
            {
                ImGui.Text("Zone Delimiter");
                if (ImGui.InputText("##CoordZone", ref zoneDelimiter, 10))
                {
                    if (!string.IsNullOrWhiteSpace(zoneDelimiter))
                    {
                        _plugin.Configuration.ZoneDelimiter = zoneDelimiter;
                    }
                }

                ImGui.TextColored(ImGuiColors.DalamudGrey,
                    "Delimiter used to determine zone.");

                ImGui.Spacing();

                ImGui.TextUnformatted("General Chat Channel");

                if (ImGui.BeginCombo("##CoordChat", _plugin.Configuration.GeneralChatType.ToString()))
                {
                    foreach (var chatType in xivChatTypes.Where(chatType =>
                        ImGui.Selectable(chatType.ToString(), chatType == _plugin.Configuration.GeneralChatType)))
                    {
                        _plugin.Configuration.GeneralChatType = chatType;
                    }

                    ImGui.EndCombo();
                }

                ImGui.TextColored(ImGuiColors.DalamudGrey,
                    "Chat channel to used to print messages from ChatCoordinates.");

                ImGuiHelpers.ScaledDummy(5);

                ImGui.TextUnformatted("Error Chat Channel");
                if (ImGui.BeginCombo("##CoordErr", _plugin.Configuration.ErrorChatType.ToString()))
                {
                    foreach (var chatType in xivChatTypes.Where(chatType =>
                        ImGui.Selectable(chatType.ToString(), chatType == _plugin.Configuration.ErrorChatType)))
                    {
                        _plugin.Configuration.ErrorChatType = chatType;
                    }

                    ImGui.EndCombo();
                }

                ImGui.TextColored(ImGuiColors.DalamudGrey,
                    "Chat channel to used to print error messages from ChatCoordinates.");

                ImGuiHelpers.ScaledDummy(5);

                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.TextColored(ImGuiColors.DPSRed, zoneDelimiter);
                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.DalamudGrey, "can be used a delimiter to place marker at given zone.");
                ImGui.TextColored(ImGuiColors.DalamudGrey, "Placed marker can be shared by typing <flag>.");
                ImGui.TextColored(ImGuiColors.DalamudGrey, "Examples:");
                ImGui.TextColored(ImGuiColors.DalamudGrey, $"/coord <x> <y> [{zoneDelimiter} <zone>]");
                ImGui.TextColored(ImGuiColors.DalamudGrey, $"/coord 8.8 11.5");
                ImGui.TextColored(ImGuiColors.DalamudGrey, $"/coord 8.8,11.5");
                ImGui.TextColored(ImGuiColors.DalamudGrey, $"/coord X: 10.7 Y: 11.7 : Lakeland");
                ImGui.TextColored(ImGuiColors.DalamudGrey, $"/coord 10.7 11.7 : Lake");

                if (ImGui.Button("Save and exit"))
                {
                    _plugin.Configuration.Save();
                    Visible = false;
                }
            }

            ImGui.End();
        }

        public void Dispose()
        {
        }
    }
}
using System;
using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Plugin;

namespace ChatCoordinates.Configuration
{
    public class Config : IPluginConfiguration
    {
        [NonSerialized] private IDalamudPluginInterface? _pluginInterface;
        
        public int Version { get; set; } = 1;

        public string ZoneDelimiter { get; set; } = ":";
        public XivChatType GeneralChatType { get; set; } = XivChatType.Debug;
        public XivChatType ErrorChatType { get; set; } = XivChatType.Urgent;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            _pluginInterface = pluginInterface;
        }

        public void Save()
        {
            _pluginInterface!.SavePluginConfig(this);
        }
    }
}
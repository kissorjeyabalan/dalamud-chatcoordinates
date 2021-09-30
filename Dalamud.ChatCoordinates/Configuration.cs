using System;
using Dalamud.Configuration;
using Dalamud.Game.Text;

namespace ChatCoordinates
{
    public class Configuration : IPluginConfiguration
    {
     
        [NonSerialized]
        private ChatCoordinates _plugin;
        
        public int Version { get; set; } = 1;

        public string ZoneDelimiter { get; set; } = ":";
        public XivChatType ChatType { get; set; } = XivChatType.Debug;
        public XivChatType ErrorChatType { get; set; } = XivChatType.Urgent;

        public void Initialize(ChatCoordinates plugin)
        {
            _plugin = plugin;
        }

        public void Save()
        {
            _plugin.Interface.SavePluginConfig(this);
        }
    }
}
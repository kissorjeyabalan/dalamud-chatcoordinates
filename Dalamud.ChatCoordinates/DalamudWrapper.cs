using Dalamud.Game.Chat;
using Dalamud.Plugin;
using Lumina.Excel;

namespace ChatCoordinates
{
    public class DalamudWrapper
    {
        private DalamudPluginInterface _pi;

        public DalamudWrapper(DalamudPluginInterface pluginInterface)
        {
            _pi = pluginInterface;
        }

        public ExcelSheet<T> GetExcelSheet<T>() where T : class, IExcelRow
        {
            return _pi.Data.GetExcelSheet<T>();
        }

        public void PrintChat(XivChatEntry message)
        {
            _pi.Framework.Gui.Chat.PrintChat(message);
        }

        public void Print(string message)
        {
            _pi.Framework.Gui.Chat.Print(message);
        }

        public void PrintError(string message)
        {
            _pi.Framework.Gui.Chat.PrintError(message);
        }
    }
}
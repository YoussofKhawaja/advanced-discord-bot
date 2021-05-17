using System.Threading.Tasks;
using ADB.FilesManager;
using ADB.Handlers;
using ADB.Services;

namespace ADB
{
    public static class SetupBot
    {
        public static async void InitBot()
        {
            //init lavallink
            ProcessManager.LaunchLavaLink();
            //init
            await new DiscordService().InitializeAsync();
        }

        public static void LogPopup(string text)
        {
            App.mainWindow.DisplayConsole(text);
        }

        public static async Task RestartBot()
        {
            DisconnectBot();
            await Task.Delay(1500);
            InitBot();
        }

        public static void DisconnectBot()
        {
            if (CommandHandler.instance != null)
                CommandHandler.instance.DisconnectBotAsync();
        }
    }
}
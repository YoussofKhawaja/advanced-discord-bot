using DiscordClient.Handlers;
using DiscordClient.Models;
using DiscordClient.Services;
using System.Threading.Tasks;
using Tools.FilesManager;

namespace DiscordClient
{
    public static class Bot
    {
        public static BotData AppData = new BotData();
        public static bool IsRunning = false;
        public static async Task<bool> Init()
        {
            SaveData();

            //init lavalink (music)
            Process.LaunchLavaLink();

            try
            {
                //init discord services
                await new DiscordService().InitializeAsync();
            }
            catch
            {
                return false;
            }

            while (!IsRunning)
            {
                await Task.Delay(1000);
            }

            return true;
        }

        public static void SaveData()
        {
            Save.WriteToXmlFile<BotData>(AppData, "Config", "ConfigData");
        }

        public static void LoadData()
        {
            AppData = Save.ReadFromXmlFile<BotData>("Config", "ConfigData");
        }

        public static void Disconnect()
        {
            if (CommandHandler.instance != null)
                CommandHandler.instance.DisconnectBotAsync();
        }

        public static string GetTotalBotGuilds()
        {
            return CommandHandler.instance._client.Guilds.Count.ToString();
        }
    }
}
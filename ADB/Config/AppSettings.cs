using System.Collections.Generic;

namespace ADB.Config
{
    public static class AppSettings
    {
        //bot config
        public static string clientId { get; set; }
        public static string appId { get; set; }
        public static string appSecret { get; set; }
        public static string CommandPrefix { get; set; } 
        public static string token { get; set; }
        public static string GameStatus { get; set; } = CommandPrefix + "help";
        public static List<ulong> BlacklistedChannels { get; set; }
    }
}
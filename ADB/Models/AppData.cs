using System;
using System.Collections.Generic;

namespace ADB.Models
{
    [Serializable]
    public class AppData
    {
        public string appId { get; set; }
        public string appSecret { get; set; }
        public string CommandPrefix { get; set; }
        public string token { get; set; }
        public static string GameStatus { get; set; }
        public static List<ulong> BlacklistedChannels { get; set; }
        public string steamtoken { get; set; }
        public string trackerggtoken { get; set; }
    }

    [Serializable]
    public class AnalyticsData
    {
        public string Server { get; set; }
    }
}
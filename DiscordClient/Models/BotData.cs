using System;
using System.Collections.Generic;

namespace DiscordClient.Models
{
    [Serializable]
    public class BotData
    {
        public string BotName { get; set; }
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string Prefix { get; set; }
        public string Token { get; set; }
        public string GameStatus { get; set; }
        public ulong OwnerClientId { get; set; }
        public List<ulong> BlacklistedChannels { get; set; }
        public List<ulong> AdminlistedUsers { get; set; }
    }
}
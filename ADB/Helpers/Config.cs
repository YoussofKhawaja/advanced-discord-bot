﻿using System.IO;
using Newtonsoft.Json;

namespace ADB.Helpers
{
    public sealed class Config
    {
        private static Config m_Instance;
        private static string m_FileName;

        private Config()
        {
        }

        public static Config Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new Config();
                return m_Instance;
            }
        }

        public void Read(string filename)
        {
            if (!File.Exists(filename)) return;
            if (m_Instance == null) return;
            m_FileName = filename;
            JsonConvert.PopulateObject(File.ReadAllText(filename), m_Instance);
        }

        public void Write()
        {
            File.WriteAllText(m_FileName, JsonConvert.SerializeObject(m_Instance));
        }

        [JsonProperty]
        public string ApiKey { get; set; }

        [JsonProperty]
        public string DiscordToken { get; set; }

        [JsonProperty]
        public char Prefix { get; set; }

        [JsonProperty]
        public string DownloadPath { get; set; }
    }
}
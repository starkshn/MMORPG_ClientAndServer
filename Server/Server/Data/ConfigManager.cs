using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Data
{
    // Server 경로를 설정하기 위함
    [Serializable]
    public class ServerConfig
    {
        public string dataPath;
    }

    public class ConfigManager
    {
        public static ServerConfig Config { get; private set; }
        public static void LoadConfig()
        {
            string text = File.ReadAllText("config.json");
            Config = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerConfig>(text);
        }
    }
}

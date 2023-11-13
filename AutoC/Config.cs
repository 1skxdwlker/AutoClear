using Newtonsoft.Json;
using System.Globalization;
using System.IO;
using System.Text;
using TShockAPI.Configuration;
using TShockAPI.Handlers.NetModules;

namespace AutoClear
{
    public class Config
    {
        public bool EnabledProjClear = true;
        public bool Enabled = true;
        public bool SendMessage = true;
        public int ClearInterval = 600000;
        public int ClearRadius = 8500;
        public void Write(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        public static Config Read(string path)
        {
            Config config = new();
            if (!File.Exists(path))
            {
                config.Write(path);
                return config;
            }
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
            return config;
        }
    }
}


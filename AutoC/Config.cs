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
        public bool AutoClearEnableProjectiles = true;
        public bool AutoClearEnabled = true;
        public bool AutoClearMessage = true;
        public int AutoClearInterval = 600000;
        public int AutoClearRadius = 40000;
        public void Write(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        public static Config Read(string path)
        {
            return File.Exists(path) ? JsonConvert.DeserializeObject<Config>(File.ReadAllText(path)) : new Config();
        }
    }
}


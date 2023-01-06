using TShockAPI;
using TerrariaApi;
using Terraria;
using TerrariaApi.Server;
using System;
using System.Timers;
using Microsoft.Xna.Framework;
using System.Reflection.Metadata.Ecma335;
using Terraria.ID;
using Microsoft.VisualBasic;
using TShockAPI.Configuration;
using System.Text;
using TShockAPI.Hooks;

namespace AutoClear
{
    [ApiVersion(2,1)]
    public class AutoClear : TerrariaPlugin
    {
    
        public override string Author => "Raiden (theraintransformend)";
        public override string Name => "AutoClear";
        public override Version Version => new Version(1, 1);
        public AutoClear(Main main) : base(main) { }
        public static Config Config = new Config();
        public static string ConfigPath { get { return Path.Combine(TShock.SavePath, "AutoClear.json"); } }
        private const int V = 1000;
        static readonly System.Timers.Timer timer = new System.Timers.Timer(1000);
        public override void Initialize()
        {
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            GeneralHooks.ReloadEvent += OnReload;
            timer.Elapsed += OnUpdate;
        }
        public const int SendCount = 5;
        public static void Wait(double timeInSec)
        {
            Thread.Sleep((int)(timeInSec * 1000));
        }
        public void OnUpdate(object Sender, EventArgs e)
        {
            if (Config.AutoClear)
            {
                string item = Config.ItemIndMessage;
                timer.Interval = Config.Interval;
                timer.Start();
                var everyone = TSPlayer.All;
                int radius = 40000;
                int cleared = 0;
                int i = 0;
                Task.Delay(Config.Interval);
                TSPlayer.All.SendInfoMessage("{0}[c/DFBF9F:Очистка мира через] [c/FF3867:10] [c/DFBF9F:секунд...]", item);
                Thread.Sleep(10000);
                for (; i < Main.maxItems; i++)
                {
                    float dX = Main.item[i].position.X - TSPlayer.Server.X;
                    float dY = Main.item[i].position.Y - TSPlayer.Server.Y;
                    if (Main.item[i].active && dX * dX + dY * dY <= radius * radius * 256f)
                    {
                        Main.item[i].active = false;
                        everyone.SendData(PacketTypes.ItemDrop, "", i);
                        cleared++;
                    }
                }
                TSPlayer.All.SendInfoMessage("{0}[c/DFBF9F:Очистка мира... Удаленно предметов:] [c/FF3867:{1}] [c/DFBF9F:В радиусе:] [c/FF3867:{2}] [c/DFBF9F:блоков]", item, cleared, radius );
                if (Config.Debug)
                {
                    Console.WriteLine("[AutoClear Debug]: Cleared items: {0} With radius: {1}", cleared, radius);
                }
            }
        }                          
        static public void OnInitialize(EventArgs args)
        {
            try
            {
                bool writeConfig = true;
                if (File.Exists(ConfigPath))
                {
                    string path = Path.Combine(TShock.SavePath, "AutoClear.json");
                    Config = Config.Read(path);
                }
                if (writeConfig)
                {
                    Config.Write(ConfigPath);
                }
                if (!File.Exists(TShock.SavePath))
                {
                    Path.Combine(TShock.SavePath, "AutoClear.json");
                    Directory.CreateDirectory(TShock.SavePath);
                }
            }
            catch (Exception ex)
            {
                Config = new Config();
                TShock.Log.ConsoleError("[AutoClear] Failed load config".SFormat(ex.ToString()));
            }
        }
        public void OnPostInitialize(EventArgs args)
        {
            timer.Start();
            //timer.Elapsed += OnUpdate;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnPostInitialize);
                GeneralHooks.ReloadEvent -= OnReload;
                timer.Elapsed -= OnUpdate;
                timer.Stop();
            }
            base.Dispose(disposing);
        }
        public static async void OnReload(ReloadEventArgs e)
        {
            try
            {
                bool writeConfig = true;
                if (File.Exists(ConfigPath))
                {
                    string path = Path.Combine(TShock.SavePath, "AutoClear.json");
                    Config = Config.Read(path);
                }
                if (writeConfig)
                {
                    Config.Write(ConfigPath);
                }
                if (!File.Exists(TShock.SavePath))
                {
                    Path.Combine(TShock.SavePath, "AutoClear.json");
                    Directory.CreateDirectory(TShock.SavePath);
                }
                e.Player.SendSuccessMessage("[AutoClear] Successfully reloaded config");
            }
            catch (Exception ex)
            {
                Config = new Config();
                TShock.Log.ConsoleError("[AutoClear] Failed Reload config".SFormat(ex.ToString()));
            }
        }
    }
}
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using TShockAPI.Hooks;
using System.Timers;

namespace AutoClear
{
    [ApiVersion(2,1)]
    public class AutoClear : TerrariaPlugin
    {
        public override string Author => "Raiden (theraintransformend)"; //https://terraria.by
        public override string Name => "AutoClear";
        public override Version Version => new Version(1, 2);
        public AutoClear(Main main) : base(main) { }
        public static string ConfigPath { get { return Path.Combine(TShock.SavePath, "AutoClear.json"); } }
        public static Config config;
        public override void Initialize()
        {
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            GeneralHooks.ReloadEvent += OnReload;
        }
        public static System.Timers.Timer Timer_ = new System.Timers.Timer(1000);
        public const int TimerReset = 100;
        private static async void OnUpdate(object? sender, ElapsedEventArgs args)
        {
            int radius = config.AutoClearRadius;
            Timer_.Interval = config.AutoClearInterval;
            if (config.AutoClearEnabled)
            {
                int clearedItems = 0;
                if (config.AutoClearRadius > 45000)
                    TShock.Log.ConsoleError("The cleaning radius should not exceed 45000!");
                else
                {
                    if (config.AutoClearMessage)
                        TSPlayer.All.SendInfoMessage("[i:4460][c/DFBF9F: Clearing the map in][c/FF3867: 10][c/DFBF9F: seconds...]");
                    await Task.Delay(10000);
                    for (int items = 0; items < Main.maxItems; items++)
                    {
                        float dX = Main.item[items].position.X - TSPlayer.Server.X;
                        float dY = Main.item[items].position.Y - TSPlayer.Server.Y;
                        if (Main.item[items].active && dX * dX + dY * dY <= radius * radius * 256f)
                        {
                            Main.item[items].active = false;
                            TSPlayer.All.SendData(PacketTypes.ItemDrop, null, items);
                            clearedItems++;
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[AutoClear Debug]: Cleared items: {0} In radius: {1}", clearedItems, radius);
                    Console.ResetColor();
                    if (config.AutoClearMessage)
                        TSPlayer.All.SendInfoMessage("[i:4460][c/DFBF9F: Deleted items:] [c/FF3867:{0}] [c/DFBF9F:Within a radius of:] [c/FF3867:{1}] [c/DFBF9F:blocks]", clearedItems, radius);
                }
            }
            if (config.AutoClearEnableProjectiles)
            {
                int clearedProjectiles = 0;
                int projectiles = 0;
                for (; projectiles < Main.maxItems; projectiles++)
                {
                    float dX = Main.projectile[projectiles].position.X - TSPlayer.Server.X;
                    float dY = Main.projectile[projectiles].position.Y - TSPlayer.Server.Y;
                    if (Main.projectile[projectiles].active && dX * dX + dY * dY <= radius * radius * 256f)
                    {
                        Main.projectile[projectiles].active = false;
                        TSPlayer.All.SendData(PacketTypes.ProjectileNew, null, projectiles);
                        clearedProjectiles++;
                    }
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[AutoClear Debug]: Cleared Projectiles: {0} In radius: {1}", clearedProjectiles, radius);
                Console.ResetColor();
                if (config.AutoClearMessage)
                    TSPlayer.All.SendInfoMessage("[i:4460][c/DFBF9F: Deleted projectiles:] [c/FF3867:{0}]", clearedProjectiles);
            }
        }
        private static void OnInitialize(EventArgs args)
        {
            ReadConfig();
        }
        private static void OnPostInitialize(EventArgs args)
        {
            Timer_.Elapsed += OnUpdate;
            Timer_.Start();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeneralHooks.ReloadEvent -= OnReload;
                Timer_.Elapsed -= OnUpdate;
                Timer_.Stop();
            }
            base.Dispose(disposing);
        }
        public static void ReadConfig()
        {
            try
            {
                bool writeConfig = true;
                string path = Path.Combine(TShock.SavePath, "AutoClear.json");
                config = Config.Read(path);
                if (!File.Exists(path))
                {
                    config.Write(path);
                }
                if (writeConfig)
                {
                    config.Write(ConfigPath);
                }
            }
            catch
            {
                Console.WriteLine("[AutoClear]: Failed load config!");
            }
        }
        public static void OnReload(ReloadEventArgs args)
        {
            try
            {
                ReadConfig();
                args.Player.SendSuccessMessage("[AutoClear] Succesfully reloaded config.");
                Timer_.Interval = TimerReset;
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[AutoClear]: Failed load config!");
                Console.ResetColor();
            }
        }
    }
}
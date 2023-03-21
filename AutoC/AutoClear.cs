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
        public override string Author => "Raiden (theraintransformend)"; //terraria.by
        public override string Name => "AutoClear";
        public override Version Version => new Version(1, 2);
        public AutoClear(Main main) : base(main) { }

        public static System.Timers.Timer Timer_ = new System.Timers.Timer(1000);
        public static string ConfigPath { get { return Path.Combine(TShock.SavePath, "AutoClear.json"); } }
        public static Config config;

        #region Initialize & Disposing
        public override void Initialize()
        {
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            GeneralHooks.ReloadEvent += OnReload;
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
        #endregion

        private static async void OnUpdate(object? sender, ElapsedEventArgs args)
        {
            int radius = config.AutoClearRadius;
            Timer_.Interval = config.AutoClearInterval;
            if (config.AutoClearRadius > 45000)
            {
                TShock.Log.ConsoleError("The cleaning radius should not exceed 45000!");
                return;
            }
            if (config.AutoClearEnabled)
            {
                int clearedItems = 0;
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
            if (config.AutoClearEnableProjectiles)
            {
                int clearedProjectiles = 0;
                for (int projectiles = 0; projectiles < Main.maxItems; projectiles++)
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
        public static void OnReload(ReloadEventArgs args)
        {
            ReadConfig();
            args.Player.SendSuccessMessage("[AutoClear] Succesfully reloaded config.");
            Timer_.Interval = 100;
        }
        public static void ReadConfig()
        {
            try
            {
                string path = Path.Combine(TShock.SavePath, "AutoClear.json");
                config = Config.Read(path);
                if (!File.Exists(path))
                {
                    config.Write(path);
                }
            }
            catch
            {
                Console.WriteLine("[AutoClear]: Failed load config!");
            }
        }
    }
}
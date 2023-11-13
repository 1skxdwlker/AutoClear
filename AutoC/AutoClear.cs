using System.Timers;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace AutoClear
{
    [ApiVersion(2,1)]
    public class AutoClear : TerrariaPlugin
    {
        public override string Author => "Raiden"; //terraria.by
        public override string Name => "AutoClear";
        public override Version Version => new Version(1, 2);
        public AutoClear(Main main) : base(main) { }

        public System.Timers.Timer timer = new System.Timers.Timer(1000);
        public string SavePath => Path.Combine(TShock.SavePath, "AutoClear.json");
        public Config config;

        #region Initialize & Disposing
        public override void Initialize()
        {
            config = Config.Read(SavePath);
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
            GeneralHooks.ReloadEvent += OnReload;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeneralHooks.ReloadEvent -= OnReload;
                timer.Elapsed -= OnUpdate;
                timer.Stop();
            }
            base.Dispose(disposing);
        }
        #endregion

        private async void OnUpdate(object? sender, ElapsedEventArgs args)
        {
            timer.Interval = config.ClearInterval;
            int radius = config.ClearRadius;
            if (config.ClearRadius > 45000)
            {
                config.ClearRadius = 8500;
                config.Write(SavePath);
                config = Config.Read(SavePath);
                radius = config.ClearRadius;
            }
            if (config.Enabled)
            {
                int clearedItems = 0;
                if (config.SendMessage)
                    TSPlayer.All.SendInfoMessage("[i:4460][c/DFBF9F: Clearing the map in][c/FF3867: 10][c/DFBF9F: seconds...]");
                await Task.Delay(10000);
                for (int i = 0; i < Main.maxItems; i++)
                {
                    float dX = Main.item[i].position.X - TSPlayer.Server.X;
                    float dY = Main.item[i].position.Y - TSPlayer.Server.Y;
                    if (Main.item[i].active && dX * dX + dY * dY <= radius * radius * 256f)
                    {
                        Main.item[i].active = false;
                        TSPlayer.All.SendData(PacketTypes.UpdateItemDrop, null, i);
                        clearedItems++;
                    }
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[AutoClear info]: Cleared i: {0} In radius: {1}", clearedItems, radius);
                Console.ResetColor();
                if (config.SendMessage)
                    TSPlayer.All.SendInfoMessage("[i:4460][c/DFBF9F: Deleted i:] [c/FF3867:{0}] [c/DFBF9F:Within a radius of:] [c/FF3867:{1}] [c/DFBF9F:blocks]", clearedItems, radius);
            }
            if (config.EnabledProjClear)
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
                Console.WriteLine("[AutoClear Info]: Cleared Projectiles: {0} In radius: {1}", clearedProjectiles, radius);
                Console.ResetColor();
                if (config.SendMessage)
                    TSPlayer.All.SendInfoMessage("[i:4460][c/DFBF9F: Deleted projectiles:] [c/FF3867:{0}]", clearedProjectiles);
            }
        }
        private void OnPostInitialize(EventArgs args)
        {
            timer.Elapsed += OnUpdate;
            timer.Start();
        }
        public void OnReload(ReloadEventArgs args)
        {
            try
            {
                config = Config.Read(SavePath);
                args.Player.SendSuccessMessage("[AutoClear] Successfully reloaded config.");
                timer.Interval = 100;
            }
            catch (Exception ex)
            {
                config.Write(SavePath);
                config = Config.Read(SavePath);
                args.Player.SendErrorMessage(ex.Message);
            }
        }
    }
}
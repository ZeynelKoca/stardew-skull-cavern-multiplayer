using System;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace SkullCavernTimeFixMultiplayer
{
    public sealed class ModEntry : Mod
    {
        private ModConfig? Config;
        private bool canApply = true;
        private int logCounter = 0;

        public override void Entry(IModHelper helper)
        {
            this.Monitor.Log("=== MOD ENTRY CALLED ===", LogLevel.Info);

            try
            {
                Config = this.Helper.ReadConfig<ModConfig>();
                this.Monitor.Log($"Config loaded: Enabled={Config.Enabled}, Verbose={Config.VerboseLogging}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Failed to load config: {ex.Message}", LogLevel.Error);
                Config = new ModConfig();
            }

            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            this.Monitor.Log("Skull Cavern Time Fix loaded successfully!", LogLevel.Info);
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            // Log every 60 ticks (once per second) for debugging
            logCounter++;
            if (logCounter >= 60)
            {
                logCounter = 0;
                this.Monitor.Log($"[DEBUG] IsWorldReady={Context.IsWorldReady}, IsMainPlayer={Context.IsMainPlayer}, HasRemotePlayers={Context.HasRemotePlayers}", LogLevel.Debug);
            }

            if (!Context.IsWorldReady)
                return;

            if (!Context.IsMainPlayer)
            {
                if (logCounter == 0)
                    this.Monitor.Log("[DEBUG] Not main player, skipping", LogLevel.Debug);
                return;
            }

            if (!Context.HasRemotePlayers)
            {
                if (logCounter == 0)
                    this.Monitor.Log("[DEBUG] No remote players, skipping", LogLevel.Debug);
                return;
            }

            if (!Config!.Enabled)
            {
                if (logCounter == 0)
                    this.Monitor.Log("[DEBUG] Mod disabled in config", LogLevel.Debug);
                return;
            }

            // Get all farmers except the host
            var remoteFarmers = Game1.getOnlineFarmers()
                .Where(f => f.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
                .ToList();

            if (remoteFarmers.Count == 0)
            {
                if (logCounter == 0)
                    this.Monitor.Log("[DEBUG] No remote farmers found", LogLevel.Debug);
                return;
            }

            // Count how many remote players are in Skull Cavern
            int inSkullCavern = remoteFarmers.Count(f =>
                f.currentLocation is MineShaft ms && ms.getMineArea() == 121);

            if (logCounter == 0)
                this.Monitor.Log($"[DEBUG] Remote players: {remoteFarmers.Count}, In Skull Cavern: {inSkullCavern}", LogLevel.Info);

            if (inSkullCavern == 0)
                return;

            // Calculate percentage
            double percentageInCavern = (double)inSkullCavern / remoteFarmers.Count;

            if (percentageInCavern < Config.MinimumPlayerPercentage)
            {
                this.Monitor.Log($"[DEBUG] Percentage {percentageInCavern:P0} below threshold {Config.MinimumPlayerPercentage:P0}", LogLevel.Debug);
                return;
            }

            // Apply slowdown when interval is high enough
            if (canApply && Game1.gameTimeInterval > 4000)
            {
                int slowdownAmount = (int)(Config.SlowdownMultiplier * 1000 * percentageInCavern);
                Game1.gameTimeInterval += slowdownAmount;

                this.Monitor.Log(
                    $"Applied Skull Cavern slowdown: {inSkullCavern}/{remoteFarmers.Count} players inside. " +
                    $"Added {slowdownAmount}ms (now: {Game1.gameTimeInterval}ms)",
                    LogLevel.Info
                );

                canApply = false;
            }

            // Reset gate when interval is low
            if (!canApply && Game1.gameTimeInterval < 2000)
            {
                canApply = true;
            }
        }
    }

    public sealed class ModConfig
    {
        public bool Enabled { get; set; } = true;
        public double SlowdownMultiplier { get; set; } = 2.0;
        public double MinimumPlayerPercentage { get; set; } = 0.5;
        public bool VerboseLogging { get; set; } = false;
    }
}
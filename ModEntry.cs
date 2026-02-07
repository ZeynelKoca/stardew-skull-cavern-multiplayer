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
        private int baseTimeInterval = 0;

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
                if (Config!.VerboseLogging)
                {
                    this.Monitor.Log($"[DEBUG] IsWorldReady={Context.IsWorldReady}, IsMainPlayer={Context.IsMainPlayer}, HasRemotePlayers={Context.HasRemotePlayers}", LogLevel.Debug);
                }
            }

            if (!Context.IsWorldReady)
                return;

            if (!Context.IsMainPlayer)
            {
                if (logCounter == 0 && Config!.VerboseLogging)
                    this.Monitor.Log("[DEBUG] Not main player, skipping", LogLevel.Debug);
                return;
            }

            if (!Context.HasRemotePlayers)
            {
                if (logCounter == 0 && Config!.VerboseLogging)
                    this.Monitor.Log("[DEBUG] No remote players, skipping", LogLevel.Debug);
                return;
            }

            if (!Config!.Enabled)
            {
                if (logCounter == 0 && Config.VerboseLogging)
                    this.Monitor.Log("[DEBUG] Mod disabled in config", LogLevel.Debug);
                return;
            }

            // Get all farmers EXCEPT the host (exclude the bot)
            var nonHostFarmers = Game1.getOnlineFarmers()
                .Where(f => f.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
                .ToList();

            if (nonHostFarmers.Count == 0)
            {
                if (logCounter == 0 && Config.VerboseLogging)
                    this.Monitor.Log("[DEBUG] No non-host farmers found", LogLevel.Debug);
                return;
            }

            // Count how many NON-HOST players are in Skull Cavern
            int inSkullCavern = nonHostFarmers.Count(f =>
                f.currentLocation is MineShaft ms && ms.getMineArea() == 121);

            if (logCounter == 0)
                this.Monitor.Log($"[DEBUG] Non-host players: {nonHostFarmers.Count}, In Skull Cavern: {inSkullCavern}, gameTimeInterval: {Game1.gameTimeInterval}ms", LogLevel.Info);

            // Apply time adjustment when interval is high enough (ready for next time tick)
            if (canApply && Game1.gameTimeInterval > 4000)
            {
                // Store the base interval before we modify it
                baseTimeInterval = Game1.gameTimeInterval > 6800 ? 6800 : Game1.gameTimeInterval;

                // Calculate the slowdown based on the proportion of NON-HOST players in Skull Cavern
                // Base slowdown is 2000ms when ALL non-host players are in Skull Cavern
                // This is proportionally scaled based on how many are inside
                int slowdownAmount = 2000 * inSkullCavern / nonHostFarmers.Count;

                // SUBTRACT from interval to make time pass slower (counter-intuitive but correct)
                Game1.gameTimeInterval -= slowdownAmount;

                // Clamp to prevent negative intervals (safety check)
                if (Game1.gameTimeInterval < 0)
                {
                    this.Monitor.Log($"WARNING: Interval went negative ({Game1.gameTimeInterval}ms), clamping to 0", LogLevel.Warn);
                    Game1.gameTimeInterval = 0;
                }

                if (slowdownAmount > 0)
                {
                    this.Monitor.Log(
                        $"Applied Skull Cavern slowdown: {inSkullCavern}/{nonHostFarmers.Count} non-host players inside. " +
                        $"Reduced interval by {slowdownAmount}ms (from {baseTimeInterval}ms to {Game1.gameTimeInterval}ms)",
                        LogLevel.Info
                    );
                }

                canApply = false;
            }

            // Reset gate when interval is low (after time has advanced)
            if (!canApply && Game1.gameTimeInterval < 1000)
            {
                canApply = true;
            }
        }
    }

    public sealed class ModConfig
    {
        public bool Enabled { get; set; } = true;
        public bool VerboseLogging { get; set; } = false;
    }
}
using System;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace SkullCavernTimeFixMultiplayer
{
    public sealed class ModEntry : Mod
    {
        private static IMonitor? StaticMonitor;
        private static ModConfig? Config;

        public override void Entry(IModHelper helper)
        {
            StaticMonitor = this.Monitor;
            Config = this.Helper.ReadConfig<ModConfig>();

            // Apply Harmony patches
            var harmony = new Harmony(this.ModManifest.UniqueID);

            // Patch the performTenMinuteClockUpdate method which handles time advancement
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.performTenMinuteClockUpdate)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(PerformTenMinuteClockUpdate_Prefix))
            );

            this.Monitor.Log("Skull Cavern Time Fix loaded successfully!", LogLevel.Info);

            if (Config.VerboseLogging)
            {
                helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            }
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            // Debug logging every second
            if (!e.IsMultipleOf(60))
                return;

            if (!Context.IsWorldReady || !Context.IsMainPlayer || !Context.HasRemotePlayers)
                return;

            var onlineFarmers = Game1.getOnlineFarmers().ToList();
            var remoteFarmers = onlineFarmers.Where(f => f.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID).ToList();

            int inSkullCavern = remoteFarmers.Count(f =>
                f.currentLocation is MineShaft ms && ms.getMineArea() == 121);

            if (inSkullCavern > 0)
            {
                this.Monitor.Log(
                    $"[DEBUG] Total online: {onlineFarmers.Count}, Remote players: {remoteFarmers.Count}, " +
                    $"In Skull Cavern: {inSkullCavern}, gameTimeInterval: {Game1.gameTimeInterval}",
                    LogLevel.Debug
                );
            }
        }

        /// <summary>
        /// Prefix patch for Game1.performTenMinuteClockUpdate to apply custom time slowdown logic
        /// </summary>
        private static bool PerformTenMinuteClockUpdate_Prefix()
        {
            try
            {
                if (!Context.IsWorldReady || !Context.IsMainPlayer)
                    return true; // Run original method

                if (!Config!.Enabled)
                    return true; // Mod disabled, run original

                // Only apply in multiplayer with remote players
                if (!Context.HasRemotePlayers)
                    return true;

                // Get all farmers except the host
                var remoteFarmers = Game1.getOnlineFarmers()
                    .Where(f => f.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
                    .ToList();

                if (remoteFarmers.Count == 0)
                    return true;

                // Count how many remote players are in Skull Cavern
                int inSkullCavern = remoteFarmers.Count(f =>
                    f.currentLocation is MineShaft ms && ms.getMineArea() == 121);

                // If no remote players in Skull Cavern, use normal time
                if (inSkullCavern == 0)
                    return true;

                // Calculate the percentage of remote players in Skull Cavern
                double percentageInCavern = (double)inSkullCavern / remoteFarmers.Count;

                // Apply time slowdown proportional to how many players are in Skull Cavern
                // In vanilla, Skull Cavern slows time by approximately 50%
                // We scale this based on percentage of players inside
                if (percentageInCavern >= Config.MinimumPlayerPercentage)
                {
                    // Apply the slowdown by increasing gameTimeInterval
                    // Higher interval = slower time passage
                    int slowdownAmount = (int)(Config.SlowdownMultiplier * 1000 * percentageInCavern);
                    Game1.gameTimeInterval += slowdownAmount;

                    if (Config.VerboseLogging && StaticMonitor != null)
                    {
                        StaticMonitor.Log(
                            $"Applied Skull Cavern slowdown: {inSkullCavern}/{remoteFarmers.Count} players inside. " +
                            $"Added {slowdownAmount}ms to gameTimeInterval (now: {Game1.gameTimeInterval}ms)",
                            LogLevel.Trace
                        );
                    }
                }

                return true; // Always run the original method
            }
            catch (Exception ex)
            {
                StaticMonitor?.Log($"Error in PerformTenMinuteClockUpdate_Prefix: {ex}", LogLevel.Error);
                return true; // Run original on error
            }
        }
    }

    public sealed class ModConfig
    {
        /// <summary>Enable or disable the mod</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Multiplier for time slowdown (default 2.0 = ~50% slowdown like vanilla)</summary>
        public double SlowdownMultiplier { get; set; } = 2.0;

        /// <summary>Minimum percentage of remote players that need to be in Skull Cavern to apply slowdown (0.5 = 50%)</summary>
        public double MinimumPlayerPercentage { get; set; } = 0.5;

        /// <summary>Enable verbose debug logging</summary>
        public bool VerboseLogging { get; set; } = false;
    }
}

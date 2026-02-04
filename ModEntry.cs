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

        public override void Entry(IModHelper helper)
        {
            Config = this.Helper.ReadConfig<ModConfig>();
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            this.Monitor.Log("Skull Cavern Time Fix loaded successfully!", LogLevel.Info);
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsMainPlayer || !Context.HasRemotePlayers)
                return;

            if (!Config!.Enabled)
                return;

            // Get all farmers except the host
            var remoteFarmers = Game1.getOnlineFarmers()
                .Where(f => f.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
                .ToList();

            if (remoteFarmers.Count == 0)
                return;

            // Count how many remote players are in Skull Cavern
            int inSkullCavern = remoteFarmers.Count(f =>
                f.currentLocation is MineShaft ms && ms.getMineArea() == 121);

            if (inSkullCavern == 0)
                return;

            // Calculate percentage
            double percentageInCavern = (double)inSkullCavern / remoteFarmers.Count;

            if (percentageInCavern < Config.MinimumPlayerPercentage)
                return;

            // Apply slowdown when interval is high enough
            if (canApply && Game1.gameTimeInterval > 4000)
            {
                int slowdownAmount = (int)(Config.SlowdownMultiplier * 1000 * percentageInCavern);
                Game1.gameTimeInterval += slowdownAmount;

                if (Config.VerboseLogging)
                {
                    this.Monitor.Log(
                        $"Applied Skull Cavern slowdown: {inSkullCavern}/{remoteFarmers.Count} players inside. " +
                        $"Added {slowdownAmount}ms (now: {Game1.gameTimeInterval}ms)",
                        LogLevel.Debug
                    );
                }

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
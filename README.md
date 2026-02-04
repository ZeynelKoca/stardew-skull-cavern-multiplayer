# Skull Cavern Time Fix for Multiplayer

A Stardew Valley mod that fixes the Skull Cavern time slowdown in multiplayer when using a headless host bot.

## The Problem

In vanilla multiplayer Stardew Valley, time only slows down in Skull Cavern when **ALL** players are inside it. This breaks when you have a headless host bot (like when running "Always On Server" mod) because:
- The host bot never moves or teleports
- The bot is never in Skull Cavern
- Therefore, time never slows down even when all human players are inside

## The Solution

This mod ignores the host player when calculating Skull Cavern time slowdown, effectively counting only the remote (human) players. When remote players enter Skull Cavern, time will slow down proportionally.

## Features

- **Smart Detection**: Only counts remote players, completely ignoring the headless host
- **Proportional Slowdown**: Time slows down based on the percentage of players in Skull Cavern
- **Configurable**: Adjust slowdown multiplier and minimum player threshold
- **Lightweight**: Uses Harmony patches for efficient, clean integration
- **Debug Logging**: Optional verbose mode for troubleshooting

## Installation

### Standard Installation (Non-Docker)

1. Install [SMAPI](https://smapi.io/) (version 4.0.0 or later)
2. Download the latest release
3. Extract the mod folder into your `Stardew Valley/Mods` directory
4. Run the game through SMAPI

### Docker Installation (cavazos-apps/stardew-multiplayer-docker)

If you're using the Docker setup, see **[DOCKER_INTEGRATION.md](DOCKER_INTEGRATION.md)** for detailed instructions.

**Quick Docker Install:**
```bash
./install-to-docker.sh /path/to/stardew-multiplayer-docker
```

## Building from Source

### Prerequisites
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download)
- Stardew Valley with SMAPI installed

### Build Steps

```bash
# Clone the repository
git clone <your-repo-url>
cd stardew-multiplayer-time-mod

# Build
dotnet build -c Release

# The compiled mod will be in: bin/Release/net6.0/
```

**Note:** The build requires Stardew Valley to be installed. If building on a machine without the game, it will fail. Build on your server/VM where the game is installed.

### Manual Installation After Building

```bash
# Copy the DLL and manifest to your Mods folder
cp bin/Release/net6.0/SkullCavernTimeFixMultiplayer.dll "~/StardewValley/Mods/SkullCavernTimeFixMultiplayer/"
cp manifest.json "~/StardewValley/Mods/SkullCavernTimeFixMultiplayer/"
```

## Configuration

After running the game once with the mod installed, a `config.json` file will be created in the mod folder. You can edit it to customize behavior:

```json
{
  "Enabled": true,
  "SlowdownMultiplier": 2.0,
  "MinimumPlayerPercentage": 0.5,
  "VerboseLogging": false
}
```

### Config Options

- **Enabled**: Turn the mod on/off without removing it (default: `true`)
- **SlowdownMultiplier**: How much to slow down time (default: `2.0` ≈ 50% slowdown like vanilla)
  - `1.0` = 0% slowdown (no effect)
  - `2.0` = 50% slowdown (vanilla Skull Cavern speed)
  - `4.0` = 75% slowdown (very slow)
- **MinimumPlayerPercentage**: Minimum % of remote players needed in Skull Cavern to apply slowdown (default: `0.5` = 50%)
  - `0.0` = Any player in cavern triggers slowdown
  - `0.5` = At least 50% of players must be in cavern
  - `1.0` = All players must be in cavern (like vanilla)
- **VerboseLogging**: Enable detailed debug logs (default: `false`)

### Example Scenarios

**Just you and your partner (2 players + host bot):**
- Both in Skull Cavern: 100% slowdown applied ✅
- One in Skull Cavern: 50% slowdown applied ✅ (with default 50% threshold)
- None in Skull Cavern: No slowdown ❌

**Three players + host bot:**
- All 3 in Skull Cavern: 100% slowdown ✅
- 2 of 3 in Skull Cavern: 66% slowdown ✅
- 1 of 3 in Skull Cavern: No slowdown ❌ (below 50% threshold)

Change `MinimumPlayerPercentage` to `0.33` to allow slowdown with just 1 of 3 players.

## Compatibility

- **Stardew Valley**: 1.6+
- **SMAPI**: 4.0.0+
- **Multiplayer**: Required (this is a multiplayer-only mod)
- **Other Mods**: Should be compatible with most mods including Always On Server, Auto Load Game, TimeSpeed, etc.

## Troubleshooting

### Time still not slowing down?

1. **Verify you're the host**: This mod only works on the host/server
2. **Check SMAPI console**: Look for `[SkullCavernTimeFixMultiplayer] Skull Cavern Time Fix loaded successfully!`
3. **Enable verbose logging**: Set `VerboseLogging: true` in config.json and check SMAPI console for detailed messages
4. **Verify player location**: Make sure remote players are actually in Skull Cavern (area 121)
5. **Check threshold**: If using default 50% threshold, at least half your players must be in the cavern

### Mod not loading?

- Ensure SMAPI 4.0.0+ is installed
- Check SMAPI error log for any loading errors
- Verify the mod folder structure:
  ```
  Mods/
  └── SkullCavernTimeFixMultiplayer/
      ├── SkullCavernTimeFixMultiplayer.dll
      ├── manifest.json
      └── config.json (auto-generated)
  ```

### Finding SMAPI logs

- **Windows**: `%appdata%\StardewValley\ErrorLogs\SMAPI-latest.txt`
- **Linux**: `~/.config/StardewValley/ErrorLogs/SMAPI-latest.txt`
- **Mac**: `~/.config/StardewValley/ErrorLogs/SMAPI-latest.txt`
- **Docker**: Check container logs or `/config/xdg/config/StardewValley/ErrorLogs/SMAPI-latest.txt`

## How It Works

1. The mod uses Harmony to patch `Game1.performTenMinuteClockUpdate`
2. When time is about to advance, it checks:
   - Are we in multiplayer with remote players?
   - How many remote players (excluding host) are in Skull Cavern?
3. If enough players are in Skull Cavern (based on `MinimumPlayerPercentage`), it applies a time slowdown
4. The slowdown is proportional to the percentage of players inside

The host player is **completely ignored** in all calculations.

## License

MIT License - Feel free to modify and distribute

## Links

- **Nexus Mods**: [Link TBD]
- **GitHub**: [Your GitHub repo]
- **Issues**: Report bugs on GitHub or Nexus Mods

## Credits

Created for headless Stardew Valley multiplayer servers using Always On Server and Auto Load Game mods.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history.

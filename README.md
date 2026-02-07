# Skull Cavern Time Fix for Multiplayer

Fixes Skull Cavern time slowdown in multiplayer when using a headless host bot.

## The Problem

In vanilla multiplayer, time only slows in Skull Cavern when ALL players are inside. With a headless host bot, this never happens since the bot stays outside.

## The Solution

This mod completely ignores the host bot and only counts real players:
- Formula: `2000ms × (real players in SC / total real players)`
- Host bot is excluded from all calculations

## Installation

1. Install [SMAPI](https://smapi.io/) 4.0.0+
2. Extract mod to `Stardew Valley/Mods`
3. Run game through SMAPI

**Docker setup:** See [DOCKER_INTEGRATION.md](DOCKER_INTEGRATION.md)

## Building

```bash
dotnet build -c Release
```

Output: `bin/Release/net6.0/SkullCavernTimeFixMultiplayer.dll`

## Configuration

`config.json` (auto-generated):
```json
{
  "Enabled": true,
  "VerboseLogging": false
}
```

## Examples

**You + host bot:**
- You outside SC: No slowdown (0/1 = 0%)
- You inside SC: 2000ms slowdown (1/1 = 100%) ✅

**You + partner + host bot:**
- Neither in SC: No slowdown (0/2 = 0%)
- Only you in SC: 1000ms slowdown (1/2 = 50%)
- Only partner in SC: 1000ms slowdown (1/2 = 50%)
- Both in SC: 2000ms slowdown (2/2 = 100%) ✅

## Requirements

- Stardew Valley 1.6+
- SMAPI 4.0.0+
- Multiplayer mode

## Troubleshooting

Enable `VerboseLogging` in config.json and check SMAPI logs for:
```
[SkullCavernTimeFixMultiplayer] Applied Skull Cavern slowdown: 1/2 players inside
```

## License

MIT

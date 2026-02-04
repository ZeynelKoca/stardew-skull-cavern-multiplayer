# Docker Integration Guide for cavazos-apps/stardew-multiplayer-docker

This guide shows you how to add the Skull Cavern Time Fix mod to your existing Docker setup.

## Understanding the Setup

The cavazos-apps Docker setup:
1. Stores mods in `docker/mods/<ModName>/` before building
2. Copies them to the container during build
3. Processes config templates using environment variables at runtime
4. Can enable/disable mods via `ENABLE_<MODNAME>_MOD` variables

## Integration Steps

### Step 1: Build Your Mod

First, build the mod (do this on your local machine or VM):

```bash
cd /path/to/stardew-multiplayer-time-mod
dotnet build -c Release
```

This creates: `bin/Release/net6.0/SkullCavernTimeFixMultiplayer.dll`

### Step 2: Add Mod to Docker Setup

Navigate to your Docker repository and create the mod directory:

```bash
cd /path/to/stardew-multiplayer-docker
mkdir -p "docker/mods/SkullCavernTimeFixMultiplayer"
```

### Step 3: Copy Mod Files

```bash
# Copy the DLL
cp /path/to/stardew-multiplayer-time-mod/bin/Release/net6.0/SkullCavernTimeFixMultiplayer.dll \
   "docker/mods/SkullCavernTimeFixMultiplayer/"

# Copy the manifest
cp /path/to/stardew-multiplayer-time-mod/manifest.json \
   "docker/mods/SkullCavernTimeFixMultiplayer/"
```

### Step 4: Create Config Template (Optional but Recommended)

Create a config template file for environment variable support:

```bash
cat > "docker/mods/SkullCavernTimeFixMultiplayer/config.json.template" << 'EOF'
{
  "Enabled": ${SKULLCAVERNTIMEFIXMULTIPLAYER_ENABLED-true},
  "SlowdownMultiplier": ${SKULLCAVERNTIMEFIXMULTIPLAYER_SLOWDOWN_MULTIPLIER-2.0},
  "MinimumPlayerPercentage": ${SKULLCAVERNTIMEFIXMULTIPLAYER_MINIMUM_PLAYER_PERCENTAGE-0.5},
  "VerboseLogging": ${SKULLCAVERNTIMEFIXMULTIPLAYER_VERBOSE_LOGGING-false}
}
EOF
```

### Step 5: Add Environment Variables to docker-compose

Edit your `docker-compose-steam.yml` (or `docker-compose-gog.yml`) and add these lines in the `environment:` section:

```yaml
environment:
  # ... existing environment variables ...

  # Skull Cavern Time Fix Multiplayer mod
  - ENABLE_SKULLCAVERNTIMEFIXMULTIPLAYER_MOD=${ENABLE_SKULLCAVERNTIMEFIXMULTIPLAYER_MOD-true}
  - SKULLCAVERNTIMEFIXMULTIPLAYER_ENABLED=${SKULLCAVERNTIMEFIXMULTIPLAYER_ENABLED-true}
  - SKULLCAVERNTIMEFIXMULTIPLAYER_SLOWDOWN_MULTIPLIER=${SKULLCAVERNTIMEFIXMULTIPLAYER_SLOWDOWN_MULTIPLIER-2.0}
  - SKULLCAVERNTIMEFIXMULTIPLAYER_MINIMUM_PLAYER_PERCENTAGE=${SKULLCAVERNTIMEFIXMULTIPLAYER_MINIMUM_PLAYER_PERCENTAGE-0.5}
  - SKULLCAVERNTIMEFIXMULTIPLAYER_VERBOSE_LOGGING=${SKULLCAVERNTIMEFIXMULTIPLAYER_VERBOSE_LOGGING-false}
```

### Step 6: Rebuild and Restart

```bash
# Rebuild the container (required when adding new mods)
docker compose -f docker-compose-steam.yml build --no-cache

# Start the container
docker compose -f docker-compose-steam.yml up -d
```

## Verify Installation

Check the SMAPI logs to confirm the mod loaded:

```bash
docker logs stardew | grep SkullCavern
```

You should see:
```
[SkullCavernTimeFixMultiplayer] Skull Cavern Time Fix loaded successfully!
```

Or view the full log:

```bash
docker exec stardew cat /config/xdg/config/StardewValley/ErrorLogs/SMAPI-latest.txt | grep SkullCavern
```

## Configuration Options

You can customize the mod behavior by setting environment variables before starting:

```bash
# Example: Enable verbose logging
export SKULLCAVERNTIMEFIXMULTIPLAYER_VERBOSE_LOGGING=true

# Example: Require 75% of players in cavern
export SKULLCAVERNTIMEFIXMULTIPLAYER_MINIMUM_PLAYER_PERCENTAGE=0.75

# Example: Increase slowdown effect
export SKULLCAVERNTIMEFIXMULTIPLAYER_SLOWDOWN_MULTIPLIER=3.0

docker compose -f docker-compose-steam.yml up -d
```

Or edit them directly in `docker-compose-steam.yml`:

```yaml
- SKULLCAVERNTIMEFIXMULTIPLAYER_VERBOSE_LOGGING=true
- SKULLCAVERNTIMEFIXMULTIPLAYER_MINIMUM_PLAYER_PERCENTAGE=0.75
- SKULLCAVERNTIMEFIXMULTIPLAYER_SLOWDOWN_MULTIPLIER=3.0
```

## Disabling the Mod

To disable without removing:

```bash
export ENABLE_SKULLCAVERNTIMEFIXMULTIPLAYER_MOD=false
docker compose -f docker-compose-steam.yml up -d
```

Or set in docker-compose-steam.yml:
```yaml
- ENABLE_SKULLCAVERNTIMEFIXMULTIPLAYER_MOD=false
```

## Updating the Mod

To update to a new version:

```bash
# Build new version
cd /path/to/stardew-multiplayer-time-mod
dotnet build -c Release

# Copy to Docker setup
cp bin/Release/net6.0/SkullCavernTimeFixMultiplayer.dll \
   /path/to/stardew-multiplayer-docker/docker/mods/SkullCavernTimeFixMultiplayer/

# Rebuild container
cd /path/to/stardew-multiplayer-docker
docker compose -f docker-compose-steam.yml build --no-cache
docker compose -f docker-compose-steam.yml up -d
```

## Directory Structure After Integration

```
stardew-multiplayer-docker/
├── docker/
│   ├── mods/
│   │   ├── Always On Server/
│   │   ├── AutoLoadGame/
│   │   ├── SkullCavernTimeFixMultiplayer/    ← Your new mod!
│   │   │   ├── SkullCavernTimeFixMultiplayer.dll
│   │   │   ├── manifest.json
│   │   │   └── config.json.template
│   │   ├── TimeSpeed/
│   │   └── ... (other mods)
│   └── Dockerfile-steam
└── docker-compose-steam.yml
```

## Testing

1. Start your server
2. Connect with your girlfriend
3. Both enter Skull Cavern
4. Time should now slow down! 🎉

If it doesn't work:
- Enable verbose logging
- Check SMAPI logs
- Verify the mod is loaded
- Ensure you're the host player

## Troubleshooting

**Mod not loading?**
```bash
# Check if mod files exist in container
docker exec stardew ls -la /data/Stardew/game/Mods/SkullCavernTimeFixMultiplayer/
```

**Config not updating?**
- The config is only generated once at first startup
- To regenerate, delete the config and restart:
```bash
docker exec stardew rm /data/Stardew/game/Mods/SkullCavernTimeFixMultiplayer/config.json
docker restart stardew
```

**Still not working?**
```bash
# View full SMAPI log
docker exec stardew cat /config/xdg/config/StardewValley/ErrorLogs/SMAPI-latest.txt
```

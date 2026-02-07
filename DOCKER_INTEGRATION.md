# Docker Integration

For cavazos-apps/stardew-multiplayer-docker

## Quick Install

```bash
# Build the mod
dotnet build -c Release

# Copy to Docker setup
cd /path/to/stardew-multiplayer-docker
mkdir -p "docker/mods/SkullCavernTimeFixMultiplayer"

cp /path/to/mod/bin/Release/net6.0/SkullCavernTimeFixMultiplayer.dll \
   docker/mods/SkullCavernTimeFixMultiplayer/

cp /path/to/mod/manifest.json \
   docker/mods/SkullCavernTimeFixMultiplayer/

# Create config template
cat > "docker/mods/SkullCavernTimeFixMultiplayer/config.json.template" << 'EOF'
{
  "Enabled": ${SKULLCAVERNTIMEFIXMULTIPLAYER_ENABLED-true},
  "VerboseLogging": ${SKULLCAVERNTIMEFIXMULTIPLAYER_VERBOSE_LOGGING-false}
}
EOF

# Rebuild container
docker compose -f docker-compose-steam.yml build --no-cache
docker compose -f docker-compose-steam.yml up -d
```

## Configuration (Optional)

Add to `docker-compose-steam.yml`:

```yaml
environment:
  - ENABLE_SKULLCAVERNTIMEFIXMULTIPLAYER_MOD=true
  - SKULLCAVERNTIMEFIXMULTIPLAYER_ENABLED=true
  - SKULLCAVERNTIMEFIXMULTIPLAYER_VERBOSE_LOGGING=false
```

## Verify

```bash
docker logs stardew | grep SkullCavern
```

Should see: `Skull Cavern Time Fix loaded successfully!`

## Troubleshooting

Enable verbose logging:
```yaml
- SKULLCAVERNTIMEFIXMULTIPLAYER_VERBOSE_LOGGING=true
```

Check logs:
```bash
docker exec stardew cat /config/xdg/config/StardewValley/ErrorLogs/SMAPI-latest.txt | grep SkullCavern
```

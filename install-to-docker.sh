#!/bin/bash
# Installation script for cavazos-apps/stardew-multiplayer-docker setup

set -e

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}Skull Cavern Time Fix - Docker Integration Installer${NC}"
echo ""

# Check if we're in the right directory
if [ ! -f "manifest.json" ] || [ ! -f "ModEntry.cs" ]; then
    echo -e "${RED}Error: Please run this script from the stardew-skull-cavern-multiplayer directory${NC}"
    exit 1
fi

# Get Docker repo path
if [ -z "$1" ]; then
    echo -e "${YELLOW}Usage: $0 /path/to/stardew-multiplayer-docker${NC}"
    echo ""
    echo "Example:"
    echo "  $0 ../stardew-multiplayer-docker"
    exit 1
fi

DOCKER_REPO="$1"

# Validate Docker repo
if [ ! -d "$DOCKER_REPO/docker/mods" ]; then
    echo -e "${RED}Error: $DOCKER_REPO doesn't appear to be a stardew-multiplayer-docker repository${NC}"
    echo "Expected to find: $DOCKER_REPO/docker/mods/"
    exit 1
fi

echo -e "${YELLOW}Step 1: Building mod...${NC}"
dotnet build -c Release

if [ $? -ne 0 ]; then
    echo -e "${RED}Build failed!${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Build successful${NC}"
echo ""

MOD_DIR="$DOCKER_REPO/docker/mods/SkullCavernTimeFixMultiplayer"

echo -e "${YELLOW}Step 2: Creating mod directory...${NC}"
mkdir -p "$MOD_DIR"
echo -e "${GREEN}✓ Created $MOD_DIR${NC}"
echo ""

echo -e "${YELLOW}Step 3: Copying mod files...${NC}"

# Copy DLL
cp bin/Release/net6.0/SkullCavernTimeFixMultiplayer.dll "$MOD_DIR/"
echo -e "${GREEN}✓ Copied SkullCavernTimeFixMultiplayer.dll${NC}"

# Copy manifest
cp manifest.json "$MOD_DIR/"
echo -e "${GREEN}✓ Copied manifest.json${NC}"

# Create config template
cat > "$MOD_DIR/config.json.template" << 'EOF'
{
  "Enabled": ${SKULLCAVERNTIMEFIXMULTIPLAYER_ENABLED-true},
  "SlowdownMultiplier": ${SKULLCAVERNTIMEFIXMULTIPLAYER_SLOWDOWN_MULTIPLIER-2.0},
  "MinimumPlayerPercentage": ${SKULLCAVERNTIMEFIXMULTIPLAYER_MINIMUM_PLAYER_PERCENTAGE-0.5},
  "VerboseLogging": ${SKULLCAVERNTIMEFIXMULTIPLAYER_VERBOSE_LOGGING-false}
}
EOF
echo -e "${GREEN}✓ Created config.json.template${NC}"
echo ""

echo -e "${YELLOW}Step 4: Checking docker-compose.yml...${NC}"

COMPOSE_FILE="$DOCKER_REPO/docker-compose-steam.yml"
if [ ! -f "$COMPOSE_FILE" ]; then
    COMPOSE_FILE="$DOCKER_REPO/docker-compose-gog.yml"
fi

if grep -q "SKULLCAVERNTIMEFIXMULTIPLAYER" "$COMPOSE_FILE"; then
    echo -e "${GREEN}✓ Environment variables already present in docker-compose${NC}"
else
    echo -e "${YELLOW}⚠ Environment variables NOT found in docker-compose${NC}"
    echo ""
    echo "Please add these lines to the 'environment:' section of $COMPOSE_FILE:"
    echo ""
    echo "  # Skull Cavern Time Fix Multiplayer mod"
    echo "  - ENABLE_SKULLCAVERNTIMEFIXMULTIPLAYER_MOD=\${ENABLE_SKULLCAVERNTIMEFIXMULTIPLAYER_MOD-true}"
    echo "  - SKULLCAVERNTIMEFIXMULTIPLAYER_ENABLED=\${SKULLCAVERNTIMEFIXMULTIPLAYER_ENABLED-true}"
    echo "  - SKULLCAVERNTIMEFIXMULTIPLAYER_SLOWDOWN_MULTIPLIER=\${SKULLCAVERNTIMEFIXMULTIPLAYER_SLOWDOWN_MULTIPLIER-2.0}"
    echo "  - SKULLCAVERNTIMEFIXMULTIPLAYER_MINIMUM_PLAYER_PERCENTAGE=\${SKULLCAVERNTIMEFIXMULTIPLAYER_MINIMUM_PLAYER_PERCENTAGE-0.5}"
    echo "  - SKULLCAVERNTIMEFIXMULTIPLAYER_VERBOSE_LOGGING=\${SKULLCAVERNTIMEFIXMULTIPLAYER_VERBOSE_LOGGING-false}"
    echo ""
fi

echo ""
echo -e "${GREEN}✓ Installation complete!${NC}"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo "1. Add environment variables to docker-compose (see above if not already added)"
echo "2. Rebuild the Docker container:"
echo "   ${GREEN}cd $DOCKER_REPO${NC}"
echo "   ${GREEN}docker compose -f docker-compose-steam.yml build --no-cache${NC}"
echo "3. Start the container:"
echo "   ${GREEN}docker compose -f docker-compose-steam.yml up -d${NC}"
echo "4. Verify the mod loaded:"
echo "   ${GREEN}docker logs stardew | grep SkullCavern${NC}"
echo ""
echo -e "${GREEN}Enjoy your fixed Skull Cavern time slowdown! 🎉${NC}"

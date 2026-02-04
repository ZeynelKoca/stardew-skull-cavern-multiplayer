# Changelog

All notable changes to this project will be documented in this file.

## [1.0.0] - 2026-02-04

### Added
- Initial release of Skull Cavern Time Fix for Multiplayer
- Uses Pathoschild.Stardew.ModBuildConfig 4.4.0
- Ignores headless host player when calculating Skull Cavern time slowdown
- Proportional slowdown based on percentage of remote players in Skull Cavern
- Configurable slowdown multiplier and minimum player percentage
- Optional verbose logging for debugging
- Harmony-based patching for clean integration with game code
- Support for Stardew Valley 1.6+ and SMAPI 4.0.0+

### Features
- **Smart Host Detection**: Automatically ignores the host player (bot) from time calculations
- **Proportional Time Slowdown**: Time slows proportionally to how many remote players are in Skull Cavern
- **Configurable**: Adjustable slowdown multiplier and player percentage threshold
- **Performance Optimized**: Uses Harmony patches for minimal performance impact
- **Debug Support**: Optional verbose logging to troubleshoot issues

### Technical Details
- Patches `Game1.performTenMinuteClockUpdate` method
- Only runs on the host/server
- Requires multiplayer mode with remote players
- Default 50% slowdown (matching vanilla) when all remote players are in Skull Cavern

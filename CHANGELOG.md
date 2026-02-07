# Changelog

## [1.0.0] - 2026-02-07

### Added
- Initial release
- Proportional time slowdown in Skull Cavern for multiplayer servers with host bots
- Uses vanilla formula: `2000ms × (real players in SC / total real players)`
- Completely excludes host bot from all calculations
- Config options: `Enabled`, `VerboseLogging`
- Supports Stardew Valley 1.6+ and SMAPI 4.0.0+

# FullingPorter

A Valheim logistics mod that adds a friendly Fuling porter hired with a contract from Haldor.

## Goals
- Buy a Fuling Porter Contract from Haldor.
- Spawn a persistent, renameable, non-aggressive porter at your base.
- Mark source containers for the porter.
- Route items into organized storage using QuickStackPlus Smart Storage filters.
- Multiplayer-safe, server-authoritative item transfers.

## Status
Early development (0.1.0). The initial milestone establishes the plugin, configuration, QuickStackPlus Smart Storage bridge, and porter state/AI scaffolding.

## Requirements
- Valheim 1.0.x
- BepInExPack Valheim 5.4.2350+
- Jotunn 2.30.2+
- QuickStackPlus 1.2.0+
- ConditionalConfigSync 1.0.5+ (QuickStackPlus dependency)

## QuickStackPlus integration
QuickStackPlus stores Smart Storage selections in the container ZDO under:
`Goneryx.QuickStackPlus.StorageFilter`

Values are item IDs separated by U+001F. FullingPorter reads this existing data directly and does not maintain a competing storage-filter system.

## Development
Set `VALHEIM_MANAGED` to Valheim's `valheim_Data/Managed` directory and `BEPINEX_CORE` to `BepInEx/core`, then build the project.

See [docs/DESIGN.md](docs/DESIGN.md) for architecture and milestones.

## Testing
The first in-game test should only begin after the build gate in [docs/PLAYTEST.md](docs/PLAYTEST.md) passes. Use a disposable world for the initial smoke test.

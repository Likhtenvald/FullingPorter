# FullingPorter

A Valheim logistics mod that adds a friendly Fuling porter hired from Haldor. The porter moves items from player-selected source containers into QuickStackPlus Smart Storage without introducing a second destination-filter system.

## Current behavior

- Buy a **Fuling Porter Contract** from Haldor for **1500 coins**.
- Use the contract to request a server-authoritative porter spawn.
- Exactly **one porter per world** is allowed.
- The porter is a normal friendly Fuling and is always immortal.
- Default work radius: **30 m** from the porter's persistent home position.
- Default carrying capacity: **10 distinct stacks per trip**.
- The nearest usable source is selected first.
- Cargo for the same destination is grouped before moving to another destination.
- Full Smart Storage targets are temporarily cooled down instead of being retried continuously.
- Item transfers are server-authoritative and remove from source only after destination insertion succeeds.
- The porter returns to its persistent home after a trip.

## Controls

All keys are configurable in the BepInEx config.

| Action | Default |
| --- | --- |
| Toggle hovered container as porter source | `Home` |
| Talk/react with hovered porter | `E` |
| Rename hovered porter | `End` |
| Dismiss hovered porter | `Delete` twice within 3 seconds |

The porter hover UI shows only its name and current activity status. Pressing `E` makes the porter react with contextual banter and a vanilla Fuling vocalization; it does not open a menu or change logistics. Dialogue has a short anti-spam cooldown and varies for idle, working, returning and blocked/full-storage states.

## QuickStackPlus integration

QuickStackPlus Smart Storage remains the destination source of truth.

QuickStackPlus 1.2.0 stores selected item prefab IDs in the container ZDO key:

`Goneryx.QuickStackPlus.StorageFilter`

Values are separated by U+001F. FullingPorter reads that existing data and does not maintain its own storage-filter configuration.

A container marked as a porter source is not considered a destination during that scan.

## Networking and persistence

Porter work and inventory transfers execute on the server. Player actions that mutate world state — spawning, source toggling, renaming and dismissal — are sent through a Jötunn RPC to the server.

The one-porter invariant is persisted with a world global key containing the porter's ZDOID. This lets the server distinguish a real unloaded porter from a stale key left by a deleted object.

The porter's name, home position, activity status and dialogue context are stored in its ZDO. Clients display the server's current activity and remaining stack count. Source markers are stored on the source container ZDO.

The server checks that every joining client has FullingPorter **0.1.1**, and Jötunn rejects missing or different major, minor or patch versions. The same requirement applies when a client with FullingPorter joins a server without it. Version equality compares declared mod versions, so bump the version on every changed build sent to another player.

## Configuration defaults

- `Contract.Price = 1500`
- `Porter.WorkRadius = 30`
- `Porter.MaxStacksPerTrip = 10`
- `Porter.SourceChestKey = Home`
- `Porter.RenameKey = End`
- `Porter.DismissKey = Delete`

The server synchronizes price, work radius and trip capacity to clients. Input keys remain local. Client values return after disconnecting.

Older development configs may still contain obsolete entries such as `CanDie`; they are ignored by current code. Existing config values are not overwritten when defaults change.

## Requirements

- Current Valheim build used by the tester
- BepInExPack Valheim 5.4.2350+
- Jötunn 2.30.2+
- QuickStackPlus 1.2.0+
- ConditionalConfigSync 1.0.5+ through QuickStackPlus

## Development

The safest local workflow is:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\dev.ps1
```

This builds against the installed Valheim/Jötunn assemblies and installs the DLL into the configured Thunderstore profile.

See:
- [docs/DESIGN.md](docs/DESIGN.md) for architecture.
- [docs/PLAYTEST.md](docs/PLAYTEST.md) for the current test matrix.
- [docs/BUILD.md](docs/BUILD.md) for local build details.

Host and remote-client smoke tests passed on two PCs on 2026-09-23, covering cargo transfers, server config and activity status. Dedicated-server, mismatched-version and long-term save migration tests remain open. Use a disposable test world until those gates pass.

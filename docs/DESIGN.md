# FullingPorter design

## Core rule
FullingPorter does not invent a second storage classification system. QuickStackPlus Smart Storage remains the source of truth for destination filters.

QuickStackPlus 1.2.0 stores selected item prefab IDs in a container ZDO key named `Goneryx.QuickStackPlus.StorageFilter`, separated by U+001F.

## Planned flow
1. Player buys a contract from Haldor.
2. Using the contract spawns one persistent porter.
3. Player explicitly marks one or more source containers.
4. Server-authoritative porter reserves a source stack.
5. Porter walks to source, takes up to configured capacity.
6. Destination resolver prefers QuickStackPlus Smart Storage containers that accept the item.
7. Porter walks to destination and transfers items atomically.
8. If no destination is available/full, cargo is returned to source or retained safely; it is never deleted.

## Multiplayer invariants
- ZDO persists porter name, owner and assigned sources.
- Only the ZNetView owner mutates porter state.
- Container transfers are revalidated immediately before mutation.
- No item is removed until destination capacity/reservation has been confirmed.
- Ownership changes must not duplicate an in-flight transfer.

## Milestones
### 0.1.0 foundation
Plugin/config, dependency declarations, prefab/state scaffolding, localization, QuickStackPlus filter bridge.

### 0.2.0 playable logistics
Contract use/spawn, Haldor trade, source marking, pathing state machine, atomic transfer.

### 0.3.0 UX/multiplayer hardening
Rename dialog, status UI, permissions, recovery, dedicated-server tests.

### 1.0.0
Stable release, packaging, compatibility matrix and migration handling.

# FullingPorter design

## Core rule

FullingPorter does not invent a second storage-classification system. QuickStackPlus Smart Storage remains the source of truth for destinations.

QuickStackPlus 1.2.0 stores selected item prefab IDs under the container ZDO key `Goneryx.QuickStackPlus.StorageFilter`, separated by U+001F.

## Work cycle

1. The server scans containers within the configured radius around the porter's persistent home.
2. Explicitly marked source containers are ordered by distance from the porter's current position.
3. The nearest source with transferable cargo is selected.
4. Up to `MaxStacksPerTrip` distinct stacks are planned; default capacity is 10.
5. The porter walks to the source and revalidates every planned stack.
6. Smart Storage destinations are resolved from QuickStackPlus filters.
7. During delivery, one nearest destination is selected and all cargo assigned to that destination is processed before choosing another.
8. Inventory/ZDO updates are rate-limited.
9. The porter returns home after the trip.

No source item is removed unless destination insertion succeeds.

## Destination failures and cooldowns

A transfer result distinguishes:
- success;
- waiting for container ownership;
- source item no longer available;
- destination no longer accepts the item;
- destination full;
- other failure.

Ownership waits have a 3-second timeout. Only a genuinely full destination starts the destination/item cooldown. Cooldown entries expire after 8 seconds and stale container/item entries are cleaned before planning a new batch.

## Porter identity and one-per-world invariant

Only one porter may exist in a world.

Current builds persist a global key in the form:

`FullingPorter.ActivePorter:<ZDO user id>:<ZDO object id>`

When checking whether the slot is occupied, the server resolves the stored ZDOID with `ZDOMan.GetZDO`. An existing unloaded porter therefore still blocks another contract, while a key whose ZDO no longer exists is removed as stale.

The old development boolean key `FullingPorter.ActivePorter` is migrated when a porter is loaded; if no loaded porter exists, it is treated as stale.

## Network authority

World mutations are server-owned.

Jötunn CustomRPC is registered during plugin startup and handles:
- contract spawn requests;
- source container toggle requests;
- porter rename requests;
- porter dismissal requests.

The contract is consumed on the requesting client only after the server confirms a successful spawn.

The worker and `TransferService` execute only on the server. Source and destination ZNetViews are claimed before mutation, and transfers are revalidated immediately before inventory changes. A planned stack records its original slot and item details; after ownership changes, the worker resolves the live inventory entry again before removing it. Container use flags and a transfer lock prevent simultaneous player and porter writes.

The server publishes localized status tokens with remaining-stack counts and the dialogue context to the porter ZDO. Clients read those values without running worker AI. Jötunn requires FullingPorter on server and every client, and checks all three version components at connection time. `Contract.Price`, `Porter.WorkRadius` and `Porter.MaxStacksPerTrip` are server-synchronized; key bindings stay local.

## Persistence

Porter ZDO:
- custom name;
- persistent home position;
- current activity status and dialogue context.

Container ZDO:
- porter-source marker;
- QuickStackPlus Smart Storage filter remains owned by QuickStackPlus.

World global keys:
- active porter ZDO identity.

## UX defaults

- Work radius: 30 m.
- Capacity: 10 stacks.
- Source toggle: `Home`.
- Rename: `End`.
- Dismiss: `Delete` twice within 3 seconds.
- Porter is permanently immune to `Character.Damage`.
- Hover text contains porter name plus current activity status.
- EnemyHud is suppressed for the porter so its name does not remain permanently visible.

## Contract

Haldor sells one injected Fuling Porter Contract trade at the configured price, default 1500 coins. The current development item uses a Fuling-themed vanilla item template; a bespoke contract asset can replace it later without changing contract behavior.

## Remaining release gates

- Compile against the tester's current Valheim/Jötunn assemblies after every API-sensitive change.
- Singleplayer and host/client smoke tests were reported as passing on two PCs on 2026-09-23; repeat after network-related changes.
- Host/client RPC race tests, especially simultaneous contract use.
- Confirm missing and mismatched FullingPorter versions are rejected before multiplayer gameplay.
- Reconnect and world restart persistence tests.
- Dedicated-server test.
- Final contract art/icon if desired.
- Package/release metadata review.


## RPC security boundary

Remote porter actions are server-authoritative.

- FullingPorter's routed RPC sender is authenticated against the actual inbound ZRpc peer before Jötunn dispatches the package.
- Remote spawn ignores the client-supplied world position and uses the server-observed peer position.
- Source assignment, rename, and dismissal require the requesting peer to be within 5 metres of the target.
- Remote actions are rate-limited server-side.
- Rename payloads are capped at the same 24-character limit as the client UI.
- The server still cannot independently inspect a remote Valheim character's local inventory, so possession of the contract remains a client-side trust boundary. The server still enforces the one-porter-per-world rule and authoritative spawn position.

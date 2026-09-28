# Persistent porter work area on a listen server

## Behavior

The porter continues its normal work cycle while the listen-server host is away from its home, even when every player has left the base. The host's ordinary active area serves the porter while the host is near it. Inventory changes remain server-authoritative.

## Design

The server reads the existing world-presence ZDO to find the porter's persistent home. When the host is outside the work area, a server-side scene extension keeps the home and configured work radius active. It refreshes terrain zones and adds the relevant ZDOs to Valheim's normal scene creation/removal lists. The existing PorterWorker and TransferService then run unchanged on the server. The extension withdraws when the host returns or the porter is dismissed; an interrupted batch releases its container locks.

The host-distance rule is isolated from Valheim APIs for unit tests. Scene and zone integration is compiled against the local Valheim assemblies. A multiplayer playtest must check an empty base while everyone is away, host departure and return, and exact source plus destination item totals, including a player-open chest during delivery.

## Limits

The server now loads and simulates this extra area for as long as the porter exists and the host is away. This costs host CPU and memory even with no player at the base and may keep other nearby objects active. A passing unit test or build cannot replace the host/client inventory regression test.

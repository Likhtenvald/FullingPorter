# Changelog

## 0.1.5 - playtest candidate

- Keep the porter work area active on the server while the host is away, including when no player remains at the base.
- Release server-held chest transfer locks if the porter worker unloads during a trip.
- Add area-activation tests and an empty-base multiplayer playtest checklist.
- In-game host/client transfer integrity and server performance are pending validation.

## 0.1.4 - beta

- Add the missing xUnit VSTest adapter so `dotnet test` discovers and runs the regression suite.
- Extract porter chest wait timers into a game-independent state tracker and add regression tests for player access, lock retries and subsequent trips.
- Known beta limitation: the porter can stop working when the host is far from the base; this release does not fix that defect.
- Valheim testing of 0.1.4 is pending after publication; earlier playtest results apply only to the builds recorded below and in the playtest documentation.

## 0.1.3 - beta

- Recover from orphaned porter chest locks and release locks while waiting for container access.
- End a porter lock wait after five seconds, or a player-held chest wait after thirty seconds, and record chest states for diagnosis.
- Reset wait timers after each trip so an interrupted delivery does not abort later trips.

## 0.1.2 - beta

- Synchronized manual and ambient porter dialogue and voice between nearby players.
- Validated speech requests on the server and applied a shared cooldown.

## 0.1.1 - beta

- Added the Fuling porter contract, source chest marking and QuickStackPlus Smart Storage routing.
- Added server-controlled price, work radius and trip capacity, plus synchronized porter activity and name.
- Protected item transfers against chest access during delivery and revalidated stacks after inventory ownership changes.
- Required matching FullingPorter versions on hosts and clients.

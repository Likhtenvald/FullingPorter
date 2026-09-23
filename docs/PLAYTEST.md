# Playtest Candidate Checklist

Use a disposable world for development tests.

## Build gate

- [ ] Build against the exact Valheim installation used for testing.
- [ ] Build against the installed BepInEx and Jötunn assemblies.
- [ ] No Harmony patch resolution errors at startup.
- [ ] Jötunn registers the FullingPorter custom RPC.
- [ ] QuickStackPlus is loaded and its Smart Storage data is readable.
- [ ] No compile-time API assumptions are taken from older Valheim builds.

## Singleplayer smoke test

1. Start a disposable world with FullingPorter and required dependencies.
2. Confirm there are no FullingPorter exceptions in `LogOutput.log`.
3. Visit Haldor.
4. Confirm exactly one Fuling Porter Contract trade is visible.
5. Confirm the configured price is used. Current default: 1500 coins.
6. Buy and use the contract.
7. Confirm exactly one friendly Fuling porter spawns.
8. Confirm exactly one contract is consumed after successful spawn.
9. Try another contract while the porter exists; confirm no second porter spawns.
10. Reload the world with the porter outside the currently loaded zone; confirm another contract is still blocked.
11. Dismiss the porter with `Delete` twice within 3 seconds.
12. Confirm a new contract can spawn a porter after dismissal.

## Hover and controls

- No permanent EnemyHud name is visible above the porter.
- Looking at the porter shows only its name and activity status.
- `E` while hovering the porter produces a localized contextual line and a Fuling vocalization.
- Repeated `E` presses are rate-limited; no sound/message spam occurs.
- Idle, working, returning and recently blocked/full-storage states can produce different dialogue pools.
- `E` never opens rename input.
- `End` while hovering the porter opens rename input.
- Rename persists after reload.
- `Home` while hovering a container toggles the porter-source marker.
- Source marker text is localized.
- `Delete` requires two presses within 3 seconds.

## Routing test

Prepare at least three source containers and multiple Smart Storage destinations.

- The nearest source with transferable cargo is selected first.
- A source with no routable cargo is skipped.
- Maximum planned cargo respects `MaxStacksPerTrip` (default 10).
- If multiple stacks target one destination, the porter processes that destination as a group before moving to another.
- With multiple destinations, the next destination is selected by distance from the porter's current position.
- The porter returns to its persistent home after finishing the trip.

## Transfer integrity

For each case compare exact item counts before and after.

- One stack, one destination, one free slot: exactly one source -> destination pass.
- Ten distinct stacks in one source.
- Multiple item types and multiple Smart Storage destinations.
- Destination becomes full after planning.
- Item is removed manually from source while porter is walking.
- QuickStackPlus filter changes while porter is walking.
- Ownership changes while porter is at destination.
- No item duplication.
- No item loss.

Expected ownership behavior:
- porter waits at destination instead of starting a second trip;
- ownership wait times out after 3 seconds;
- only a genuinely full destination starts the 8-second destination cooldown.

## Cooldown behavior

- Fill a matching Smart Storage destination.
- Confirm the porter does not continuously retry it.
- Wait beyond 8 seconds and free a slot.
- Confirm the destination becomes eligible again.
- Destroy/remove a previously cooled-down container and confirm later scans continue normally.

## Persistence

- Rename porter, reload world, verify name.
- Move porter through a normal trip, reload world, verify persistent home.
- Source markers persist.
- One-porter global identity survives world restart.
- Dismissed/deleted porter does not leave a permanent stale world slot.

## Multiplayer gate

Run host + one client with identical mod versions.

1. Host buys/uses contract.
2. Client sees the same porter, name, position and movement.
3. Client attempts another contract; server refuses it.
4. Host and client attempt contracts at nearly the same time; exactly one porter is created.
5. Host toggles a source; client sees the marker.
6. Client toggles a source; server applies it and host sees the marker.
7. Host renames porter; client receives the name.
8. Client renames porter; host receives the name.
9. Host dismisses porter.
10. Repeat with client dismissal.
11. Transfer one stack and then a 10-stack batch; verify no duplicate/loss.
12. Disconnect/reconnect client during normal porter activity.
13. Restart world/server and verify one-porter persistence.
14. Restart while porter is outside the loaded zone; a second contract must still be blocked.

## Dedicated-server gate

- Server has FullingPorter, Jötunn and QuickStackPlus installed.
- Clients use matching versions.
- Contract spawn request reaches server.
- Source/rename/dismiss requests reach server.
- Item movement occurs only once on the server.
- World restart preserves porter identity and home.

Do not use a valuable production world until these gates pass.

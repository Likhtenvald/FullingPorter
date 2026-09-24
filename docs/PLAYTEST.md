# Playtest Candidate Checklist

Use a disposable world for development tests.

Host and remote-client tests on two PCs were reported passing on 2026-09-23 after the transfer, config and activity-status fixes. A local host tested 0.1.3 on 2026-09-24: four complete trips, ten moved stacks, and delivery resumed after source and destination chests were closed. Repeat the chest-access regression with a remote client; dedicated-server compatibility has not yet been confirmed.

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

Run host + one client with identical FullingPorter 0.1.3 builds. Check cargo, synced config and activity on both PCs after a version upgrade.

1. Host buys/uses contract.
2. Client sees the same porter, name, position and movement.
3. On the client, hover over the porter while it collects, delivers, and returns; confirm each status and the remaining-stack counter match the host. Wait until it is idle and confirm the status changes back.
4. On the client, interact with the porter while it is working and returning; confirm the dialogue uses the corresponding activity context.
5. Client attempts another contract; server refuses it.
6. Host and client attempt contracts at nearly the same time; exactly one porter is created.
7. Host toggles a source; client sees the marker.
8. Client toggles a source; server applies it and host sees the marker.
9. Host renames porter; client receives the name.
10. Client renames porter; host receives the name.
11. Host dismisses porter.
12. Repeat with client dismissal.
13. Transfer one stack and then a 10-stack batch; verify no duplicate/loss.
14. Disconnect/reconnect client during normal porter activity.
15. Restart world/server and verify one-porter persistence.
16. Restart while porter is outside the loaded zone; a second contract must still be blocked.

## Multiplayer regression: synchronized porter dialogue (0.1.2)

1. Install 0.1.3 on the host and remote client, stand both players near the porter, and press E on the host. Confirm both see the same line and hear the same voice variant exactly once.
2. Press E on the remote client and confirm both see the same line and hear the same voice variant exactly once. Repeat while the porter is working or returning.
3. Leave both players near the porter until it speaks on its own. Confirm each ambient line and voice matches on both PCs with no duplicate bubbles or overlapping spam.
4. Move the host more than 20 m away while the client stays nearby. Confirm the client's manual dialogue still works and the host does not see a distant speech bubble.
5. Press E quickly on both PCs; confirm the shared server cooldown prevents simultaneous overlapping lines.
6. Move the client out of interaction range and confirm a modified client cannot trigger manual dialogue remotely. Reconnect and repeat to verify no stale bubble is replayed.

Expected result: the server chooses a single line and voice per speech event; all nearby players observe it once. Clients cannot force a distant speech event.

## Dedicated-server gate

- Server has FullingPorter, Jötunn and QuickStackPlus installed.
- Clients use matching versions.
- Contract spawn request reaches server.
- Source/rename/dismiss requests reach server.
- Item movement occurs only once on the server.
- World restart preserves porter identity and home.

Do not use a valuable production world until these gates pass.


## Regression: multiple item types to one Smart Storage

1. Configure one QuickStackPlus Smart Storage chest to accept at least three different item types.
2. Leave enough free capacity in that destination for every test stack.
3. Put one stack of each accepted type into a single porter source chest.
4. Let the porter complete one trip without touching either chest.
5. Verify all planned stacks are delivered during that same visit to the destination.
6. Verify the porter does not return to the source for an item type that already had reserved capacity in the first trip.
7. Repeat with the destination nearly full and verify the porter does not over-plan capacity or lose/duplicate items.

Expected result: one batch may contain several different item types for the same destination, and planned destination capacity is respected across the whole batch.


## Multiplayer regression: RPC hardening

Run these checks with one host/server and one remote client.

1. From the remote client, use a valid contract normally and verify the porter spawns beside that client's server-observed position.
2. Stand more than 5 metres from a source chest and verify a remote source-toggle request is rejected; move next to it and verify the same action succeeds.
3. Stand more than 5 metres from the porter and verify remote rename/dismiss requests are rejected; move next to the porter and verify they succeed.
4. Rename the porter with exactly 24 characters and verify success. Attempt more than 24 characters through a modified/test client and verify the server rejects it.
5. Send porter actions faster than the server action interval with a modified/test client and verify excess requests are rejected without changing world state.
6. With a modified/test client, forge the routed sender UID to another connected peer and verify the FullingPorter routed RPC is dropped before its action handler runs.
7. After rejected requests, verify source markers, porter name, world-presence state, and inventories remain unchanged.

Expected result: every remote action is attributed to the real inbound peer, proximity is enforced server-side, and rejected RPCs have no world-state side effects.


## Multiplayer regression: open chest during porter transfer

This is the multiplayer anti-duplication regression gate; 0.1.3 has passed the local-host variant, and the remote-client variant remains to be checked.

1. Host a world on one PC and connect a second PC as a remote client.
2. Put several identifiable stacks into a marked source chest and configure a Smart Storage destination with enough free space.
3. While the porter is walking toward the destination, open the source chest on the remote client and keep it open through the expected delivery moment.
4. Verify the porter waits and does not mutate either inventory while the source chest is open.
5. Close the source chest and verify the porter resumes, delivering each stack exactly once.
6. Repeat while holding the destination chest open instead of the source chest.
7. Repeatedly try to open either chest during the short transfer moment and verify the porter transfer lock blocks opening until the transaction finishes.
8. After every variant, count source + destination totals and verify no item was duplicated or lost.
9. Repeat with several different item types routed into the same Smart Storage chest.
10. Hold the source chest open for 10-15 seconds during delivery, then close it. Confirm the porter transfers the pending stack, completes the trip and returns home. Repeat while holding the destination chest open.
11. Immediately start another trip with new stacks. Confirm that the previous wait does not cause an immediate timeout or prevent delivery.
12. Hold either chest open for over 30 seconds while the porter is delivering. Confirm it aborts that trip, releases its locks and returns home; after closing the chest, confirm a later trip can proceed.

Expected result: an open source or destination chest pauses the porter without changing item counts. The 30-second player wait and the 5-second porter-lock wait are independent, and both timers reset between trips. A chest cannot be newly opened during the porter's transfer lock. Total item count remains constant. The host log records source and destination ZDO IDs and lock states when waiting for porter-lock access; a player-held chest timeout logs the item and return home.


## Multiplayer regression: server config synchronization

1. On the host/server, set Contract.Price to 1777, Porter.WorkRadius to 40, and Porter.MaxStacksPerTrip to 4.
2. On the remote client, set Contract.Price to 999, Porter.WorkRadius to 15, and Porter.MaxStacksPerTrip to 2 before joining.
3. Start the world on the host and join from the remote client.
4. Open Haldor's store on both machines; verify the contract price is 1777 on both.
5. Verify the client's effective WorkRadius is 40 and MaxStacksPerTrip is 4 while connected.
6. Leave DismissKey, RenameKey, and SourceChestKey different on the client and verify they retain their local bindings.
7. Disconnect the client and verify its local price, radius, and trip capacity are restored outside the server session.

Expected result: contract price, work radius, and trip capacity follow the server while connected; input bindings remain local.


## Multiplayer regression: FullingPorter version gate

1. Install 0.1.3 on both host and remote client; verify connection succeeds and normal porter actions work.
2. Remove FullingPorter from the remote client while leaving Jötunn installed; verify connection is refused with a mod compatibility error.
3. Restore FullingPorter on the client and remove it from the host; verify connection is refused.
4. Put 0.1.2 on the client and 0.1.3 on the host; verify connection is refused. Reverse the versions and repeat.
5. Restore 0.1.3 on both sides; verify connection works again and the contract, config and activity status still synchronize.
6. Repeat the missing-mod and mismatched-version cases on a dedicated server before release.

Expected result: both sides need FullingPorter, with matching major, minor and patch versions. Ensure each newly built DLL has an incremented declared version; the check cannot distinguish two different binaries labeled with the same version.

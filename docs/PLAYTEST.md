# Playtest Candidate Checklist

This document defines the gate before asking for an in-game test.

## Build gate
- [ ] Compile against the tester's current Valheim `assembly_valheim.dll`.
- [ ] Compile against installed BepInEx and Jotunn assemblies.
- [ ] No Harmony patch resolution errors at startup.
- [ ] QuickStackPlus dependency GUID/version confirmed from the installed plugin.

## Smoke test
1. Start a disposable local world with only required dependencies and FullingPorter.
2. Confirm there are no FullingPorter exceptions in `LogOutput.log`.
3. Visit Haldor and confirm exactly one Fuling Porter Contract trade is visible.
4. Buy and use the contract. Confirm exactly one friendly porter spawns and the contract is consumed once.
5. Rename the porter and reload the world. Confirm the name persists.
6. Alt-interact with a chest to mark it as a source.
7. Configure a different chest as QuickStackPlus Smart Storage for one item type.
8. Put that item in the source chest and confirm the porter walks source -> destination -> home.
9. Confirm the source count decreases exactly by the destination count increase.
10. Fill the destination and confirm no items are lost.
11. Reload the world and repeat one transfer.

## Multiplayer gate
- Host plus one client, both with identical mod versions.
- Client can mark/unmark a source.
- Only one transfer occurs for one job.
- Disconnect/reconnect does not duplicate or delete cargo.

Do not use a valuable production world for the first test.

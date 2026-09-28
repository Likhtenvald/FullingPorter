# Working with local Codex

Use the Windows checkout at `C:\Users\Micrososochek\Documents\GitHub\FullingPorter`. ChatGPT Work is for design discussions and reviewing a Codex result through GitHub. Local Codex handles code, builds and package preparation because it can access the installed Valheim assemblies. The user tests in Valheim and publishes the ZIP under the Micrososochek Thunderstore team.

Codex reads the root `AGENTS.md` when launched from this repository. Start a fresh Codex session after updating `AGENTS.md`.

## Start a development task

In PowerShell:

```powershell
cd "C:\Users\Micrososochek\Documents\GitHub\FullingPorter"
git switch develop
git pull --ff-only origin develop
codex
```

Give Codex the specific bug, desired behavior and relevant host/client logs. Example task:

> Work in this FullingPorter checkout on `develop`. Investigate [symptom and reproduction], implement the smallest safe fix, run the unit tests and a local game-assembly build with `scripts/dev.ps1 -NoInstall`. Keep transfers server-authoritative. Tell me what changed, how to test it in Valheim, the commit or working-tree status, and any remaining risk. Do not publish, push or merge without my explicit instruction.

When Codex has built a DLL, close the game and have it install through `scripts/dev.ps1`, or run that command yourself. For a two-PC test, both machines need the same declared FullingPorter version and the same new build.

## Prepare a release candidate

First test the changed build in Valheim. Then ask local Codex:

> My Valheim test of FullingPorter [version/commit] passed. Prepare the next beta release candidate in this local checkout: align plugin/project/manifest versions; update changelog, package README and relevant docs; run `dotnet test` and a fresh `scripts/dev.ps1 -NoInstall` build; run `scripts/package-thunderstore.ps1`; inspect ZIP integrity, root contents and version alignment. Report the absolute ZIP path, SHA-256, test/build results and git status. Do not upload to Thunderstore, create a GitHub release, push or merge unless I explicitly ask.

Codex's finished report and any pushed PR can be reviewed here. The user imports/tests the archive if needed and uploads it manually to Thunderstore. A GitHub commit does not update a Thunderstore package.

## Current testing boundary

A local host test of 0.1.3 covered ordinary delivery and player-held chests. The remote-client anti-duplication regression and the reported stop when the host leaves the base need separate tests. Do not describe them as verified until their results are recorded.

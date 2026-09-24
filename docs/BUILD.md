# Local build

Build against the same Valheim installation and mod profile that will run the test. Valheim API signatures change between game builds, so a successful build against unrelated reference DLLs is not a valid gate.

## Recommended development command

From the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\dev.ps1
```

The script:
- detects the local Valheim installation and Thunderstore profile;
- builds against the installed Valheim/BepInEx/Jötunn assemblies;
- writes `artifacts\FullingPorter.dll`;
- installs the DLL into the FullingPorter folder in the active Thunderstore profile.

Valheim must be fully closed before replacing the DLL. If `Copy-Item` reports an open mapped section / locked DLL, close the game and rerun the command.

## Build without install

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\dev.ps1 -NoInstall
```

Or use the lower-level build script directly:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-local.ps1
```

## Non-default Valheim path

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-local.ps1 -ValheimDir "D:\SteamLibrary\steamapps\common\Valheim"
```

## Current config defaults

A fresh config should contain:

- `Contract.Price = 1500`
- `Porter.WorkRadius = 30`
- `Porter.MaxStacksPerTrip = 10`
- `Porter.SourceChestKey = Home`
- `Porter.RenameKey = End`
- `Porter.DismissKey = Delete`

BepInEx preserves existing values. If a development profile previously used price 10 or capacity 12, changing the source defaults does not overwrite that file. Update the existing config manually or delete the test config and let BepInEx regenerate it.

An obsolete `CanDie` entry from older development builds is ignored; the porter is now always immortal.

## Versioned multiplayer builds

The server and every client must install FullingPorter 0.1.3 for this build. Jötunn rejects a missing mod or a different major, minor or patch version when connecting. Keep `Plugin.PluginVersion`, `FullingPorter.csproj` `<Version>` and `manifest.json` `version_number` identical; CI checks this. Increase all three before distributing any changed DLL, including a hotfix, because version checks cannot distinguish different binaries labeled with the same version.

## Thunderstore and r2modman beta package

After a successful game build and playtest on the current code, run:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\dev.ps1 -NoInstall
powershell -ExecutionPolicy Bypass -File .\scripts\package-thunderstore.ps1
```

The second command takes `artifacts\FullingPorter.dll`, checks that its assembly version matches the plugin, project and manifest, verifies the 256×256 icon, and creates `dist\FullingPorter-0.1.3-thunderstore.zip`. The archive contains only `manifest.json`, `icon.png`, `README.md`, `CHANGELOG.md` and `FullingPorter.dll` at its root. Dependencies are declared in the manifest and are not bundled. The script refuses to overwrite an archive with the same version.

Before upload, import the ZIP as a local mod in r2modman or Thunderstore Mod Manager and check that it loads in a fresh profile. Select the Valheim community and the correct publishing team in Thunderstore. This is a public beta: the package version is immutable once uploaded, so any correction needs a new version and a rebuild.

## After an API-sensitive change

Check the full compiler output. In particular verify:
- Harmony target methods resolve;
- `ZoneSystem`, `ZDOMan`, `ZNetScene`, `ZPackage` and Jötunn RPC signatures match the installed build;
- no older public API was assumed from online decompilations.

Then run the relevant section of [PLAYTEST.md](PLAYTEST.md).

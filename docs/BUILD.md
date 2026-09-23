# Local build

The compiler must use the assemblies from the same Valheim installation that will run the mod.

## Windows / Steam

Prerequisites: current Valheim, BepInEx, Jotunn and QuickStackPlus installed; .NET SDK capable of building net48.

From the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-local.ps1
```

If Valheim is not in the default Steam location:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-local.ps1 -ValheimDir "D:\SteamLibrary\steamapps\common\Valheim"
```

Successful output is written to `artifacts\FullingPorter.dll`.

If compilation fails, preserve the entire console output. Compiler errors are the useful result at this stage; they tell us which current Valheim/Jotunn signatures need adapting.

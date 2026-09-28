# FullingPorter — local Codex instructions

## Project and roles

- Work in the local Windows checkout of `Likhtenvald/FullingPorter`. Use `develop` as the development base; inspect `git status` and the current branch before editing. Preserve unrelated local changes.
- The user decides gameplay and tests builds in Valheim. ChatGPT Work helps plan and review changes. Codex implements and validates code in the local checkout, where Valheim, BepInEx, Jötunn, PowerShell and .NET are available.
- Do not publish to Thunderstore, create a GitHub release, push commits, or merge branches unless the current task explicitly authorizes that action. A local commit is fine when the task calls for one. Report the commit and exact output path.

## Development checks

- Run `dotnet test .\tests\FullingPorter.Tests\FullingPorter.Tests.csproj -c Release` after changing core behavior.
- Run `powershell -ExecutionPolicy Bypass -File .\scripts\dev.ps1 -NoInstall` for a local game-assembly build. GitHub CI runs game-independent tests but does not compile against the user's Valheim DLLs.
- For an installed dev build, first close Valheim, then run `powershell -ExecutionPolicy Bypass -File .\scripts\dev.ps1`. If detection picks the wrong game or profile, pass `-ValheimDir` and `-ThunderstoreProfile`.
- Changes affecting chest access or inventory transfer require a host/client regression check. Keep inventory mutation server-authoritative; compare source plus destination item counts before and after. A passing unit test is not a substitute for the multiplayer anti-duplication test in `docs/PLAYTEST.md`.

## Versions and release preparation

- Before sharing a changed DLL with another player, keep `src/Plugin.cs`, `FullingPorter.csproj` and `manifest.json` on the same new version. The version compatibility gate cannot distinguish different binaries carrying the same version.
- After the user reports a successful Valheim test and asks for a release candidate, update `packaging/CHANGELOG.md`, `packaging/README.md` and relevant docs. Run the unit tests and a fresh local Release build, then `powershell -ExecutionPolicy Bypass -File .\scripts\package-thunderstore.ps1`.
- Check the new ZIP's filename, manifest and DLL versions, exact root contents and integrity; the packaging script rejects an existing archive with the same version. Report its full `dist\FullingPorter-<version>-thunderstore.zip` path and SHA-256 hash. Leave Thunderstore upload to the user.
- Follow `docs/CODEX_WORKFLOW.md` for handoffs and `docs/BUILD.md` for build details.

# FullingPorter (beta)

Hire a friendly Fuling from Haldor to move stacks from the chests you mark as sources into [QuickStackPlus](https://thunderstore.io/c/valheim/p/Goneryx/QuickStackPlus/) Smart Storage. Set each destination chest's accepted items in QuickStackPlus. The porter uses those filters; it does not create another storage rule system.

## Install

Install FullingPorter with r2modman or Thunderstore Mod Manager. Its declared dependencies are installed automatically. Every player joining a world and the host must use the same FullingPorter version (0.1.3 for this beta).

For manual installation, install BepInExPack Valheim, Jotunn and QuickStackPlus first, then place `FullingPorter.dll` in `BepInEx/plugins/FullingPorter/` on the host and every client.

## Getting started

1. Buy a Fuling Porter Contract from Haldor and use it at your base. One porter can exist per world.
2. Look at a source chest and press **Home** to mark it for pickup.
3. Use QuickStackPlus Smart Storage to choose accepted item types on destination chests.
4. The porter carries up to **10 stacks** per trip within **30 m** of its home by default.

Look at the porter to see its name and current status. Nearby players see and hear the same dialogue when anyone talks to the porter. Press **E** to talk, **End** to rename, or **Delete** twice within three seconds to dismiss it. These keys are configurable.

The host controls contract price, work radius and trip capacity. Key bindings stay local to each player.

## Beta testing

Earlier host/client tests covered transfers, shared settings, activity status and version mismatch handling. In a local host test of 0.1.3, the porter finished four trips and transferred ten stacks while source and destination chests were opened during delivery. Repeat the chest-access and synchronized-dialogue checks with a remote client. Dedicated servers and migration from older saved worlds have not yet been verified. Back up valuable worlds before beta testing.

Please report bugs at [GitHub Issues](https://github.com/Likhtenvald/FullingPorter/issues). Include the mod version, host/client setup, steps to reproduce, item counts before and after a transfer, and `BepInEx/LogOutput.log` from the host when relevant.

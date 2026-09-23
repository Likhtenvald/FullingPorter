using System.Collections.Generic;
using UnityEngine;

namespace FullingPorter.Storage;

/// <summary>
/// Prevents porter inventory mutations from racing a player who is browsing a
/// container. Valheim's local Container.IsInUse() is not authoritative in
/// multiplayer, so the synced ZDO in-use flag is always checked as well.
/// </summary>
internal static class ContainerUseGuard
{
    internal const string PorterLockKey = "FullingPorter.TransferLock";

    private static readonly HashSet<ZDOID> HeldLocks = new();

    internal static bool IsPlayerBusy(Container container)
    {
        if (!container)
            return true;

        var view = container.GetComponent<ZNetView>();
        if (view == null || !view.IsValid())
            return true;

        if (container.IsInUse())
            return true;

        var zdo = view.GetZDO();
        return zdo == null || zdo.GetInt(ZDOVars.s_inUse, 0) == 1;
    }

    internal static bool IsPorterLocked(Container container)
    {
        var view = container ? container.GetComponent<ZNetView>() : null;
        var zdo = view != null && view.IsValid() ? view.GetZDO() : null;
        return zdo != null && zdo.GetBool(PorterLockKey, false);
    }

    internal static bool TryAcquire(Container container, out bool newlyAcquired)
    {
        newlyAcquired = false;

        if (ZNet.instance == null || !ZNet.instance.IsServer() || IsPlayerBusy(container))
            return false;

        var view = container.GetComponent<ZNetView>();
        var zdo = view?.GetZDO();
        if (zdo == null)
            return false;

        if (HeldLocks.Contains(zdo.m_uid))
            return true;

        if (zdo.GetBool(PorterLockKey, false))
            return false;

        zdo.Set(PorterLockKey, true);
        HeldLocks.Add(zdo.m_uid);
        newlyAcquired = true;
        return true;
    }

    internal static void Release(Container container)
    {
        var view = container ? container.GetComponent<ZNetView>() : null;
        var zdo = view != null && view.IsValid() ? view.GetZDO() : null;
        if (zdo == null)
            return;

        if (HeldLocks.Remove(zdo.m_uid) && ZNet.instance != null && ZNet.instance.IsServer())
            zdo.Set(PorterLockKey, false);
    }

    internal static void ClearStaleLock(Container container)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer())
            return;

        var view = container ? container.GetComponent<ZNetView>() : null;
        var zdo = view != null && view.IsValid() ? view.GetZDO() : null;
        if (zdo == null)
            return;

        HeldLocks.Remove(zdo.m_uid);
        if (zdo.GetBool(PorterLockKey, false))
            zdo.Set(PorterLockKey, false);
    }
}

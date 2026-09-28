using System;
using System.Collections.Generic;
using System.Reflection;
using FullingPorter.Core;
using HarmonyLib;
using UnityEngine;

namespace FullingPorter.Porter;

/// <summary>
/// Keeps the porter's work area simulated on the server while the listen-server
/// host is away, including when no player remains at the base. Inventory writes run
/// exclusively through PorterWorker and TransferService on the server.
/// </summary>
internal static class PorterServerArea
{
    private static readonly MethodInfo PokeLocalZone = AccessTools.Method(typeof(ZoneSystem), "PokeLocalZone");
    private static readonly List<ZDO> SectorObjects = new();
    private static readonly HashSet<ZDO> SeenObjects = new();
    private static readonly List<ZDO> CachedAreaObjects = new();
    private static ZoneSystem _cachedWorld;
    private static Vector3 _cachedHome;
    private static float _cachedRadius;
    private static float _nextAreaScan;
    private static float _nextTerrainRefresh;
    private static bool _missingZoneMethodLogged;
    private static bool _areaWasActive;

    internal static bool TryGetActiveArea(out Vector3 home, out float workRadius)
    {
        home = Vector3.zero;
        workRadius = 0f;
        if (ZNet.instance == null || !ZNet.instance.IsServer() ||
            !PorterState.TryGetWorldHome(out home))
            return false;

        workRadius = Mathf.Max(0f, Plugin.WorkRadius.Value);
        var activeRadius = Mathf.Max(64f, workRadius + 32f);
        var host = ZNet.instance.GetReferencePosition();
        var active = PorterActiveAreaRules.NeedsServerArea(
            home.x, home.z,
            host.x, host.z,
            activeRadius);
        if (active != _areaWasActive)
        {
            Plugin.Log.LogInfo(active
                ? "Host is away; porter server work area is active, including with no players nearby."
                : "Host is near the porter; vanilla active area is sufficient.");
            _areaWasActive = active;
        }
        return active;
    }

    private static void ForEachWorkZone(Vector3 home, float radius, Action<Vector2s> action)
    {
        var extent = new Vector3(radius + 3f, 0f, radius + 3f);
        var min = ZoneSystem.GetZone(home - extent);
        var max = ZoneSystem.GetZone(home + extent);
        for (var x = (int)min.x; x <= max.x; ++x)
        {
            for (var y = (int)min.y; y <= max.y; ++y)
                action(new Vector2s(x, y));
        }
    }

    internal static void KeepTerrainLoaded(ZoneSystem zoneSystem)
    {
        if (zoneSystem == null || Time.time < _nextTerrainRefresh ||
            !TryGetActiveArea(out var home, out var radius))
            return;

        if (PokeLocalZone == null)
        {
            if (!_missingZoneMethodLogged)
            {
                Plugin.Log.LogError("Valheim PokeLocalZone is unavailable; porter server area cannot load.");
                _missingZoneMethodLogged = true;
            }
            return;
        }

        // PokeLocalZone resets the TTL of an existing zone. Create at most one
        // new zone per update to avoid a frame spike when the area activates.
        var spawned = false;
        ForEachWorkZone(home, radius, zone =>
        {
            if (spawned) return;
            try
            {
                spawned = (bool)PokeLocalZone.Invoke(zoneSystem, new object[] { zone });
            }
            catch (Exception error)
            {
                Plugin.Log.LogError("Could not keep porter terrain loaded: " + error.GetBaseException().Message);
                spawned = true;
            }
        });
        _nextTerrainRefresh = Time.time + (spawned ? 0.1f : 1f);
    }

    internal static void KeepSceneObjectsLoaded(List<ZDO> nearObjects)
    {
        if (nearObjects == null || ZDOMan.instance == null ||
            !TryGetActiveArea(out var home, out var radius))
        {
            CachedAreaObjects.Clear();
            _cachedWorld = null;
            return;
        }

        if (_cachedWorld != ZoneSystem.instance || _cachedHome != home ||
            _cachedRadius != radius || Time.time >= _nextAreaScan)
        {
            CachedAreaObjects.Clear();
            SeenObjects.Clear();
            ForEachWorkZone(home, radius, zone =>
            {
                SectorObjects.Clear();
                ZDOMan.instance.FindSectorObjects(
                    zone, new SimulationDistance(0, 0, false), SectorObjects, null);
                foreach (var zdo in SectorObjects)
                {
                    if (zdo != null && SeenObjects.Add(zdo))
                        CachedAreaObjects.Add(zdo);
                }
            });
            SectorObjects.Clear();
            SeenObjects.Clear();
            _cachedWorld = ZoneSystem.instance;
            _cachedHome = home;
            _cachedRadius = radius;
            _nextAreaScan = Time.time + 0.5f;
        }

        foreach (var zdo in nearObjects)
            SeenObjects.Add(zdo);
        foreach (var zdo in CachedAreaObjects)
        {
            if (ZDOMan.instance.GetZDO(zdo.m_uid) == zdo && SeenObjects.Add(zdo))
                nearObjects.Add(zdo);
        }
        SeenObjects.Clear();
    }
}

[HarmonyPatch(typeof(ZoneSystem), "Update")]
internal static class PorterZoneUpdatePatch
{
    private static void Postfix(ZoneSystem __instance)
    {
        PorterServerArea.KeepTerrainLoaded(__instance);
    }
}

[HarmonyPatch(typeof(ZNetScene), "CreateObjects")]
internal static class PorterSceneObjectsPatch
{
    private static void Prefix(List<ZDO> currentNearObjects)
    {
        PorterServerArea.KeepSceneObjectsLoaded(currentNearObjects);
    }
}

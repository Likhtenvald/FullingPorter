using System.Collections.Generic;
using UnityEngine;

namespace FullingPorter.Porter;

internal sealed class PorterState : MonoBehaviour, Hoverable
{
    internal const string WorldPresenceKey = "FullingPorter.ActivePorter";

    private const string NameKey = "FullingPorter.Name";
    private const string HomeKey = "FullingPorter.Home";
    private const string HasHomeKey = "FullingPorter.HasHome";
    private const string DefaultName = "$fullingporter_name";

    private static PorterState _serverActive;

    private ZNetView _view;
    private Character _character;
    private PorterWorker _worker;

    private void Awake()
    {
        _view = GetComponent<ZNetView>();
        _character = GetComponent<Character>();
        _worker = GetComponent<PorterWorker>();
        ApplyDisplayName();
    }

    private void Start()
    {
        ApplyDisplayName();

        if (ZNet.instance == null || !ZNet.instance.IsServer())
            return;

        if (_serverActive != null && _serverActive != this)
        {
            Plugin.Log.LogWarning("Duplicate porter detected on server; removing the newer instance.");
            if (_view != null && _view.IsValid() && !_view.IsOwner())
                _view.ClaimOwnership();
            if (ZNetScene.instance != null)
                ZNetScene.instance.Destroy(gameObject);
            return;
        }

        _serverActive = this;
        MarkWorldOccupied();
    }

    private void OnDestroy()
    {
        if (_serverActive == this)
            _serverActive = null;
    }

    internal static bool WorldHasPorter()
    {
        if (_serverActive != null)
            return true;

        // Reconcile the convenience global key against the persistent ZDO set.
        // This also sees porters in unloaded zones, unlike FindObjectsOfType.
        if (ZDOMan.instance != null)
        {
            var zdos = new List<ZDO>();
            ZDOMan.instance.GetAllZDOsWithPrefab(PorterPrefabRegistry.PrefabName, zdos);

            if (zdos.Count > 0)
            {
                MarkWorldOccupied();
                return true;
            }

            ClearWorldOccupied();
            return false;
        }

        foreach (var porter in Object.FindObjectsOfType<PorterState>())
        {
            if (porter != null)
                return true;
        }

        return ZoneSystem.instance != null && ZoneSystem.instance.GetGlobalKey(WorldPresenceKey);
    }

    internal static void MarkWorldOccupied()
    {
        if (ZoneSystem.instance != null && !ZoneSystem.instance.GetGlobalKey(WorldPresenceKey))
            ZoneSystem.instance.SetGlobalKey(WorldPresenceKey);
    }

    internal static void ClearWorldOccupied()
    {
        if (ZoneSystem.instance != null && ZoneSystem.instance.GetGlobalKey(WorldPresenceKey))
            ZoneSystem.instance.RemoveGlobalKey(WorldPresenceKey);
    }

    internal Vector3 GetOrCreateHome(Vector3 fallback)
    {
        if (_view == null || !_view.IsValid())
            return fallback;

        var zdo = _view.GetZDO();
        if (zdo == null)
            return fallback;

        if (zdo.GetBool(HasHomeKey, false))
            return zdo.GetVec3(HomeKey, fallback);

        if (!_view.IsOwner())
            _view.ClaimOwnership();
        if (!_view.IsOwner())
            return fallback;

        zdo.Set(HomeKey, fallback);
        zdo.Set(HasHomeKey, true);
        return fallback;
    }

    internal string PorterName
    {
        get
        {
            if (_view == null || !_view.IsValid()) return DefaultName;
            return _view.GetZDO().GetString(NameKey, DefaultName);
        }
        set
        {
            if (_view == null || !_view.IsValid()) return;
            if (!_view.IsOwner()) _view.ClaimOwnership();
            _view.GetZDO().Set(NameKey, string.IsNullOrWhiteSpace(value) ? DefaultName : value.Trim());
            ApplyDisplayName();
        }
    }

    private void ApplyDisplayName()
    {
        if (_character != null)
            _character.m_name = PorterName;
    }

    public string GetHoverName() => PorterName;
    public float GetHoverOffset() => 1.5f;
    public string GetHoverText()
    {
        var status = _worker != null ? _worker.GetStatusText() : "$fullingporter_status_idle";
        return $"{PorterName}\n{status}\n[<color=yellow><b>$KEY_Use</b></color>] $fullingporter_interact";
    }
}

using System.Globalization;
using Jotunn.Managers;
using UnityEngine;

namespace FullingPorter.Porter;

internal sealed class PorterState : MonoBehaviour, Hoverable
{
    internal const string WorldPresenceKey = "FullingPorter.ActivePorter";
    private const string WorldPresencePrefix = WorldPresenceKey + ":";

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
        MarkWorldOccupied(_view);
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

        foreach (var porter in Object.FindObjectsOfType<PorterState>())
        {
            if (porter == null) continue;

            _serverActive = porter;
            MarkWorldOccupied(porter._view);
            return true;
        }

        var zone = ZoneSystem.instance;
        if (zone == null)
            return false;

        var legacyKeyFound = false;
        var staleKeys = new System.Collections.Generic.List<string>();

        foreach (var key in zone.GetGlobalKeys())
        {
            if (key == WorldPresenceKey)
            {
                legacyKeyFound = true;
                continue;
            }

            if (!key.StartsWith(WorldPresencePrefix))
                continue;

            if (!TryParseWorldKey(key, out var id) || ZDOMan.instance == null || ZDOMan.instance.GetZDO(id) == null)
            {
                staleKeys.Add(key);
                continue;
            }

            return true;
        }

        foreach (var staleKey in staleKeys)
            zone.RemoveGlobalKey(staleKey);

        // Development builds before the ZDOID-backed key used one bare boolean
        // key. If no loaded porter can migrate it, treat it as stale so a deleted
        // porter cannot permanently lock the world.
        if (legacyKeyFound)
            zone.RemoveGlobalKey(WorldPresenceKey);

        return false;
    }

    internal static void MarkWorldOccupied(ZNetView view)
    {
        var zone = ZoneSystem.instance;
        var zdo = view != null && view.IsValid() ? view.GetZDO() : null;
        if (zone == null || zdo == null)
            return;

        if (zone.GetGlobalKey(WorldPresenceKey))
            zone.RemoveGlobalKey(WorldPresenceKey);

        var key = MakeWorldKey(zdo.m_uid);
        if (!zone.GetGlobalKey(key))
            zone.SetGlobalKey(key);
    }

    internal static void ClearWorldOccupied()
    {
        var zone = ZoneSystem.instance;
        if (zone == null)
            return;

        var keys = new System.Collections.Generic.List<string>(zone.GetGlobalKeys());
        foreach (var key in keys)
        {
            if (key == WorldPresenceKey || key.StartsWith(WorldPresencePrefix))
                zone.RemoveGlobalKey(key);
        }

        _serverActive = null;
    }

    private static string MakeWorldKey(ZDOID id)
    {
        return WorldPresencePrefix
            + id.UserID.ToString(CultureInfo.InvariantCulture)
            + ":"
            + id.ID.ToString(CultureInfo.InvariantCulture);
    }

    private static bool TryParseWorldKey(string key, out ZDOID id)
    {
        id = ZDOID.None;
        if (string.IsNullOrEmpty(key) || !key.StartsWith(WorldPresencePrefix))
            return false;

        var value = key.Substring(WorldPresencePrefix.Length);
        var parts = value.Split(':');
        if (parts.Length != 2)
            return false;

        if (!long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var userId) ||
            !uint.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var objectId))
        {
            return false;
        }

        id = new ZDOID(userId, objectId);
        return true;
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
        set => SetNameServer(value);
    }

    internal bool SetNameServer(string value)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() || _view == null || !_view.IsValid())
            return false;

        if (!_view.IsOwner())
            _view.ClaimOwnership();
        if (!_view.IsOwner())
            return false;

        var zdo = _view.GetZDO();
        if (zdo == null)
            return false;

        zdo.Set(NameKey, string.IsNullOrWhiteSpace(value) ? DefaultName : value.Trim());
        ApplyDisplayName();
        return true;
    }

    private void ApplyDisplayName()
    {
        if (_character != null)
            _character.m_name = PorterName;
    }

    internal string BuildHoverText()
    {
        var status = _worker != null ? _worker.GetStatusText() : "$fullingporter_status_idle";
        var name = TranslateTokenWithSuffix(PorterName);
        var localizedStatus = TranslateTokenWithSuffix(status);
        return name + "\n" + localizedStatus;
    }

    private static string TranslateTokenWithSuffix(string value)
    {
        if (string.IsNullOrEmpty(value) || value[0] != '$')
            return value;

        var suffixIndex = value.IndexOf(" (");
        if (suffixIndex < 0)
            return LocalizationManager.Instance.TryTranslate(value);

        var token = value.Substring(0, suffixIndex);
        return LocalizationManager.Instance.TryTranslate(token) + value.Substring(suffixIndex);
    }

    public string GetHoverName() => PorterName;
    public float GetHoverOffset() => 1.5f;
    public string GetHoverText() => BuildHoverText();
}

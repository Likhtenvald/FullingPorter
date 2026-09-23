using Jotunn.Managers;
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

        foreach (var porter in Object.FindObjectsOfType<PorterState>())
        {
            if (porter == null) continue;

            _serverActive = porter;
            MarkWorldOccupied();
            return true;
        }

        // On this Valheim build there is no public ZDOMan API for enumerating
        // unloaded instances by prefab, so avoid pretending we can prove their
        // absence. Keep an existing world key as the conservative fallback.
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

    internal string BuildHoverText()
    {
        var status = _worker != null ? _worker.GetStatusText() : "$fullingporter_status_idle";
        var name = TranslateTokenWithSuffix(PorterName);
        var localizedStatus = TranslateTokenWithSuffix(status);
        var interact = LocalizationManager.Instance.TryTranslate("$fullingporter_interact");
        return name + "\n" + localizedStatus + "\n[<color=yellow><b>E</b></color>] " + interact;
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

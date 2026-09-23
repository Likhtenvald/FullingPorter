using UnityEngine;

namespace FullingPorter.Porter;

internal sealed class PorterState : MonoBehaviour, Hoverable
{
    internal const string WorldPresenceKey = "FullingPorter.ActivePorter";

    private const string NameKey = "FullingPorter.Name";
    private const string DefaultName = "$fullingporter_name";

    private static PorterState _serverActive;

    private ZNetView _view;
    private Character _character;

    private void Awake()
    {
        _view = GetComponent<ZNetView>();
        _character = GetComponent<Character>();
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

        if (ZoneSystem.instance != null && ZoneSystem.instance.GetGlobalKey(WorldPresenceKey))
            return true;

        foreach (var porter in Object.FindObjectsOfType<PorterState>())
        {
            if (porter != null)
                return true;
        }

        return false;
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
    public float GetHoverOffset() => 2.3f;
    public string GetHoverText() => $"{PorterName}\n[<color=yellow><b>$KEY_Use</b></color>] $fullingporter_interact";
}

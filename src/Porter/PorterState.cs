using UnityEngine;

namespace FullingPorter.Porter;

internal sealed class PorterState : MonoBehaviour, Hoverable
{
    private const string NameKey = "FullingPorter.Name";
    private const string DefaultName = "$fullingporter_name";

    private ZNetView _view;
    private Character _character;

    private void Awake()
    {
        _view = GetComponent<ZNetView>();
        _character = GetComponent<Character>();
        ApplyDisplayName();
    }

    private void Start() => ApplyDisplayName();

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
    public string GetHoverText() => $"{PorterName}\n[<color=yellow><b>$KEY_Use</b></color>] $fullingporter_interact";
}

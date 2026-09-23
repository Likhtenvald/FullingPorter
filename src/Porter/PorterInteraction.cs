using UnityEngine;

namespace FullingPorter.Porter;

internal sealed class PorterInteraction : MonoBehaviour, Interactable, TextReceiver
{
    private PorterState _state;

    private void Awake() => _state = GetComponent<PorterState>();

    public bool Interact(Humanoid character, bool hold, bool alt)
    {
        if (hold || character != Player.m_localPlayer) return false;
        TextInput.instance.RequestText(this, "$fullingporter_rename_title", 24);
        return true;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;
    public string GetText() => _state?.PorterName ?? string.Empty;
    public void SetText(string text)
    {
        if (_state != null) _state.PorterName = text;
    }
}

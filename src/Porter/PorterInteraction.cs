using UnityEngine;

namespace FullingPorter.Porter;

internal sealed class PorterInteraction : MonoBehaviour, Interactable, TextReceiver
{
    private PorterState _state;
    private ZNetView _view;

    private void Awake()
    {
        _state = GetComponent<PorterState>();
        _view = GetComponent<ZNetView>();
    }

    public bool Interact(Humanoid character, bool hold, bool alt)
    {
        if (hold || character != Player.m_localPlayer) return false;

        if (alt)
        {
            Dismiss(character);
            return true;
        }

        TextInput.instance.RequestText(this, "$fullingporter_rename_title", 24);
        return true;
    }

    private void Dismiss(Humanoid character)
    {
        if (_view == null || !_view.IsValid() || ZNetScene.instance == null)
            return;

        if (!_view.IsOwner())
            _view.ClaimOwnership();

        character.Message(MessageHud.MessageType.Center, "$fullingporter_dismissed");
        ZNetScene.instance.Destroy(gameObject);
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;
    public string GetText() => _state?.PorterName ?? string.Empty;

    public void SetText(string text)
    {
        if (_state != null) _state.PorterName = text;
    }
}

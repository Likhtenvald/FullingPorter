using UnityEngine;

namespace FullingPorter.Porter;

internal sealed class PorterInteraction : MonoBehaviour, Interactable, TextReceiver
{
    private const float DismissConfirmSeconds = 3f;

    private PorterState _state;
    private ZNetView _view;
    private float _dismissConfirmUntil;

    private void Awake()
    {
        _state = GetComponent<PorterState>();
        _view = GetComponent<ZNetView>();
    }

    private void Update()
    {
        var player = Player.m_localPlayer;
        if (player == null || Plugin.DismissKey == null || !Input.GetKeyDown(Plugin.DismissKey.Value))
            return;

        var hover = player.GetHoverObject();
        if (hover == null || hover.GetComponentInParent<PorterInteraction>() != this)
            return;

        if (Time.time <= _dismissConfirmUntil)
        {
            Dismiss(player);
            return;
        }

        _dismissConfirmUntil = Time.time + DismissConfirmSeconds;
        player.Message(MessageHud.MessageType.Center, "$fullingporter_dismiss_confirm");
    }

    public bool Interact(Humanoid character, bool hold, bool alt)
    {
        if (hold || character != Player.m_localPlayer) return false;

        TextInput.instance.RequestText(this, "$fullingporter_rename_title", 24);
        return true;
    }

    private void Dismiss(Humanoid character)
    {
        if (_view == null || !_view.IsValid() || ZNetScene.instance == null)
            return;

        if (!_view.IsOwner())
            _view.ClaimOwnership();

        PorterState.ClearWorldOccupied();
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

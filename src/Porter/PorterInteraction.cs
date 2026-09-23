using FullingPorter.Network;
using UnityEngine;

namespace FullingPorter.Porter;

internal sealed class PorterInteraction : MonoBehaviour, Interactable, TextReceiver
{
    private const float DismissConfirmSeconds = 3f;

    private PorterState _state;
    private ZNetView _view;
    private PorterPersonality _personality;
    private float _dismissConfirmUntil;

    private void Awake()
    {
        _state = GetComponent<PorterState>();
        _view = GetComponent<ZNetView>();
        _personality = GetComponent<PorterPersonality>();
    }

    private void Update()
    {
        var player = Player.m_localPlayer;
        if (player == null)
            return;

        var dismissPressed = Plugin.DismissKey != null && Input.GetKeyDown(Plugin.DismissKey.Value);
        var renamePressed = Plugin.RenameKey != null && Input.GetKeyDown(Plugin.RenameKey.Value);
        if (!dismissPressed && !renamePressed)
            return;

        var hover = player.GetHoverObject();
        if (hover == null || hover.GetComponentInParent<PorterInteraction>() != this)
            return;

        if (renamePressed)
        {
            RequestRename();
            return;
        }

        if (Time.time <= _dismissConfirmUntil)
        {
            PorterRpc.RequestDismiss(_view);
            _dismissConfirmUntil = 0f;
            return;
        }

        _dismissConfirmUntil = Time.time + DismissConfirmSeconds;
        player.Message(MessageHud.MessageType.Center, "$fullingporter_dismiss_confirm");
    }

    public bool Interact(Humanoid character, bool hold, bool alt)
    {
        if (hold || character != Player.m_localPlayer)
            return false;

        return _personality != null && _personality.React(Player.m_localPlayer);
    }

    private void RequestRename()
    {
        if (TextInput.instance != null)
            TextInput.instance.RequestText(this, "$fullingporter_rename_title", 24);
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;
    public string GetText() => _state?.PorterName ?? string.Empty;

    public void SetText(string text)
    {
        PorterRpc.RequestRename(_view, text);
    }
}

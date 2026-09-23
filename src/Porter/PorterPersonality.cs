using FullingPorter.Core;
using FullingPorter.Network;
using Jotunn.Managers;
using UnityEngine;

namespace FullingPorter.Porter;

internal sealed class PorterPersonality : MonoBehaviour
{
    private static readonly string[] IdleLines =
    {
        "$fullingporter_talk_idle_01",
        "$fullingporter_talk_idle_02",
        "$fullingporter_talk_idle_03",
        "$fullingporter_talk_idle_04",
        "$fullingporter_talk_idle_05",
        "$fullingporter_talk_idle_06"
    };

    private static readonly string[] WorkingLines =
    {
        "$fullingporter_talk_work_01",
        "$fullingporter_talk_work_02",
        "$fullingporter_talk_work_03",
        "$fullingporter_talk_work_05",
        "$fullingporter_talk_work_06",
        "$fullingporter_talk_work_07",
        "$fullingporter_talk_work_04"
    };

    private static readonly string[] ReturningLines =
    {
        "$fullingporter_talk_return_01",
        "$fullingporter_talk_return_02",
        "$fullingporter_talk_return_03",
        "$fullingporter_talk_idle_040"
    };

    private static readonly string[] BlockedLines =
    {
        "$fullingporter_talk_blocked_01",
        "$fullingporter_talk_blocked_02",
        "$fullingporter_talk_blocked_03",
        "$fullingporter_talk_idle_041"
    };

    private static readonly string[] VoicePrefabs =
    {
        "sfx_goblin_idle",
        "sfx_goblin_idle",
        "sfx_goblin_idle",
        "sfx_goblin_alerted"
    };

    private PorterWorker _worker;
    private PorterState _state;
    private ZNetView _view;
    private float _nextReactionTime;
    private float _nextAmbientReactionTime;
    private const float ReactionCooldown = 2.5f;
    internal const float AmbientHearDistance = 18f;
    private const float AmbientMinInterval = 18f;
    private const float AmbientMaxInterval = 42f;

    private void Awake()
    {
        _worker = GetComponent<PorterWorker>();
        _state = GetComponent<PorterState>();
        _view = GetComponent<ZNetView>();
        ScheduleNextAmbient();
    }

    private void Update()
    {
        if (Time.time < _nextAmbientReactionTime)
            return;

        var player = Player.m_localPlayer;
        if (player == null)
        {
            ScheduleNextAmbient();
            return;
        }

        var distanceSqr = (player.transform.position - transform.position).sqrMagnitude;
        if (distanceSqr > AmbientHearDistance * AmbientHearDistance)
        {
            _nextAmbientReactionTime = Time.time + 5f;
            return;
        }

        PorterRpc.RequestSpeak(_view, true);
        ScheduleNextAmbient();
    }

    internal bool React(Player player)
    {
        if (player == null || Time.time < _nextReactionTime)
            return false;

        if (!PorterRpc.RequestSpeak(_view, false))
            return false;

        _nextReactionTime = Time.time + ReactionCooldown;

        // Keep local ambient requests away from a manual interaction. The
        // server also enforces its own cooldown for all connected players.
        if (_nextAmbientReactionTime < Time.time + 8f)
            _nextAmbientReactionTime = Time.time + 8f;

        return true;
    }

    internal bool TrySpeakServer(bool ambient)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() ||
            _view == null || !_view.IsValid() || Time.time < _nextReactionTime ||
            (ambient && Time.time < _nextAmbientReactionTime))
            return false;

        _nextReactionTime = Time.time + ReactionCooldown;
        ScheduleNextAmbient();

        var line = PickLine();
        var voiceIndex = Random.Range(0, VoicePrefabs.Length);
        PlaySpeech(line, voiceIndex);
        PorterRpc.BroadcastSpeech(_view, line, voiceIndex);
        return true;
    }

    internal void PlaySpeech(string line, int voiceIndex)
    {
        if (string.IsNullOrEmpty(line) || voiceIndex < 0 || voiceIndex >= VoicePrefabs.Length ||
            Player.m_localPlayer == null)
            return;

        if ((Player.m_localPlayer.transform.position - transform.position).sqrMagnitude > 20f * 20f)
            return;

        var localized = LocalizationManager.Instance.TryTranslate(line);
        ShowSpeechBubble(Player.m_localPlayer, localized);
        PlayVoice(voiceIndex);
    }

    private void ScheduleNextAmbient()
    {
        _nextAmbientReactionTime = Time.time + Random.Range(AmbientMinInterval, AmbientMaxInterval);
    }

    private void ShowSpeechBubble(Player player, string text)
    {
        if (Chat.instance == null)
        {
            player.Message(MessageHud.MessageType.Center, text);
            return;
        }

        var speakerName = _state != null ? _state.PorterName : "$fullingporter_name";
        speakerName = LocalizationManager.Instance.TryTranslate(speakerName);

        Chat.instance.SetNpcText(
            gameObject,
            Vector3.up * 1.8f,
            20f,
            4f,
            speakerName,
            text,
            false);
    }

    private string PickLine()
    {
        var context = _worker != null
            ? _worker.GetDialogueContext()
            : PorterDialogueContext.Idle;

        string[] lines;
        switch (context)
        {
            case PorterDialogueContext.Working:
                lines = WorkingLines;
                break;
            case PorterDialogueContext.Returning:
                lines = ReturningLines;
                break;
            case PorterDialogueContext.Blocked:
                lines = BlockedLines;
                break;
            default:
                lines = IdleLines;
                break;
        }

        return lines[Random.Range(0, lines.Length)];
    }

    private void PlayVoice(int voiceIndex)
    {
        var prefabName = VoicePrefabs[voiceIndex];
        var soundPrefab = PrefabManager.Instance.GetPrefab(prefabName);
        if (soundPrefab == null)
        {
            Plugin.Log.LogDebug($"Porter voice prefab not found: {prefabName}");
            return;
        }

        Object.Instantiate(soundPrefab, transform.position + Vector3.up * 1.2f, Quaternion.identity);
    }
}

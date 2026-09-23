using Jotunn.Managers;
using UnityEngine;

namespace FullingPorter.Porter;

internal sealed class PorterPersonality : MonoBehaviour
{
    private static readonly string[] IdleLines =
    {
        "$fullingporter_talk_idle_01",
        "$fullingporter_talk_idle_02",
        "$fullingporter_talk_idle_04",
        "$fullingporter_talk_idle_06",
        "$fullingporter_talk_idle_07",
        "$fullingporter_talk_idle_08"
    };

    private static readonly string[] WorkingLines =
    {
        "$fullingporter_talk_work_01",
        "$fullingporter_talk_work_02",
        "$fullingporter_talk_work_03",
        "$fullingporter_talk_work_04",
        "$fullingporter_talk_work_05",
        "$fullingporter_talk_work_06",
        "$fullingporter_talk_work_08"
    };

    private static readonly string[] ReturningLines =
    {
        "$fullingporter_talk_return_01",
        "$fullingporter_talk_return_03",
        "$fullingporter_talk_return_04",
        "$fullingporter_talk_return_05"
    };

    private static readonly string[] BlockedLines =
    {
        "$fullingporter_talk_blocked_01",
        "$fullingporter_talk_blocked_02",
        "$fullingporter_talk_blocked_03",
        "$fullingporter_talk_blocked_06"
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
    private float _nextReactionTime;
    private float _nextAmbientReactionTime;
    private const float ReactionCooldown = 2.5f;
    private const float AmbientHearDistance = 18f;
    private const float AmbientMinInterval = 18f;
    private const float AmbientMaxInterval = 42f;

    private void Awake()
    {
        _worker = GetComponent<PorterWorker>();
        _state = GetComponent<PorterState>();
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

        Speak(player);
        ScheduleNextAmbient();
    }

    internal bool React(Player player)
    {
        if (player == null || Time.time < _nextReactionTime)
            return false;

        _nextReactionTime = Time.time + ReactionCooldown;
        Speak(player);

        // A manual interaction should not be followed immediately by ambient
        // chatter, otherwise E can accidentally produce two lines in a row.
        if (_nextAmbientReactionTime < Time.time + 8f)
            _nextAmbientReactionTime = Time.time + 8f;

        return true;
    }

    private void Speak(Player player)
    {
        var line = PickLine();
        var localized = LocalizationManager.Instance.TryTranslate(line);

        ShowSpeechBubble(player, localized);
        PlayVoice();
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
            : PorterWorker.DialogueContext.Idle;

        string[] lines;
        switch (context)
        {
            case PorterWorker.DialogueContext.Working:
                lines = WorkingLines;
                break;
            case PorterWorker.DialogueContext.Returning:
                lines = ReturningLines;
                break;
            case PorterWorker.DialogueContext.Blocked:
                lines = BlockedLines;
                break;
            default:
                lines = IdleLines;
                break;
        }

        return lines[Random.Range(0, lines.Length)];
    }

    private void PlayVoice()
    {
        var prefabName = VoicePrefabs[Random.Range(0, VoicePrefabs.Length)];
        var soundPrefab = PrefabManager.Instance.GetPrefab(prefabName);
        if (soundPrefab == null)
        {
            Plugin.Log.LogDebug($"Porter voice prefab not found: {prefabName}");
            return;
        }

        Object.Instantiate(soundPrefab, transform.position + Vector3.up * 1.2f, Quaternion.identity);
    }
}

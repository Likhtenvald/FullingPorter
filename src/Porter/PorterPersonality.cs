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
        "$fullingporter_talk_idle_06",
        "$fullingporter_talk_idle_07"
    };

    private static readonly string[] WorkingLines =
    {
        "$fullingporter_talk_work_01",
        "$fullingporter_talk_work_02",
        "$fullingporter_talk_work_03",
        "$fullingporter_talk_work_04",
        "$fullingporter_talk_work_05",
        "$fullingporter_talk_work_06",
        "$fullingporter_talk_work_07"
    };

    private static readonly string[] ReturningLines =
    {
        "$fullingporter_talk_return_01",
        "$fullingporter_talk_return_02",
        "$fullingporter_talk_return_03",
        "$fullingporter_talk_return_04",
        "$fullingporter_talk_return_05"
    };

    private static readonly string[] BlockedLines =
    {
        "$fullingporter_talk_blocked_01",
        "$fullingporter_talk_blocked_02",
        "$fullingporter_talk_blocked_03",
        "$fullingporter_talk_blocked_04",
        "$fullingporter_talk_blocked_05",
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
    private float _nextReactionTime;
    private const float ReactionCooldown = 2.5f;

    private void Awake()
    {
        _worker = GetComponent<PorterWorker>();
    }

    internal bool React(Player player)
    {
        if (player == null || Time.time < _nextReactionTime)
            return false;

        _nextReactionTime = Time.time + ReactionCooldown;

        var line = PickLine();
        var localized = LocalizationManager.Instance.TryTranslate(line);
        player.Message(MessageHud.MessageType.Center, localized);

        PlayVoice();
        return true;
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

using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace FullingPorter.Porter;

internal static class PorterPrefabRegistry
{
    internal const string PrefabName = "FullingPorter_Fuling";

    internal static void Register()
    {
        var source = PrefabManager.Instance.GetPrefab("Goblin");
        if (source == null)
        {
            Plugin.Log.LogError("Vanilla Goblin prefab was not found.");
            return;
        }

        // Clone through Jotunn's prefab manager. It temporarily deactivates the
        // source before Unity cloning, preventing network/AI Awake methods from
        // running against an unregistered prefab.
        var prefab = PrefabManager.Instance.CreateClonedPrefab(PrefabName, source);
        if (prefab == null)
        {
            Plugin.Log.LogError("Could not clone the porter prefab from vanilla Goblin.");
            return;
        }

        // Keep the vanilla MonsterAI component so Character/EnemyHud retain the
        // BaseAI references they expect, but disable its Update loop. Removing the
        // component entirely makes EnemyHud dereference a missing BaseAI every frame.
        // PorterWorker is the only component that drives porter behaviour.
        var monsterAi = prefab.GetComponent<MonsterAI>();
        if (monsterAi != null)
            monsterAi.enabled = false;

        var character = prefab.GetComponent<Character>();
        if (character != null)
            character.m_faction = Character.Faction.Players;

        var custom = new CustomPrefab(prefab, true);
        prefab.AddComponent<PorterState>();
        prefab.AddComponent<PorterWorker>();
        prefab.AddComponent<PorterInteraction>();

        // Runtime AI neutralisation is finalized in PorterWorker because
        // vanilla Character/MonsterAI initialization order matters.
        PrefabManager.Instance.AddPrefab(custom);
    }
}

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

        // Replace combat MonsterAI on the inactive cloned prefab with a passive
        // BaseAI subclass. EnemyHud still sees a BaseAI, while locomotion and
        // pathfinding stay vanilla and no hostile thinking is inherited.
        var monsterAi = prefab.GetComponent<MonsterAI>();
        if (monsterAi != null)
            Object.DestroyImmediate(monsterAi);

        var movementAi = prefab.GetComponent<PorterMovementAI>();
        if (movementAi == null)
            movementAi = prefab.AddComponent<PorterMovementAI>();
        movementAi.m_pathAgentType = Pathfinding.AgentType.Humanoid;

        var character = prefab.GetComponent<Character>();
        if (character != null)
            character.m_faction = Character.Faction.Players;

        var custom = new CustomPrefab(prefab, true);
        prefab.AddComponent<PorterState>();
        prefab.AddComponent<PorterWorker>();
        if (prefab.GetComponent<Talker>() == null)
            prefab.AddComponent<Talker>();
        prefab.AddComponent<PorterPersonality>();
        prefab.AddComponent<PorterInteraction>();

        PrefabManager.Instance.AddPrefab(custom);
    }
}

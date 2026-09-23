using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace FullingPorter.Porter;

internal static class PorterPrefabRegistry
{
    internal const string PrefabName = "FullingPorter_Fuling";

    internal static void Register()
    {
        var source = PrefabManager.Instance.GetPrefab("Troll");
        if (source == null)
        {
            Plugin.Log.LogError("Vanilla Troll prefab was not found.");
            return;
        }

        // Clone through Jotunn's prefab manager. It temporarily deactivates the
        // source before Unity cloning, preventing network/AI Awake methods from
        // running against an unregistered prefab.
        var prefab = PrefabManager.Instance.CreateClonedPrefab(PrefabName, source);
        if (prefab == null)
        {
            Plugin.Log.LogError("Could not clone the porter prefab from vanilla Troll.");
            return;
        }

        // Keep the troll's visual/animation rig, but shrink the entire body to
        // roughly Fuling size. Collider dimensions shrink with the prefab too.
        // PorterMovementAI below uses a Humanoid path agent so navigation is
        // planned for door-sized passages rather than full-size troll clearance.
        prefab.transform.localScale = source.transform.localScale * 0.45f;

        // Replace combat MonsterAI on the inactive cloned prefab with a passive
        // BaseAI subclass. EnemyHud still sees a BaseAI, while locomotion and
        // pathfinding stay vanilla and no hostile thinking is inherited.
        var monsterAi = prefab.GetComponent<MonsterAI>();
        if (monsterAi != null)
            Object.DestroyImmediate(monsterAi);

        if (prefab.GetComponent<PorterMovementAI>() == null)
            prefab.AddComponent<PorterMovementAI>();

        var character = prefab.GetComponent<Character>();
        if (character != null)
            character.m_faction = Character.Faction.Players;

        var custom = new CustomPrefab(prefab, true);
        prefab.AddComponent<PorterState>();
        prefab.AddComponent<PorterWorker>();
        prefab.AddComponent<PorterInteraction>();

        PrefabManager.Instance.AddPrefab(custom);
    }
}

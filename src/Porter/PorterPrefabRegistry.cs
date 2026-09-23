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

        var custom = new CustomPrefab(PrefabName, source);
        var prefab = custom.Prefab;
        prefab.AddComponent<PorterState>();
        prefab.AddComponent<PorterWorker>();\n        prefab.AddComponent<PorterInteraction>();

        // Runtime AI neutralisation is finalized in PorterWorker because
        // vanilla Character/MonsterAI initialization order matters.
        PrefabManager.Instance.AddPrefab(custom);
    }
}

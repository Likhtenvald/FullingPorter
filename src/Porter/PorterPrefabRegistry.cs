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

        // Do not Instantiate the networked creature prefab here. Instantiating a
        // vanilla ZNetView prefab outside ZNetScene spawning runs Awake immediately
        // without a valid ZDO and leaves Character/AI components half initialized.
        // Jotunn clones the source safely while registering the custom prefab.
        var custom = new CustomPrefab(PrefabName, "Goblin", true);
        var prefab = custom.Prefab;
        if (prefab == null)
        {
            Plugin.Log.LogError("Could not create the porter prefab from vanilla Goblin.");
            return;
        }

        prefab.AddComponent<PorterState>();
        prefab.AddComponent<PorterWorker>();
        prefab.AddComponent<PorterInteraction>();

        // Runtime AI neutralisation is finalized in PorterWorker because
        // vanilla Character/MonsterAI initialization order matters.
        PrefabManager.Instance.AddPrefab(custom);
    }
}

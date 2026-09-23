using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn;
using Jotunn.Managers;
using FullingPorter.Localization;
using UnityEngine;

namespace FullingPorter;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency(Jotunn.Main.ModGuid)]
[BepInDependency(QuickStackPlusGuid, BepInDependency.DependencyFlags.HardDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "Likhtenvald.FullingPorter";
    public const string PluginName = "FullingPorter";
    public const string PluginVersion = "0.1.0";
    public const string QuickStackPlusGuid = "Goneryx.QuickStackPlus";

    internal static BepInEx.Logging.ManualLogSource Log;
    internal static ConfigEntry<int> ContractPrice;
    internal static ConfigEntry<float> WorkRadius;
    internal static ConfigEntry<int> MaxStacksPerTrip;
    internal static ConfigEntry<bool> PorterCanDie;
    internal static ConfigEntry<KeyCode> DismissKey;

    private Harmony _harmony;

    private void Awake()
    {
        Log = Logger;
        ContractPrice = Config.Bind("Contract", "Price", 10, "Haldor contract price in coins.");
        WorkRadius = Config.Bind("Porter", "WorkRadius", 30f, "Maximum work radius in metres.");
        MaxStacksPerTrip = Config.Bind("Porter", "MaxStacksPerTrip", 4, "Maximum distinct stacks carried per trip.");
        PorterCanDie = Config.Bind("Porter", "CanDie", true, "Whether the porter can take lethal damage.");
        DismissKey = Config.Bind("Porter", "DismissKey", KeyCode.Delete, "Key used while looking at a porter to dismiss it. Press twice to confirm.");

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll();

        // Register translations during plugin startup, before Valheim/Jotunn
        // finishes setting up the active language. Prefab registration stays
        // on OnVanillaPrefabsAvailable because it depends on vanilla prefabs.
        LocalizationRegistry.Register();
        PrefabManager.OnVanillaPrefabsAvailable += OnVanillaPrefabsAvailable;
        Logger.LogInfo($"{PluginName} {PluginVersion} loaded.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }

    private void OnVanillaPrefabsAvailable()
    {
        Contract.ContractRegistry.Register();
        Porter.PorterPrefabRegistry.Register();
        PrefabManager.OnVanillaPrefabsAvailable -= OnVanillaPrefabsAvailable;
    }
}

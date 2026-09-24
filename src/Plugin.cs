using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn;
using Jotunn.Managers;
using Jotunn.Utils;
using FullingPorter.Localization;
using FullingPorter.Network;
using UnityEngine;

namespace FullingPorter;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency(Jotunn.Main.ModGuid)]
[BepInDependency(QuickStackPlusGuid, BepInDependency.DependencyFlags.HardDependency)]
[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "Likhtenvald.FullingPorter";
    public const string PluginName = "FullingPorter";
    public const string PluginVersion = "0.1.3";
    public const string QuickStackPlusGuid = "Goneryx.QuickStackPlus";

    internal static BepInEx.Logging.ManualLogSource Log;
    internal static ConfigEntry<int> ContractPrice;
    internal static ConfigEntry<float> WorkRadius;
    internal static ConfigEntry<int> MaxStacksPerTrip;
    internal static ConfigEntry<KeyCode> DismissKey;
    internal static ConfigEntry<KeyCode> RenameKey;
    internal static ConfigEntry<KeyCode> SourceChestKey;

    private Harmony _harmony;

    private void Awake()
    {
        Log = Logger;
        var serverOnly = new ConfigurationManagerAttributes { IsAdminOnly = true };

        ContractPrice = Config.Bind(
            "Contract",
            "Price",
            1500,
            new ConfigDescription(
                "Haldor contract price in coins. Synchronized from the server.",
                null,
                serverOnly));

        WorkRadius = Config.Bind(
            "Porter",
            "WorkRadius",
            30f,
            new ConfigDescription(
                "Maximum work radius in metres. Synchronized from the server.",
                null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));

        MaxStacksPerTrip = Config.Bind(
            "Porter",
            "MaxStacksPerTrip",
            10,
            new ConfigDescription(
                "Maximum distinct stacks carried per trip. Synchronized from the server.",
                null,
                new ConfigurationManagerAttributes { IsAdminOnly = true }));
        DismissKey = Config.Bind("Porter", "DismissKey", KeyCode.Delete, "Key used while looking at a porter to dismiss it. Press twice to confirm.");
        RenameKey = Config.Bind("Porter", "RenameKey", KeyCode.End, "Key used while looking at a porter to rename it.");
        SourceChestKey = Config.Bind("Porter", "SourceChestKey", KeyCode.Home, "Key used while looking at a container to toggle it as a porter source.");

        PorterRpc.Register();

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

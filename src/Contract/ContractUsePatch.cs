using FullingPorter.Network;
using HarmonyLib;
using UnityEngine;

namespace FullingPorter.Contract;

[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UseItem))]
internal static class ContractUsePatch
{
    private static bool Prefix(Humanoid __instance, Inventory inventory, ItemDrop.ItemData item, bool fromInventoryGui)
    {
        if (__instance != Player.m_localPlayer || item?.m_dropPrefab?.name != ContractRegistry.ItemName)
            return true;

        var forward = __instance.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.01f)
            forward = Vector3.forward;

        PorterRpc.RequestSpawn(__instance.transform.position, forward);
        return false;
    }
}

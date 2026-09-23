using FullingPorter.Porter;
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

        if (PorterState.WorldHasPorter())
        {
            __instance.Message(MessageHud.MessageType.Center, "$fullingporter_already_exists");
            return false;
        }

        var prefab = ZNetScene.instance?.GetPrefab(PorterPrefabRegistry.PrefabName);
        if (prefab == null)
        {
            Plugin.Log.LogError("Porter prefab is not registered in ZNetScene.");
            return false;
        }

        var forward = __instance.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;

        var spawned = Object.Instantiate(prefab, __instance.transform.position + forward.normalized * 2f,
            Quaternion.LookRotation(-forward.normalized));
        if (spawned == null) return false;

        PorterState.MarkWorldOccupied();
        inventory.RemoveOneItem(item);
        __instance.Message(MessageHud.MessageType.Center, "$fullingporter_spawned");
        return false;
    }
}

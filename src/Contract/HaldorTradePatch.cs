using HarmonyLib;
using UnityEngine;

namespace FullingPorter.Contract;

[HarmonyPatch(typeof(Trader), nameof(Trader.GetAvailableItems))]
internal static class HaldorTradePatch
{
    private static void Postfix(Trader __instance, ref System.Collections.Generic.List<Trader.TradeItem> __result)
    {
        if (__instance == null || __result == null) return;

        // Haldor is the vanilla Black Forest trader.
        // Use the trader prefab identity rather than localized display text.
        if (!__instance.name.StartsWith("Haldor") && !__instance.name.StartsWith("Trader")) return;

        var prefab = ObjectDB.instance?.GetItemPrefab(ContractRegistry.ItemName);
        var drop = prefab ? prefab.GetComponent<ItemDrop>() : null;
        if (drop == null) return;

        foreach (var trade in __result)
            if (trade?.m_prefab == drop)
                return;

        __result.Add(new Trader.TradeItem
        {
            m_prefab = drop,
            m_stack = 1,
            m_price = Plugin.ContractPrice.Value,
            m_requiredGlobalKey = string.Empty
        });
    }
}

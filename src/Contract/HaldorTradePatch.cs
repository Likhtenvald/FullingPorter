using HarmonyLib;
using UnityEngine;

namespace FullingPorter.Contract;

[HarmonyPatch(typeof(Trader), nameof(Trader.GetAvailableItems))]
internal static class HaldorTradePatch
{
    private static bool _logged;

    private static void Postfix(Trader __instance, ref System.Collections.Generic.List<Trader.TradeItem> __result)
    {
        if (__instance == null || __result == null) return;
        if (!__instance.name.StartsWith("Haldor") && !__instance.name.StartsWith("Trader")) return;

        var contract = ContractRegistry.Contract?.ItemDrop;
        if (contract == null)
        {
            Plugin.Log.LogWarning("Haldor trade injection skipped: contract ItemDrop is not registered.");
            return;
        }

        // GetAvailableItems can be called repeatedly while the store is open.
        if (__result.Exists(item => item != null && item.m_prefab == contract))
            return;

        var shared = contract.m_itemData?.m_shared;
        var trade = new Trader.TradeItem
        {
            m_prefab = contract,
            m_stack = 1,
            m_price = Plugin.ContractPrice.Value,
            m_requiredGlobalKey = null,
            m_levelUpEffect = false,
            m_buyPlayerEffects = new EffectList(),
            m_icon = shared?.m_icons != null && shared.m_icons.Length > 0 ? shared.m_icons[0] : null,
            m_name = shared?.m_name ?? "$fullingporter_contract",
            m_tooltip = shared?.m_description ?? "$fullingporter_contract_desc",
            m_buyKey = string.Empty,
            m_incrementKey = string.Empty,
            m_incrementAmount = 0
        };

        __result.Add(trade);

        if (!_logged)
        {
            _logged = true;
            Plugin.Log.LogInfo("Fulling Porter contract added to Haldor with the current Trader.TradeItem fields populated.");
        }
    }
}

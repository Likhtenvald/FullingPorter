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

        // Temporarily do not inject the custom trade. The current Valheim StoreGui
        // expects additional TradeItem fields that our initial compatibility shim
        // does not populate. Keeping the patch active but non-mutating lets us
        // verify that Haldor itself is healthy before wiring the current API.
        if (!_logged)
        {
            _logged = true;
            Plugin.Log.LogWarning("Haldor integration is temporarily disabled while validating the current Trader.TradeItem API.");
        }
    }
}

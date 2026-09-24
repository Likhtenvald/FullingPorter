using FullingPorter.Porter;
using HarmonyLib;

namespace FullingPorter.Patches;

[HarmonyPatch(typeof(Character), nameof(Character.Damage))]
internal static class PorterDamagePatch
{
    private static bool Prefix(Character __instance)
    {
        return __instance == null || __instance.GetComponent<PorterState>() == null;
    }
}

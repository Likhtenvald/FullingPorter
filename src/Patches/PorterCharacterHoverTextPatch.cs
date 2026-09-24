using FullingPorter.Porter;
using HarmonyLib;

namespace FullingPorter.Patches;

[HarmonyPatch(typeof(Character), nameof(Character.GetHoverText))]
internal static class PorterCharacterHoverTextPatch
{
    private static void Postfix(Character __instance, ref string __result)
    {
        var state = __instance != null ? __instance.GetComponent<PorterState>() : null;
        if (state == null)
            return;

        __result = state.BuildHoverText();
    }
}

using FullingPorter.Porter;
using HarmonyLib;

namespace FullingPorter.Patches;

[HarmonyPatch(typeof(EnemyHud), nameof(EnemyHud.ShowHud))]
internal static class PorterEnemyHudPatch
{
    private static bool Prefix(Character c)
    {
        return c == null || c.GetComponent<PorterState>() == null;
    }
}

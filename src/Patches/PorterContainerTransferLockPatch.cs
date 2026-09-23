using FullingPorter.Storage;
using HarmonyLib;

namespace FullingPorter.Patches;

[HarmonyPatch(typeof(Container), "Interact")]
internal static class PorterContainerTransferLockPatch
{
    private static bool Prefix(
        Container __instance,
        Humanoid character,
        bool hold,
        bool alt,
        ref bool __result)
    {
        if (hold || character != Player.m_localPlayer || !ContainerUseGuard.IsPorterLocked(__instance))
            return true;

        Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "$msg_inuse");
        __result = false;
        return false;
    }
}

[HarmonyPatch(typeof(Container), "Awake")]
internal static class PorterContainerStaleLockPatch
{
    private static void Postfix(Container __instance)
    {
        ContainerUseGuard.ClearStaleLock(__instance);
    }
}

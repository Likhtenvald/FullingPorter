using FullingPorter.Storage;
using HarmonyLib;
using UnityEngine;

namespace FullingPorter.Patches;

[HarmonyPatch(typeof(Player), "Update")]
internal static class ContainerInteractionPatch
{
    private static void Postfix(Player __instance)
    {
        if (__instance != Player.m_localPlayer || Plugin.SourceChestKey == null ||
            !Input.GetKeyDown(Plugin.SourceChestKey.Value))
        {
            return;
        }

        var hover = __instance.GetHoverObject();
        var container = hover ? hover.GetComponentInParent<Container>() : null;
        if (container == null)
        {
            return;
        }

        var enabled = SourceContainerMarker.Toggle(container);
        __instance.Message(MessageHud.MessageType.Center,
            enabled ? "$fullingporter_source_enabled" : "$fullingporter_source_disabled");
    }
}

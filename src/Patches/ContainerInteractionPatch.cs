using FullingPorter.Storage;
using HarmonyLib;
using UnityEngine;

namespace FullingPorter.Patches;

[HarmonyPatch(typeof(Container), nameof(Container.Interact))]
internal static class ContainerInteractionPatch
{
    private static bool Prefix(Container __instance, Humanoid character, bool hold, bool alt, ref bool __result)
    {
        if (hold || !alt || character != Player.m_localPlayer) return true;

        var enabled = SourceContainerMarker.Toggle(__instance);
        character.Message(MessageHud.MessageType.Center,
            enabled ? "$fullingporter_source_enabled" : "$fullingporter_source_disabled");
        __result = true;
        return false;
    }
}

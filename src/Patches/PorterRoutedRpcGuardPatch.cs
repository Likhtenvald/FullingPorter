using FullingPorter.Core;
using HarmonyLib;

namespace FullingPorter.Patches;

/// <summary>
/// Authenticates the sender of FullingPorter's routed RPC before Jötunn sees it.
/// Vanilla routed packets carry a client-written sender ID, so remote action
/// validation must bind that claimed ID to the actual ZRpc connection first.
/// </summary>
[HarmonyPatch(typeof(ZRoutedRpc), nameof(ZRoutedRpc.RPC_RoutedRPC))]
internal static class PorterRoutedRpcGuardPatch
{
    private static readonly int PorterActionsHash =
        (Plugin.PluginGuid + "!PorterActions").GetStableHashCode();

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool Prefix(ZRpc rpc, ZPackage pkg)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() || rpc == null || pkg == null)
            return true;

        var startPos = pkg.GetPos();

        try
        {
            pkg.ReadLong(); // message ID
            var claimedSender = pkg.ReadLong();
            pkg.ReadLong(); // target peer
            pkg.ReadZDOID(); // target ZDO
            var methodHash = pkg.ReadInt();

            if (methodHash != PorterActionsHash)
            {
                pkg.SetPos(startPos);
                return true;
            }

            var peer = ZNet.instance.GetPeer(rpc);
            if (peer == null || !peer.IsReady())
            {
                Plugin.Log.LogWarning("Dropped FullingPorter RPC from unresolved or unready connection.");
                return false;
            }

            if (!PorterServerRequestRules.IsAuthenticatedSender(claimedSender, peer.m_uid))
            {
                Plugin.Log.LogWarning(
                    $"Dropped forged FullingPorter RPC: claimed peer {claimedSender}, actual peer {peer.m_uid}.");
                return false;
            }

            pkg.SetPos(startPos);
            return true;
        }
        catch
        {
            pkg.SetPos(startPos);
            return true;
        }
    }
}

using System.Collections;
using FullingPorter.Contract;
using FullingPorter.Porter;
using FullingPorter.Storage;
using Jotunn.Entities;
using Jotunn.Managers;
using HarmonyLib;
using UnityEngine;

namespace FullingPorter.Network;

internal static class PorterRpc
{
    private enum ActionCode
    {
        Spawn = 1,
        ToggleSource = 2,
        Dismiss = 3,
        Rename = 4
    }

    private static CustomRPC _rpc;
    private static bool _spawnPending;
    private static Inventory _pendingContractInventory;
    private static ItemDrop.ItemData _pendingContractItem;
    private static readonly System.Reflection.MethodInfo GetServerPeerIdMethod =
        AccessTools.Method(typeof(ZRoutedRpc), "GetServerPeerID");
    private static readonly System.Reflection.FieldInfo RoutedRpcIdField =
        AccessTools.Field(typeof(ZRoutedRpc), "m_id");

    internal static void Register()
    {
        _rpc = NetworkManager.Instance.AddRPC("PorterActions", ServerReceive, ClientReceive);
    }

    internal static void RequestSpawn(
        Vector3 position,
        Vector3 forward,
        Inventory inventory,
        ItemDrop.ItemData contractItem)
    {
        if (_spawnPending || inventory == null || contractItem == null)
            return;

        _pendingContractInventory = inventory;
        _pendingContractItem = contractItem;

        if (ZNet.instance != null && ZNet.instance.IsServer())
        {
            var success = TrySpawnServer(position, forward);
            CompleteLocalSpawn(success);
            return;
        }

        if (_rpc == null || ZRoutedRpc.instance == null)
        {
            ClearPendingSpawn();
            return;
        }

        var serverPeerId = GetServerPeerId();
        if (serverPeerId == 0L)
        {
            Plugin.Log.LogWarning("Porter spawn request skipped because the server peer ID could not be resolved.");
            ClearPendingSpawn();
            return;
        }

        _spawnPending = true;
        var pkg = new ZPackage();
        pkg.Write((int)ActionCode.Spawn);
        pkg.Write(position);
        pkg.Write(forward);
        _rpc.SendPackage(serverPeerId, pkg);
    }

    internal static void RequestToggleSource(Container container)
    {
        var view = container ? container.GetComponent<ZNetView>() : null;
        var zdo = view != null && view.IsValid() ? view.GetZDO() : null;
        if (_rpc == null || ZRoutedRpc.instance == null || zdo == null)
            return;

        if (ZNet.instance != null && ZNet.instance.IsServer())
        {
            if (SourceContainerMarker.TryToggleServer(container, out var enabled))
                Player.m_localPlayer?.Message(MessageHud.MessageType.Center,
                    enabled ? "$fullingporter_source_enabled" : "$fullingporter_source_disabled");
            return;
        }

        var serverPeerId = GetServerPeerId();
        if (serverPeerId == 0L) return;

        var pkg = new ZPackage();
        pkg.Write((int)ActionCode.ToggleSource);
        pkg.Write(zdo.m_uid);
        _rpc.SendPackage(serverPeerId, pkg);
    }

    internal static void RequestDismiss(ZNetView view)
    {
        var zdo = view != null && view.IsValid() ? view.GetZDO() : null;
        if (_rpc == null || ZRoutedRpc.instance == null || zdo == null)
            return;

        if (ZNet.instance != null && ZNet.instance.IsServer())
        {
            if (TryDismissServer(view.gameObject))
                Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "$fullingporter_dismissed");
            return;
        }

        var serverPeerId = GetServerPeerId();
        if (serverPeerId == 0L) return;

        var pkg = new ZPackage();
        pkg.Write((int)ActionCode.Dismiss);
        pkg.Write(zdo.m_uid);
        _rpc.SendPackage(serverPeerId, pkg);
    }

    internal static void RequestRename(ZNetView view, string name)
    {
        var zdo = view != null && view.IsValid() ? view.GetZDO() : null;
        if (_rpc == null || ZRoutedRpc.instance == null || zdo == null)
            return;

        if (ZNet.instance != null && ZNet.instance.IsServer())
        {
            var state = view.GetComponent<PorterState>();
            state?.SetNameServer(name);
            return;
        }

        var serverPeerId = GetServerPeerId();
        if (serverPeerId == 0L) return;

        var pkg = new ZPackage();
        pkg.Write((int)ActionCode.Rename);
        pkg.Write(zdo.m_uid);
        pkg.Write(name ?? string.Empty);
        _rpc.SendPackage(serverPeerId, pkg);
    }

    private static long GetServerPeerId()
    {
        var routed = ZRoutedRpc.instance;
        if (routed == null)
            return 0L;

        try
        {
            if (GetServerPeerIdMethod != null)
            {
                var value = GetServerPeerIdMethod.Invoke(routed, null);
                if (value is long peerId && peerId != 0L)
                    return peerId;
            }

            if (ZNet.instance != null && ZNet.instance.IsServer() && RoutedRpcIdField != null)
            {
                var value = RoutedRpcIdField.GetValue(routed);
                if (value is long localPeerId && localPeerId != 0L)
                    return localPeerId;
            }
        }
        catch (System.Exception ex)
        {
            Plugin.Log.LogWarning("Could not resolve Valheim server peer ID: " + ex.GetBaseException().Message);
            return 0L;
        }

        Plugin.Log.LogWarning("Valheim server peer ID is unavailable on this build.");
        return 0L;
    }

    private static IEnumerator ServerReceive(long sender, ZPackage pkg)
    {
        var action = (ActionCode)pkg.ReadInt();

        switch (action)
        {
            case ActionCode.Spawn:
                HandleSpawn(sender, pkg);
                break;
            case ActionCode.ToggleSource:
                HandleToggleSource(sender, pkg);
                break;
            case ActionCode.Dismiss:
                yield return HandleDismiss(sender, pkg);
                yield break;
            case ActionCode.Rename:
                HandleRename(sender, pkg);
                break;
        }

        yield return null;
    }

    private static IEnumerator ClientReceive(long sender, ZPackage pkg)
    {
        var action = (ActionCode)pkg.ReadInt();
        var success = pkg.ReadBool();

        switch (action)
        {
            case ActionCode.Spawn:
                CompleteLocalSpawn(success);
                break;

            case ActionCode.ToggleSource:
                if (success)
                {
                    var enabled = pkg.ReadBool();
                    Player.m_localPlayer?.Message(MessageHud.MessageType.Center,
                        enabled ? "$fullingporter_source_enabled" : "$fullingporter_source_disabled");
                }
                break;

            case ActionCode.Dismiss:
                if (success)
                    Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "$fullingporter_dismissed");
                break;
        }

        yield return null;
    }

    private static void HandleSpawn(long sender, ZPackage pkg)
    {
        var position = pkg.ReadVector3();
        var forward = pkg.ReadVector3();
        SendResult(sender, ActionCode.Spawn, TrySpawnServer(position, forward));
    }

    private static bool TrySpawnServer(Vector3 position, Vector3 forward)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer())
            return false;

        if (PorterState.WorldHasPorter())
            return false;

        var prefab = ZNetScene.instance?.GetPrefab(PorterPrefabRegistry.PrefabName);
        if (prefab == null)
        {
            Plugin.Log.LogError("Porter prefab is not registered in ZNetScene.");
            return false;
        }

        forward.y = 0f;
        if (forward.sqrMagnitude < 0.01f)
            forward = Vector3.forward;

        var spawned = Object.Instantiate(prefab, position + forward.normalized * 2f,
            Quaternion.LookRotation(-forward.normalized));
        var view = spawned != null ? spawned.GetComponent<ZNetView>() : null;
        if (spawned == null || view == null)
            return false;

        PorterState.MarkWorldOccupied(view);
        return true;
    }

    private static void HandleToggleSource(long sender, ZPackage pkg)
    {
        var target = FindObject(pkg.ReadZDOID());
        var container = target != null ? target.GetComponent<Container>() : null;
        if (container == null || !SourceContainerMarker.TryToggleServer(container, out var enabled))
        {
            SendResult(sender, ActionCode.ToggleSource, false);
            return;
        }

        var response = NewResult(ActionCode.ToggleSource, true);
        response.Write(enabled);
        _rpc.SendPackage(sender, response);
    }

    private static IEnumerator HandleDismiss(long sender, ZPackage pkg)
    {
        var target = FindObject(pkg.ReadZDOID());
        var view = target != null ? target.GetComponent<ZNetView>() : null;
        var state = target != null ? target.GetComponent<PorterState>() : null;
        if (view == null || state == null || ZNetScene.instance == null)
        {
            SendResult(sender, ActionCode.Dismiss, false);
            yield break;
        }

        var deadline = Time.time + 3f;
        while (!view.IsOwner() && Time.time < deadline)
        {
            view.ClaimOwnership();
            yield return null;
        }

        if (!view.IsOwner())
        {
            SendResult(sender, ActionCode.Dismiss, false);
            yield break;
        }

        SendResult(sender, ActionCode.Dismiss, TryDismissServer(target));
    }

    private static bool TryDismissServer(GameObject target)
    {
        if (ZNet.instance == null || !ZNet.instance.IsServer() || target == null || ZNetScene.instance == null)
            return false;

        var state = target.GetComponent<PorterState>();
        if (state == null)
            return false;

        PorterState.ClearWorldOccupied();
        ZNetScene.instance.Destroy(target);
        return true;
    }

    private static void HandleRename(long sender, ZPackage pkg)
    {
        var target = FindObject(pkg.ReadZDOID());
        var state = target != null ? target.GetComponent<PorterState>() : null;
        if (state == null)
        {
            SendResult(sender, ActionCode.Rename, false);
            return;
        }

        var success = state.SetNameServer(pkg.ReadString());
        SendResult(sender, ActionCode.Rename, success);
    }

    private static GameObject FindObject(ZDOID id)
    {
        return ZNetScene.instance != null ? ZNetScene.instance.FindInstance(id) : null;
    }

    private static void SendResult(long target, ActionCode action, bool success)
    {
        _rpc.SendPackage(target, NewResult(action, success));
    }

    private static ZPackage NewResult(ActionCode action, bool success)
    {
        var pkg = new ZPackage();
        pkg.Write((int)action);
        pkg.Write(success);
        return pkg;
    }

    private static void CompleteLocalSpawn(bool success)
    {
        _spawnPending = false;

        if (success)
        {
            ConsumePendingContract();
            Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "$fullingporter_spawned");
        }
        else
        {
            Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "$fullingporter_already_exists");
        }

        ClearPendingSpawn();
    }

    private static void ConsumePendingContract()
    {
        var inventory = _pendingContractInventory ?? Player.m_localPlayer?.GetInventory();
        if (inventory == null)
            return;

        if (_pendingContractItem != null && inventory.ContainsItem(_pendingContractItem))
        {
            inventory.RemoveOneItem(_pendingContractItem);
            return;
        }

        var items = inventory.GetAllItems();
        if (items == null)
            return;

        foreach (var item in items)
        {
            if (item?.m_dropPrefab?.name != ContractRegistry.ItemName)
                continue;

            inventory.RemoveOneItem(item);
            return;
        }
    }

    private static void ClearPendingSpawn()
    {
        _spawnPending = false;
        _pendingContractInventory = null;
        _pendingContractItem = null;
    }
}

using System.Collections;
using FullingPorter.Contract;
using FullingPorter.Porter;
using FullingPorter.Storage;
using Jotunn.Entities;
using Jotunn.Managers;
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

    internal static void Register()
    {
        _rpc = NetworkManager.Instance.AddRPC("PorterActions", ServerReceive, ClientReceive);
    }

    internal static void RequestSpawn(Vector3 position, Vector3 forward)
    {
        if (_rpc == null || ZRoutedRpc.instance == null || _spawnPending)
            return;

        _spawnPending = true;
        var pkg = new ZPackage();
        pkg.Write((int)ActionCode.Spawn);
        pkg.Write(position);
        pkg.Write(forward);
        _rpc.SendPackage(ZRoutedRpc.instance.GetServerPeerID(), pkg);
    }

    internal static void RequestToggleSource(Container container)
    {
        var view = container ? container.GetComponent<ZNetView>() : null;
        var zdo = view != null && view.IsValid() ? view.GetZDO() : null;
        if (_rpc == null || ZRoutedRpc.instance == null || zdo == null)
            return;

        var pkg = new ZPackage();
        pkg.Write((int)ActionCode.ToggleSource);
        pkg.Write(zdo.m_uid);
        _rpc.SendPackage(ZRoutedRpc.instance.GetServerPeerID(), pkg);
    }

    internal static void RequestDismiss(ZNetView view)
    {
        var zdo = view != null && view.IsValid() ? view.GetZDO() : null;
        if (_rpc == null || ZRoutedRpc.instance == null || zdo == null)
            return;

        var pkg = new ZPackage();
        pkg.Write((int)ActionCode.Dismiss);
        pkg.Write(zdo.m_uid);
        _rpc.SendPackage(ZRoutedRpc.instance.GetServerPeerID(), pkg);
    }

    internal static void RequestRename(ZNetView view, string name)
    {
        var zdo = view != null && view.IsValid() ? view.GetZDO() : null;
        if (_rpc == null || ZRoutedRpc.instance == null || zdo == null)
            return;

        var pkg = new ZPackage();
        pkg.Write((int)ActionCode.Rename);
        pkg.Write(zdo.m_uid);
        pkg.Write(name ?? string.Empty);
        _rpc.SendPackage(ZRoutedRpc.instance.GetServerPeerID(), pkg);
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
                _spawnPending = false;
                if (success)
                {
                    ConsumeLocalContract();
                    Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "$fullingporter_spawned");
                }
                else
                {
                    Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "$fullingporter_already_exists");
                }
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

        if (PorterState.WorldHasPorter())
        {
            SendResult(sender, ActionCode.Spawn, false);
            return;
        }

        var prefab = ZNetScene.instance?.GetPrefab(PorterPrefabRegistry.PrefabName);
        if (prefab == null)
        {
            Plugin.Log.LogError("Porter prefab is not registered in ZNetScene.");
            SendResult(sender, ActionCode.Spawn, false);
            return;
        }

        forward.y = 0f;
        if (forward.sqrMagnitude < 0.01f)
            forward = Vector3.forward;

        var spawned = Object.Instantiate(prefab, position + forward.normalized * 2f,
            Quaternion.LookRotation(-forward.normalized));
        var view = spawned != null ? spawned.GetComponent<ZNetView>() : null;
        if (spawned == null || view == null)
        {
            SendResult(sender, ActionCode.Spawn, false);
            return;
        }

        PorterState.MarkWorldOccupied(view);
        SendResult(sender, ActionCode.Spawn, true);
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

        PorterState.ClearWorldOccupied();
        ZNetScene.instance.Destroy(target);
        SendResult(sender, ActionCode.Dismiss, true);
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

    private static void ConsumeLocalContract()
    {
        var inventory = Player.m_localPlayer?.GetInventory();
        var items = inventory?.GetAllItems();
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
}

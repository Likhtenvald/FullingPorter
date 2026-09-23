using System.Collections.Generic;
using UnityEngine;

namespace FullingPorter.Storage;

/// <summary>
/// Centralizes item mutation. Every move is revalidated immediately before
/// removal so porter AI never performs ad-hoc container mutations.
/// </summary>
internal static class TransferService
{
    internal enum TransferResult
    {
        Success,
        WaitingForOwnership,
        SourceUnavailable,
        DestinationRejected,
        DestinationFull,
        ContainerBusy,
        Failed
    }

    internal static TransferResult TryMoveWholeStack(Container source, Container destination, ItemDrop.ItemData item)
    {
        if (!source || !destination || item == null || source == destination) return TransferResult.Failed;
        if (ZNet.instance == null || !ZNet.instance.IsServer()) return TransferResult.Failed;

        if (ContainerUseGuard.IsPlayerBusy(source) || ContainerUseGuard.IsPlayerBusy(destination))
            return TransferResult.ContainerBusy;

        var sourceView = source.GetComponent<ZNetView>();
        var destinationView = destination.GetComponent<ZNetView>();
        if (sourceView == null || destinationView == null || !sourceView.IsValid() || !destinationView.IsValid())
            return TransferResult.Failed;

        var sourceNewLock = false;
        var destinationNewLock = false;

        if (!ContainerUseGuard.TryAcquire(source, out sourceNewLock))
            return TransferResult.ContainerBusy;

        if (!ContainerUseGuard.TryAcquire(destination, out destinationNewLock))
        {
            ContainerUseGuard.Release(source);
            return TransferResult.ContainerBusy;
        }

        // First pass only publishes the porter lock. Inventory mutation is
        // deliberately delayed until a later tick so clients can observe the
        // lock before the server can claim ownership and modify either chest.
        if (sourceNewLock || destinationNewLock)
            return TransferResult.WaitingForOwnership;

        try
        {
            if (ContainerUseGuard.IsPlayerBusy(source) || ContainerUseGuard.IsPlayerBusy(destination))
                return TransferResult.ContainerBusy;

            if (!sourceView.IsOwner()) sourceView.ClaimOwnership();
            if (!destinationView.IsOwner()) destinationView.ClaimOwnership();
            if (!sourceView.IsOwner() || !destinationView.IsOwner())
                return TransferResult.WaitingForOwnership;

            if (ContainerUseGuard.IsPlayerBusy(source) || ContainerUseGuard.IsPlayerBusy(destination))
                return TransferResult.ContainerBusy;

            var sourceInventory = source.GetInventory();
            var destinationInventory = destination.GetInventory();
            if (sourceInventory == null || destinationInventory == null) return TransferResult.Failed;
            if (!sourceInventory.ContainsItem(item)) return TransferResult.SourceUnavailable;
            if (!QuickStackPlusBridge.Accepts(destination, item)) return TransferResult.DestinationRejected;
            if (!destinationInventory.CanAddItem(item, -1)) return TransferResult.DestinationFull;

            // Never pass the source ItemData instance directly into another
            // inventory. Valheim may partially merge the supplied object even when
            // AddItem ultimately returns false. Work on a clone and rollback the
            // destination if the operation does not fully succeed.
            using (var destinationSnapshot = new InventorySnapshot(destinationInventory))
            {
                var moved = item.Clone();
                moved.m_stack = item.m_stack;
                moved.m_equipped = false;

                if (!destinationInventory.AddItem(moved))
                    return TransferResult.Failed;

                // The source item has not been mutated. Remove it only after the
                // destination accepted the complete cloned stack. If removal fails,
                // disposing the snapshot rolls the destination back as well.
                if (!sourceInventory.RemoveItem(item))
                    return TransferResult.SourceUnavailable;

                destinationSnapshot.Commit();
                return TransferResult.Success;
            }
        }
        finally
        {
            ContainerUseGuard.Release(destination);
            ContainerUseGuard.Release(source);
        }
    }

    /// <summary>
    /// Restores an inventory if a tentative AddItem operation fails, using only
    /// public Inventory APIs available in the current Valheim assembly.
    /// </summary>
    private sealed class InventorySnapshot : System.IDisposable
    {
        private readonly Inventory _inventory;
        private readonly List<ItemDrop.ItemData> _items;
        private bool _committed;

        internal InventorySnapshot(Inventory inventory)
        {
            _inventory = inventory;
            _items = new List<ItemDrop.ItemData>();

            foreach (var item in inventory.GetAllItems())
            {
                if (item == null)
                    continue;

                var clone = item.Clone();
                clone.m_stack = item.m_stack;
                clone.m_gridPos = item.m_gridPos;
                _items.Add(clone);
            }
        }

        internal void Commit() => _committed = true;

        public void Dispose()
        {
            if (_committed)
                return;

            var currentItems = new List<ItemDrop.ItemData>(_inventory.GetAllItems());
            foreach (var item in currentItems)
                _inventory.RemoveItem(item);

            foreach (var item in _items)
            {
                var clone = item.Clone();
                clone.m_stack = item.m_stack;
                clone.m_gridPos = item.m_gridPos;
                _inventory.AddItem(clone);
            }
        }
    }
}

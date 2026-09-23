using UnityEngine;

namespace FullingPorter.Storage;

/// <summary>
/// Centralizes item mutation. Every move is revalidated immediately before
/// removal so porter AI never performs ad-hoc container mutations.
/// </summary>
internal static class TransferService
{
    internal static bool TryMoveWholeStack(Container source, Container destination, ItemDrop.ItemData item)
    {
        if (!source || !destination || item == null || source == destination) return false;
        if (ZNet.instance == null || !ZNet.instance.IsServer()) return false;

        var sourceView = source.GetComponent<ZNetView>();
        var destinationView = destination.GetComponent<ZNetView>();
        if (sourceView == null || destinationView == null || !sourceView.IsValid() || !destinationView.IsValid())
            return false;

        if (!sourceView.IsOwner()) sourceView.ClaimOwnership();
        if (!destinationView.IsOwner()) destinationView.ClaimOwnership();
        if (!sourceView.IsOwner() || !destinationView.IsOwner())
            return false;

        var sourceInventory = source.GetInventory();
        var destinationInventory = destination.GetInventory();
        if (sourceInventory == null || destinationInventory == null) return false;
        if (!sourceInventory.ContainsItem(item)) return false;
        if (!QuickStackPlusBridge.Accepts(destination, item)) return false;
        if (!destinationInventory.CanAddItem(item, -1)) return false;

        // Inventory.AddItem(ItemData) clones/moves the stack into the destination.
        // Remove only after AddItem reports success.
        if (!destinationInventory.AddItem(item)) return false;
        sourceInventory.RemoveItem(item);
        return true;
    }
}

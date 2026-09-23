using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;

namespace FullingPorter.Contract;

internal static class ContractRegistry
{
    internal const string ItemName = "FullingPorterContract";

    internal static void Register()
    {
        // Uses a vanilla paper-like item as an MVP base. A dedicated icon/asset
        // can be added without changing the contract/spawn architecture.
        var item = new CustomItem(ItemName, "YagluthDrop", new ItemConfig
        {
            Name = "$fullingporter_contract",
            Description = "$fullingporter_contract_desc",
            StackSize = 1,
            Weight = 0.1f
        });
        ItemManager.Instance.AddItem(item);

        // Haldor trade registration is kept separate because trader APIs changed
        // across Valheim/Jotunn versions. It will be wired after compile validation
        // against the current game assemblies.
    }
}

using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;

namespace FullingPorter.Contract;

internal static class ContractRegistry
{
    internal const string ItemName = "FullingPorterContract";
    internal static CustomItem Contract { get; private set; }

    internal static void Register()
    {
        Contract = new CustomItem(ItemName, "YagluthDrop", new ItemConfig
        {
            Name = "$fullingporter_contract",
            Description = "$fullingporter_contract_desc",
            StackSize = 1,
            Weight = 0.1f
        });
        ItemManager.Instance.AddItem(Contract);
    }
}

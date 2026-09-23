using Jotunn.Managers;

namespace FullingPorter.Localization;

internal static class LocalizationRegistry
{
    internal static void Register()
    {
        LocalizationManager.Instance.AddTranslation("English", new()
        {
            ["fullingporter_name"] = "Fuling Porter",
            ["fullingporter_interact"] = "Talk",
            ["fullingporter_contract"] = "Fuling Porter Contract",
            ["fullingporter_contract_desc"] = "A contract for the services of a surprisingly disciplined Fuling porter."
        });
        LocalizationManager.Instance.AddTranslation("Russian", new()
        {
            ["fullingporter_name"] = "Фулинг-грузчик",
            ["fullingporter_interact"] = "Поговорить",
            ["fullingporter_contract"] = "Контракт фулинга-грузчика",
            ["fullingporter_contract_desc"] = "Контракт на услуги на удивление дисциплинированного фулинга-грузчика."
        });
    }
}

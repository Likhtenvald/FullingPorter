using Jotunn.Configs;
using Jotunn.Managers;

namespace FullingPorter.Localization;

internal static class LocalizationRegistry
{
    internal static void Register()
    {
        var localization = new CustomLocalization();

        localization.AddTranslation("English", new()
        {
            ["fullingporter_name"] = "Fuling Porter",
            ["fullingporter_interact"] = "Talk",
            ["fullingporter_contract"] = "Fuling Porter Contract",
            ["fullingporter_contract_desc"] = "A contract for the services of a surprisingly disciplined Fuling porter.",
            ["fullingporter_source_enabled"] = "Porter source chest assigned",
            ["fullingporter_source_disabled"] = "Porter source chest unassigned",
            ["fullingporter_spawned"] = "Your Fuling porter has arrived",
            ["fullingporter_rename_title"] = "Name your porter"
        });

        localization.AddTranslation("Russian", new()
        {
            ["fullingporter_name"] = "Фулинг-грузчик",
            ["fullingporter_interact"] = "Поговорить",
            ["fullingporter_contract"] = "Контракт фулинга-грузчика",
            ["fullingporter_contract_desc"] = "Контракт на услуги на удивление дисциплинированного фулинга-грузчика.",
            ["fullingporter_source_enabled"] = "Сундук назначен источником грузчика",
            ["fullingporter_source_disabled"] = "Сундук больше не является источником грузчика",
            ["fullingporter_spawned"] = "Ваш фулинг-грузчик прибыл",
            ["fullingporter_rename_title"] = "Имя грузчика"
        });

        LocalizationManager.Instance.AddLocalization(localization);
    }
}

using System.Collections.Generic;
using System.Reflection;

namespace FullingPorter.Localization;

internal static class LocalizationRegistry
{
    private static readonly Dictionary<string, string> English = new()
    {
        ["fullingporter_name"] = "Fuling Porter",
        ["fullingporter_interact"] = "Talk",
        ["fullingporter_contract"] = "Fuling Porter Contract",
        ["fullingporter_contract_desc"] = "A contract for the services of a surprisingly disciplined Fuling porter.",
        ["fullingporter_source_enabled"] = "Porter source chest assigned",
        ["fullingporter_source_disabled"] = "Porter source chest unassigned",
        ["fullingporter_spawned"] = "Your Fuling porter has arrived",
        ["fullingporter_rename_title"] = "Name your porter"
    };

    private static readonly Dictionary<string, string> Russian = new()
    {
        ["fullingporter_name"] = "Фулинг-грузчик",
        ["fullingporter_interact"] = "Поговорить",
        ["fullingporter_contract"] = "Контракт фулинга-грузчика",
        ["fullingporter_contract_desc"] = "Контракт на услуги на удивление дисциплинированного фулинга-грузчика.",
        ["fullingporter_source_enabled"] = "Сундук назначен источником грузчика",
        ["fullingporter_source_disabled"] = "Сундук больше не является источником грузчика",
        ["fullingporter_spawned"] = "Ваш фулинг-грузчик прибыл",
        ["fullingporter_rename_title"] = "Имя грузчика"
    };

    internal static void Register()
    {
        // Localization APIs have changed between Jotunn releases. Register through
        // Valheim's Localization.AddWord at runtime to keep this build compatible.
        var localization = global::Localization.instance;
        if (localization == null) return;
        var addWord = typeof(global::Localization).GetMethod("AddWord", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (addWord == null) return;

        var language = localization.GetSelectedLanguage();
        var words = language == "Russian" ? Russian : English;
        foreach (var pair in words) addWord.Invoke(localization, new object[] { pair.Key, pair.Value });
    }
}

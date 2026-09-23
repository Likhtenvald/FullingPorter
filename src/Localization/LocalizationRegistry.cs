using Jotunn.Managers;
using System.Collections.Generic;

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
        ["fullingporter_already_exists"] = "Only one porter can be contracted in this world",
        ["fullingporter_rename_title"] = "Name your porter",
        ["fullingporter_dismissed"] = "Porter contract terminated",
        ["fullingporter_dismiss_confirm"] = "Press the dismissal key again within 3 seconds to terminate this contract"
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
        ["fullingporter_already_exists"] = "В этом мире уже есть фулинг-грузчик",
        ["fullingporter_rename_title"] = "Имя грузчика",
        ["fullingporter_dismissed"] = "Контракт с грузчиком расторгнут",
        ["fullingporter_dismiss_confirm"] = "Нажмите клавишу увольнения ещё раз в течение 3 секунд, чтобы расторгнуть контракт"
    };

    internal static void Register()
    {
        LocalizationManager.Instance.AddLocalization("English", English);
        LocalizationManager.Instance.AddLocalization("Russian", Russian);
        Plugin.Log.LogInfo("FullingPorter English and Russian localization registered.");
    }
}

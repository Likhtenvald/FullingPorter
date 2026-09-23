using Jotunn.Managers;
using System.Collections.Generic;

namespace FullingPorter.Localization;

internal static class LocalizationRegistry
{
    private static readonly Dictionary<string, string> English = new()
    {
        ["fullingporter_name"] = "Fuling Porter",
        ["fullingporter_contract"] = "Fuling Porter Contract",
        ["fullingporter_contract_desc"] = "Hire one friendly Fuling porter to move source-chest items into QuickStackPlus Smart Storage.",
        ["fullingporter_source_enabled"] = "Porter source chest assigned",
        ["fullingporter_source_disabled"] = "Porter source chest unassigned",
        ["fullingporter_source_hover"] = "Porter source",
        ["fullingporter_status_idle"] = "Waiting for work",
        ["fullingporter_status_collecting"] = "Going to source",
        ["fullingporter_status_delivering"] = "Delivering",
        ["fullingporter_status_returning"] = "Returning home",
        ["fullingporter_spawned"] = "Your Fuling porter has arrived",
        ["fullingporter_already_exists"] = "Only one porter can be contracted in this world",
        ["fullingporter_rename_title"] = "Name your porter",
        ["fullingporter_dismissed"] = "Porter contract terminated",
        ["fullingporter_dismiss_confirm"] = "Press the dismissal key again within 3 seconds to terminate this contract",
        ["fullingporter_talk_idle_01"] = "Finally, no work!",
        ["fullingporter_talk_idle_02"] = "Porter carries. Viking watches.",
        ["fullingporter_talk_idle_03"] = "Maybe today we move nothing? No? Fine.",
        ["fullingporter_talk_idle_04"] = "Coins to Haldor. Work to me. Very fair.",
        ["fullingporter_talk_idle_05"] = "Fuling smart. Viking... builds more chests.",
        ["fullingporter_talk_idle_06"] = "I have seen berserkers carry less.",
        ["fullingporter_talk_idle_07"] = "Work exists. Vacation does not.",
        ["fullingporter_talk_work_01"] = "Work again!?",
        ["fullingporter_talk_work_02"] = "Ten stacks! I have two hands!",
        ["fullingporter_talk_work_03"] = "I am porter, not portal!",
        ["fullingporter_talk_work_04"] = "More wood? Did you finish the forest?",
        ["fullingporter_talk_work_05"] = "You put it in chest. I put it in another chest. Civilization!",
        ["fullingporter_talk_work_06"] = "Walk there. Carry this. Walk back. Great career.",
        ["fullingporter_talk_work_07"] = "Next contract, I negotiate breaks.",
        ["fullingporter_talk_return_01"] = "Done. Now coin?",
        ["fullingporter_talk_return_02"] = "Back to home. Feet complain.",
        ["fullingporter_talk_return_03"] = "Cargo delivered. Glory minimal.",
        ["fullingporter_talk_return_04"] = "I survived another chest.",
        ["fullingporter_talk_return_05"] = "Finished! Pretending to be busy now.",
        ["fullingporter_talk_blocked_01"] = "Chest full! I am not magic!",
        ["fullingporter_talk_blocked_02"] = "Where put it!? On your head?",
        ["fullingporter_talk_blocked_03"] = "No room. Build chest. Big chest.",
        ["fullingporter_talk_blocked_04"] = "If storage is full, that is not porter problem!",
        ["fullingporter_talk_blocked_05"] = "Smart Storage very smart. Still full.",
        ["fullingporter_talk_blocked_06"] = "I can carry it forever. You pay by hour?"
    };

    private static readonly Dictionary<string, string> Russian = new()
    {
        ["fullingporter_name"] = "Фулинг-грузчик",
        ["fullingporter_contract"] = "Контракт фулинга-грузчика",
        ["fullingporter_contract_desc"] = "Наймите одного дружелюбного фулинга-грузчика для переноса вещей из источников в QuickStackPlus Smart Storage.",
        ["fullingporter_source_enabled"] = "Сундук назначен источником грузчика",
        ["fullingporter_source_disabled"] = "Сундук больше не является источником грузчика",
        ["fullingporter_source_hover"] = "Источник грузчика",
        ["fullingporter_status_idle"] = "Ожидает работу",
        ["fullingporter_status_collecting"] = "Идёт к источнику",
        ["fullingporter_status_delivering"] = "Разносит груз",
        ["fullingporter_status_returning"] = "Возвращается домой",
        ["fullingporter_spawned"] = "Ваш фулинг-грузчик прибыл",
        ["fullingporter_already_exists"] = "В этом мире уже есть фулинг-грузчик",
        ["fullingporter_rename_title"] = "Имя грузчика",
        ["fullingporter_dismissed"] = "Контракт с грузчиком расторгнут",
        ["fullingporter_dismiss_confirm"] = "Нажмите клавишу увольнения ещё раз в течение 3 секунд, чтобы расторгнуть контракт",
        ["fullingporter_talk_idle_01"] = "Наконец-то нет работы!",
        ["fullingporter_talk_idle_02"] = "Грузчик носит. Викинг смотрит.",
        ["fullingporter_talk_idle_03"] = "Может сегодня ничего не переносить? Нет? Ладно.",
        ["fullingporter_talk_idle_04"] = "Монеты Хальдору. Работа мне. Очень честно.",
        ["fullingporter_talk_idle_05"] = "Фулинг умный. Викинг... строит ещё сундуки.",
        ["fullingporter_talk_idle_06"] = "Я видел берсерков. Они меньше таскают.",
        ["fullingporter_talk_idle_07"] = "Работа есть. Отпуска нет.",
        ["fullingporter_talk_work_01"] = "Опять работать!?",
        ["fullingporter_talk_work_02"] = "Десять стопок! У меня две руки!",
        ["fullingporter_talk_work_03"] = "Я грузчик, не портал!",
        ["fullingporter_talk_work_04"] = "Ещё дерево? У вас лес закончился?",
        ["fullingporter_talk_work_05"] = "Ты кладёшь в сундук. Я кладу в другой. Цивилизация!",
        ["fullingporter_talk_work_06"] = "Иди туда. Неси это. Иди назад. Отличная карьера.",
        ["fullingporter_talk_work_07"] = "В следующем контракте выбью себе перерывы.",
        ["fullingporter_talk_return_01"] = "Готово. Теперь монета?",
        ["fullingporter_talk_return_02"] = "Домой. Ноги жалуются.",
        ["fullingporter_talk_return_03"] = "Груз доставлен. Славы мало.",
        ["fullingporter_talk_return_04"] = "Я пережил ещё один сундук.",
        ["fullingporter_talk_return_05"] = "Закончил! Теперь делаю вид, что занят.",
        ["fullingporter_talk_blocked_01"] = "Сундук полный! Я не маг!",
        ["fullingporter_talk_blocked_02"] = "Куда класть!? На голову тебе?",
        ["fullingporter_talk_blocked_03"] = "Места нет. Строй сундук. Большой сундук.",
        ["fullingporter_talk_blocked_04"] = "Если склад полный — это не проблема грузчика!",
        ["fullingporter_talk_blocked_05"] = "Smart Storage очень умный. Но всё равно полный.",
        ["fullingporter_talk_blocked_06"] = "Я могу носить это вечно. Оплата почасовая?"
    };

    internal static void Register()
    {
        LocalizationManager.Instance.AddLocalization("English", English);
        LocalizationManager.Instance.AddLocalization("Russian", Russian);
        Plugin.Log.LogInfo("FullingPorter English and Russian localization registered.");
    }
}

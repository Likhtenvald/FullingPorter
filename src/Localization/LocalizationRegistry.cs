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
        ["fullingporter_status_blocked"] = "Waiting for storage space",
        ["fullingporter_spawned"] = "Your Fuling porter has arrived",
        ["fullingporter_already_exists"] = "Only one porter can be contracted in this world",
        ["fullingporter_rename_title"] = "Name your porter",
        ["fullingporter_dismissed"] = "Porter contract terminated",
        ["fullingporter_dismiss_confirm"] = "Press the dismissal key again within 3 seconds to terminate this contract",
        ["fullingporter_talk_idle_01"] = "Finally, no work!",
        ["fullingporter_talk_idle_02"] = "Porter carries. Viking watches.",
        ["fullingporter_talk_idle_03"] = "Coins to Haldor. Work to me. Very fair.",
        ["fullingporter_talk_idle_04"] = "I have seen berserkers carry less.",
        ["fullingporter_talk_idle_05"] = "Work exists. Vacation does not.",
        ["fullingporter_talk_idle_06"] = "That\'s how it goes, doggy.",
        ["fullingporter_talk_work_01"] = "Work again!?",
        ["fullingporter_talk_work_02"] = "Ten stacks! I have two hands!",
        ["fullingporter_talk_work_03"] = "I am porter, not portal!",
        ["fullingporter_talk_work_04"] = "Help me...",
        ["fullingporter_talk_work_05"] = "More wood? Did the forest happen to run out?",
        ["fullingporter_talk_work_06"] = "You put it in chest. I put it in another.",
        ["fullingporter_talk_work_07"] = "Walk there. Carry this. Walk back...",
        ["fullingporter_talk_return_01"] = "Done!",
        ["fullingporter_talk_return_02"] = "Cargo delivered. Please leave five stars.",
        ["fullingporter_talk_return_03"] = "I survived another chest.",
        ["fullingporter_talk_idle_040"] = "Finished! Pretending to be busy now.",
        ["fullingporter_talk_blocked_01"] = "Chest full! I am not magic!",
        ["fullingporter_talk_blocked_02"] = "Where put it!? On your head?",
        ["fullingporter_talk_blocked_03"] = "No room. Build chest. Big chest.",
        ["fullingporter_talk_idle_041"] = "What do you want? Can\'t you see? Five-minute break!"
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
        ["fullingporter_status_blocked"] = "Ждёт освобождения склада",
        ["fullingporter_spawned"] = "Ваш фулинг-грузчик прибыл",
        ["fullingporter_already_exists"] = "В этом мире уже есть фулинг-грузчик",
        ["fullingporter_rename_title"] = "Имя грузчика",
        ["fullingporter_dismissed"] = "Контракт с грузчиком расторгнут",
        ["fullingporter_dismiss_confirm"] = "Нажмите клавишу увольнения ещё раз в течение 3 секунд, чтобы расторгнуть контракт",
        ["fullingporter_talk_idle_01"] = "Наконец-то нет работы!",
        ["fullingporter_talk_idle_02"] = "Грузчик носит. Викинг смотрит.",
        ["fullingporter_talk_idle_03"] = "Монеты Хальдору. Работа мне. Очень честно.",
        ["fullingporter_talk_idle_04"] = "Я видел берсерков. Они меньше таскают.",
        ["fullingporter_talk_idle_05"] = "Работа есть. Отпуска нет.",
        ["fullingporter_talk_idle_06"] = "Вот такие дела, собачка.",
        ["fullingporter_talk_work_01"] = "Опять работать!?",
        ["fullingporter_talk_work_02"] = "Десять стопок! У меня две руки!",
        ["fullingporter_talk_work_03"] = "Я грузчик, не портал!",
        ["fullingporter_talk_work_04"] = "Помогите...",
        ["fullingporter_talk_work_05"] = "Ещё дерево? У вас лес случайно не закончился?",
        ["fullingporter_talk_work_06"] = "Ты кладёшь в сундук. Я кладу в другой.",
        ["fullingporter_talk_work_07"] = "Иди туда. Неси это. Иди назад...",
        ["fullingporter_talk_return_01"] = "Готово!",
        ["fullingporter_talk_return_02"] = "Груз доставлен. Поставьте пять звезд.",
        ["fullingporter_talk_return_03"] = "Я пережил ещё один сундук.",
        ["fullingporter_talk_idle_040"] = "Закончил! Теперь делаю вид, что занят.",
        ["fullingporter_talk_blocked_01"] = "Сундук полный! Я не маг!",
        ["fullingporter_talk_blocked_02"] = "Куда класть!? На голову тебе?",
        ["fullingporter_talk_blocked_03"] = "Места нет. Строй сундук. Большой сундук.",
        ["fullingporter_talk_idle_041"] = "Чего тебе? Не видишь? Перерыв пять минут!"
    };

    internal static void Register()
    {
        var localization = LocalizationManager.Instance.GetLocalization();
        localization.AddTranslation("English", English);
        localization.AddTranslation("Russian", Russian);
        Plugin.Log.LogInfo("FullingPorter English and Russian localization registered.");
    }
}

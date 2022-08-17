using System.Threading.Tasks;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;
using YeelightAPI;

namespace LampLackey;

public static class KeyboardHelper
{
    public static async Task<InlineKeyboardMarkup> GetIndividualSwitchKeys(IEnumerable<Device> devices)
    {
        var keys = new List<InlineKeyboardButton>();
        var commandPrefix = "switch ";

        foreach (var item in Program.devicesCollection)
        {
            await item.Connect();
            keys.Add(InlineKeyboardButton.WithCallbackData(item.Name + item.GetPowerState(),
                                                           commandPrefix + item.Id));
        }
        return new InlineKeyboardMarkup(keys);
    }
}
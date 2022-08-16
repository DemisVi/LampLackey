using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
// using Telegram.Bot.Exceptions;
using YeelightAPI;
// using YeelightAPI.Models;

namespace LampLackey;

public static class UpdateHandlers
{
    public static Task PollingErrorHandler(ITelegramBotClient bot, Exception ex, CancellationToken cts)
    {
        System.Console.WriteLine($"Polling exception {ex}");
        return Task.CompletedTask;
    }

    public static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cts)
    {
        try
        {
            await (update.Type switch
            {
                UpdateType.Message => HandleMessageAsync(bot, update.Message!, cts),
                UpdateType.CallbackQuery => HandleCallbackQuery(bot, update.CallbackQuery!, cts),
                _ => Task.CompletedTask
            });
        }
        catch (Exception ex)
        {
            await PollingErrorHandler(bot, ex, cts);
        }
    }

    private static async Task HandleCallbackQuery(ITelegramBotClient bot, CallbackQuery callbackQuery, CancellationToken cts)
    {
        try
        {
            var dev = Program.devicesCollection.Where(x => x.Id == callbackQuery.Data).ElementAt(0);
            await bot.AnswerCallbackQueryAsync(callbackQueryId: callbackQuery.Id,
                                               text: $"switching {dev.Name}");
                                               /* text: $"{callbackQuery.Message.MessageId}"); */
            if (!dev.IsConnected)
            {
                await dev.Connect();
            }
            await dev.Toggle();

            await bot.EditMessageReplyMarkupAsync(chatId: callbackQuery.Message!.Chat.Id,
                                                  messageId: callbackQuery.Message.MessageId,
                                                  await KeyboardHelper.GetIndividualSwitchKeys(Program.devicesCollection));
        }
        catch (Exception ex)
        {
            await PollingErrorHandler(bot, ex, cts);
        }
    }

    private static async Task HandleMessageAsync(ITelegramBotClient bot, Message msg, CancellationToken cts)
    {
        try
        {
            await (msg switch
            {
                // { Text: "/start" } => Usage(),
                { Text: "/list" } => LocateAndListAsync(bot, msg), //replace with "scan", split into 2 methods
                { Text: "/switch" or "/start" } => HandleSwitchOneAsync(bot, msg),
/*                 { Text: "/cancel" } => CancelActionAsync(bot, msg), */
                { Text: "/q" } when msg.Chat.Id == long.Parse(Configuration.config["tgadmin"]) =>
                    Task.Run(() => Program.Shut("admin shutdown request")),
                { Text: "/q" } when msg.Chat.Id != long.Parse(Configuration.config["tgadmin"]) =>
                    bot.SendTextMessageAsync(msg.Chat.Id, "Only admin can command me this"),
                _ => Task.CompletedTask
            });
        }
        catch (Exception ex)
        {
            await PollingErrorHandler(bot, ex, cts);
        }

        /* static async Task CancelActionAsync(ITelegramBotClient bot, Message msg)
        {
            await bot.SendTextMessageAsync(chatId: msg.Chat.Id,
                                           text: "Action was cancelled.",
                                           replyMarkup: new ReplyKeyboardRemove());
        } */

        static async Task HandleSwitchOneAsync(ITelegramBotClient bot, Message msg)
        {
            if (Program.devicesCollection != null && Program.devicesCollection.Any())
            {
                var keys = new List<InlineKeyboardButton>();

                foreach (var item in Program.devicesCollection)
                {
                    await item.Connect();
                    keys.Add(InlineKeyboardButton.WithCallbackData(item.Name + item.GetPowerState(),
                                                                   /* "switch " +  */item.Id));
                }

                await bot.SendTextMessageAsync(chatId: msg.Chat.Id,
                                               text: "Which Lamp do you want to turn on?",
                                               replyMarkup: await KeyboardHelper.GetIndividualSwitchKeys(
                                                Program.devicesCollection));
            }
            else
            {
                await bot.SendTextMessageAsync(msg.Chat.Id, $"No Devices located atm. Maybe '/list' would help?");
            }
        }

        static async Task LocateAndListAsync(ITelegramBotClient bot, Message msg) // TODO: split into two methods:
        {                                                                         // one for scan, one for display
            var stringBuilder = new StringBuilder();
            var format = "💡Id: {0}\n" +
                         "Name: {1}\n" +
                         "Model: {2}\n";

            Program.devicesCollection = await DeviceLocator.DiscoverAsync();

            foreach (var dev in Program.devicesCollection)
            {
                stringBuilder.AppendFormat(format, dev.Id, dev.Name, dev.Model);
            }

            if (Program.devicesCollection.Any())
            {
                await bot.SendTextMessageAsync(msg.Chat.Id, stringBuilder.ToString());
            }
            else
            {
                await bot.SendTextMessageAsync(msg.Chat.Id, "Discovered no Devices");
            }
        }
    }

    private static Task Usage()
    {
        throw new NotImplementedException();
    }
}

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
            string devId = callbackQuery.Data!.Split(' ')[1];

            await bot.AnswerCallbackQueryAsync(callbackQueryId: callbackQuery.Id,
                                               text: $"switching {Program.devicesCollection.GetNameById(devId)}");

            var command = callbackQuery.Data?.Split(' ')[0];
            var action = command switch
            {
                "switch" => ToggleLightAsync(bot, callbackQuery, devId),
                _ => Task.CompletedTask
            };

            await action;
        }
        catch (Exception ex)
        {
            await PollingErrorHandler(bot, ex, cts);
        }

        static async Task ToggleLightAsync(ITelegramBotClient bot, CallbackQuery callbackQuery, string devId)
        {
            var dev = Program.devicesCollection.Where(x => x.Id == devId).ElementAt(0);

            if (!dev.IsConnected)
            {
                await dev.Connect();
            }
            await dev.Toggle();

            await bot.EditMessageReplyMarkupAsync(chatId: callbackQuery.Message!.Chat.Id,
                                                  messageId: callbackQuery.Message.MessageId,
                                                  await KeyboardHelper.GetIndividualSwitchKeys(Program.devicesCollection));
        }
    }

    private static async Task HandleMessageAsync(ITelegramBotClient bot, Message msg, CancellationToken cts)
    {
        try
        {
            await (msg switch
            {
                { Text: "/start" } => Usage(bot, msg),
                { Text: "/list" } => LocateAndListAsync(bot, msg), //replace with "scan", split into 2 methods
                { Text: "/switch" } => SendSwitchKeyboardAsync(bot, msg),
                _ => Task.CompletedTask
            });
        }
        catch (Exception ex)
        {
            await PollingErrorHandler(bot, ex, cts);
        }

        static async Task SendSwitchKeyboardAsync(ITelegramBotClient bot, Message msg)
        {
            if (Program.devicesCollection != null && Program.devicesCollection.Any())
            {
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

        static async Task Usage(ITelegramBotClient bot, Message msg)
        {
            string usage = "list - List available Lights\n" +
                           "scan - Scan network for Lights\n" +
                           "switch - Switching menu\n";

            await bot.SendTextMessageAsync(chatId: msg.Chat.Id,
                                     text: usage);
        }
    }
}

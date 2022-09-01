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
using Telegram.Bot.Exceptions;
using YeelightAPI;
// using YeelightAPI.Models;

namespace LampLackey;

public class UpdateHandler
{
    private static IEnumerable<Device> devices = DeviceLocator.DiscoverAsync().Result;

    public static Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cts)
    {
        try
        {
            return (update.Type switch
            {
                UpdateType.Message => HandleCommandAsync(bot, update.Message!, cts),
                UpdateType.CallbackQuery => HandleCallbackQuery(bot, update.CallbackQuery!, cts),
                _ => Task.CompletedTask
            });
        }
        catch (Exception ex)
        {
            return ErrorHandler.PollingErrorHandler(bot, ex, cts);
        }
    }

    private static async Task HandleCallbackQuery(ITelegramBotClient bot, CallbackQuery callbackQuery, CancellationToken cts)
    {
        try
        {
            var callbackData = callbackQuery.Data!.Split(':');
            var devId = callbackData.Last();
            var command = callbackData.First();

            await bot.AnswerCallbackQueryAsync(callbackQueryId: callbackQuery.Id,
                                               text: $"switching {devices.GetNameById(devId)}");

            await (command switch
            {
                "switch" => ToggleLightAsync(bot, callbackQuery, devId),
                _ => Task.CompletedTask
            });
        }
        catch (Exception ex)
        {
            await ErrorHandler.PollingErrorHandler(bot, ex, cts);
        }

        static async Task ToggleLightAsync(ITelegramBotClient bot, CallbackQuery callbackQuery, string devId)
        {
            var dev = devices.Where(x => x.Id == devId).ElementAt(0);

            if (!dev.IsConnected)
            {
                await dev.Connect();
            }
            await dev.Toggle();

            try
            {
                await bot.EditMessageReplyMarkupAsync(chatId: callbackQuery.Message!.Chat.Id,
                                                      messageId: callbackQuery.Message.MessageId,
                                                      await KeyboardHelper.GetIndividualSwitchKeys(devices));
            }
            catch (ApiRequestException) { }
        }
    }

    private static Task HandleCommandAsync(ITelegramBotClient bot, Message msg, CancellationToken cts)
    {
        try
        {
            return (msg switch
            {
                { Text: "/start" or "/usage" } => Usage(bot, msg),
                { Text: "/list" } => ListAsync(bot, msg),
                { Text: "/scan" } => ScanAsync(bot, msg),
                { Text: "/switch" } => SendSwitchKeyboardAsync(bot, msg),
                _ => Task.CompletedTask
            });
        }
        catch (Exception ex)
        {
            return ErrorHandler.PollingErrorHandler(bot, ex, cts);
        }

        static async Task SendSwitchKeyboardAsync(ITelegramBotClient bot, Message msg)
        {
            if (devices is not null && devices.Any())
            {
                await bot.SendTextMessageAsync(chatId: msg.Chat.Id,
                                               text: "Which Lamp do you want to switch?",
                                               replyMarkup: await KeyboardHelper.GetIndividualSwitchKeys(
                                                   devices));
            }
            else
            {
                await bot.SendTextMessageAsync(msg.Chat.Id, $"No Devices located atm. Maybe '/list' would help?");
            }
        }

        static Task ListAsync(ITelegramBotClient bot, Message msg) // TODO: split into two methods:
        {                                                                         // one for scan, one for display
            if (devices.Any())
            {
                return bot.SendTextMessageAsync(msg.Chat.Id, Messages.GetDeviceListMessage(devices));
            }
            else
            {
                return bot.SendTextMessageAsync(msg.Chat.Id, "No Devices located. Try '/scan' first.");
            }
        }

        static async Task ScanAsync(ITelegramBotClient bot, Message msg) // TODO: split into two methods:
        {                                                                         // one for scan, one for display
            devices = await DeviceLocator.DiscoverAsync();

            await ListAsync(bot, msg);
        }

        static Task Usage(ITelegramBotClient bot, Message msg)
        {
            return bot.SendTextMessageAsync(chatId: msg.Chat.Id,
                                            text: Messages.GetUsageMessage());
        }
    }
}

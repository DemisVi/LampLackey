using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Exceptions;
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
                UpdateType.Message => HandleMessageAsync(bot, update.Message!),
                UpdateType.CallbackQuery => HandleCallbackQuery(bot, update.CallbackQuery!),
                _ => Task.CompletedTask
            });
        }
        catch (Exception ex)
        {
            await bot.SendTextMessageAsync(update.Message!.Chat.Id, ex switch       // Bad idea replace with PollingErrorHandler
            {
                ArgumentOutOfRangeException => $"\n Discovered {Program.devicesCollection.Count()} devices.",
                ApiRequestException => $"API: {ex.Message}",
                _ => $"Exception while handling '{update.Type}'. ex: {ex.Message}. source: {ex.Source}. type: {ex.GetType().Name}",
            });
        }
    }

    private static async Task HandleCallbackQuery(ITelegramBotClient bot, CallbackQuery callbackQuery)
    {
        await (callbackQuery.Data switch
        {
            
            _ => Task.CompletedTask
        });
    }

    private static async Task HandleMessageAsync(ITelegramBotClient bot, Message msg)
    {
        try
        {
            await (msg switch
            {
                { Text: "/list" } => LocateAndListAsync(bot, msg),
                { Text: "/on" } => TurnOnAsync(bot, msg),
                { Text: "/q" } when msg.Chat.Id == long.Parse(Configuration.config["tgadmin"]) =>
                    Task.Run(() => Program.Shut("admin shutdown request")),
                _ => Task.CompletedTask
            });
        }
        catch (System.Exception ex)
        {
            await bot.SendTextMessageAsync(msg.Chat.Id, $"Exception while handling Message: {ex.Message}");
        }
    }

    private static async Task TurnOnAsync(ITelegramBotClient bot, Message msg)
    {
        var inlineKeyboard = new InlineKeyboardMarkup(
            new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "lamp name", callbackData: "lamp id"),
                InlineKeyboardButton.WithCallbackData(text: "another lamp", callbackData: "another id"),
            }
        );


        await bot.SendTextMessageAsync(msg.Chat.Id,
                                 "Which Lamp do you want to turn on?",
                                 replyMarkup: inlineKeyboard);
    }

    private static async Task LocateAndListAsync(ITelegramBotClient bot, Message msg)
    {
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

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YeelightAPI.Models;

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
            var dev = Program.devicesCollection.ElementAt(0);

            await dev.Connect();

            var state = await dev.GetProp(PROPERTIES.power);
            await (update switch
            {
                { Message.Text: "/list" } => bot.SendTextMessageAsync(
                    update.Message.Chat.Id, $"Discovered: {dev.Model} {state as string}"),
                { Message.Text: "/on" } => dev.TurnOn(1000),
                { Message.Text: "/off" } => dev.TurnOff(1000),
                { } when update.Type is UpdateType.Message && update.Message!.Text is not null => bot.SendTextMessageAsync(
                    update.Message!.Chat.Id, $"Unknown command \"{update.Message.Text}\""),
                // { } => Task.Run(() => System.Console.WriteLine(update.Type + " " + update.Message!.Text)),
                _ => Task.CompletedTask
            });

            dev.Disconnect();
        }
        catch (Exception ex)
        {
            await bot.SendTextMessageAsync(update.Message!.Chat.Id, ex switch
            {
                ArgumentOutOfRangeException => $"\n Discovered {Program.devicesCollection.Count()} devices.",
                _ => $"Exception while handling '{update.Type}': {ex.Message}.",
            });
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using YeelightAPI;

namespace LampLackey;
public static class Program
{
    public static async Task Main()
    {
        var bot = new TelegramBotClient(Config.Token);
        var me = await bot.GetMeAsync();
        using var cts = new CancellationTokenSource();
        var receieverOptions = new ReceiverOptions()
        {
            ThrowPendingUpdates = true,
        };

        bot.StartReceiving(updateHandler: UpdateHandler.HandleUpdateAsync,
                           pollingErrorHandler: ErrorHandler.PollingErrorHandler,
                           receiverOptions: receieverOptions,
                           cancellationToken: cts.Token);

        Console.WriteLine($"Start listening for @{me.Username} Id: {me.Id}");

        Console.CancelKeyPress += (_, _) =>
        {
            cts.Cancel();
            System.Console.WriteLine("Terminating");
        };

        Thread.Sleep(Timeout.Infinite);

    }
}

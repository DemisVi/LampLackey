using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using YeelightAPI;

namespace LampLackey;
public static class Program
{
    public static IEnumerable<Device> devicesCollection;

    static Program()
    {
        devicesCollection = DeviceLocator.DiscoverAsync().Result;
    }

    public static async Task Main()
    {
        var config = Configuration.Read();
        var bot = new TelegramBotClient(config["tgtoken"]);
        var me = await bot.GetMeAsync();
        using var cts = new CancellationTokenSource();
        var receieverOptions = new ReceiverOptions()
        {
            ThrowPendingUpdates = true,
        };

        bot.StartReceiving(updateHandler: UpdateHandlers.HandleUpdateAsync,
                           pollingErrorHandler: UpdateHandlers.PollingErrorHandler,
                           receiverOptions: receieverOptions,
                           cancellationToken: cts.Token);

        Console.WriteLine($"Start listening for @{me.Username} id: {me.Id}");
        System.Console.WriteLine("Press 'q' to quit...");

        while (Console.ReadKey().KeyChar != 'q') ;

        cts.Cancel();
    }
}

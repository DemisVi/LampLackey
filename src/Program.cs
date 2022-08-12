using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using YeelightAPI;

namespace LampLackey;
internal static class Program
{
    internal static IEnumerable<Device> devicesCollection;

    static Program() => devicesCollection = DeviceLocator.DiscoverAsync().Result;

    public static async Task Main()
    {
        var bot = new TelegramBotClient(Configuration.config["tgtoken"]);
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

        await bot.SendTextMessageAsync(Configuration.config["tgadmin"],
                                       $"Start listening for @{me.Username} id: {me.Id}. '/q' to shut");

        while (Console.ReadKey().KeyChar != 'q') ;

        cts.Cancel();
    }

    internal static void Shut(string message)       // TODO: need some better idea how terminate application
    {
        System.Console.WriteLine(message);
        Environment.Exit(0);
    }
}

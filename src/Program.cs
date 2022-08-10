using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Threading.Tasks;
using YeelightAPI;
using YeelightAPI.Models;

namespace LampLackey;
public static class Program
{
    public static IEnumerable<Device> devs;
    public static Device dev;

    static Program()
    {
        devs = DeviceLocator.DiscoverAsync().Result;
        dev  = devs.ElementAt(0);
    }
    
    public static async Task Main()
{
    var config = Configuration.Read();



    var bot = new TelegramBotClient(config["tgtoken"]);
    var me = await bot.GetMeAsync();
    using var cts = new CancellationTokenSource();

    bot.StartReceiving(HandleUpdateAsync, PollingErrorHandler, null, cts.Token);

    Console.WriteLine($"Start listening for @{me.Username} id: {me.Id}");
    System.Console.WriteLine("Press 'q' to quit...");

    while (Console.ReadKey().KeyChar != 'q') ;

    cts.Cancel();
}

private static Task PollingErrorHandler(ITelegramBotClient bot, Exception ex, CancellationToken cts)
{
    System.Console.WriteLine($"Polling exception {ex}");
    return Task.CompletedTask;
}

private static async Task HandleUpdateAsync(ITelegramBotClient bot, Update upd, CancellationToken cts)
{


    await dev.Connect();
    var state = (string)await dev.GetProp(PROPERTIES.power);
    try
    {
        await (upd switch
        {
            { Message.Text: "/list" } => bot.SendTextMessageAsync(
                upd.Message.Chat.Id, $"Discovered: {dev.Model} {state}"),
            { Message.Text: "/on" } => dev.TurnOn(1000),
            { Message.Text: "/off" } => dev.TurnOff(1000),
            { } => Task.Run(() => System.Console.WriteLine(upd.Type + " " + upd.Message!.Text)),
            _ => Task.CompletedTask
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception while handling {upd.Type}: {ex}");
    }
}
}

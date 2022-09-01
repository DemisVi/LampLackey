using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace LampLackey;

public class ErrorHandler
{
        public static Task PollingErrorHandler(ITelegramBotClient bot, Exception ex, CancellationToken cts)
        {
            System.Console.WriteLine($"Polling exception {ex}");
            return Task.CompletedTask;
        }
}
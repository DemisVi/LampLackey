using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YeelightAPI;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LampLackey;

public class Messages
{
    internal static string GetDeviceListMessage(IEnumerable<Device> devs)
    {
        var stringBuilder = new StringBuilder();

        var format = "💡Id: {0}\n" +
                     "Name: {1}\n" +
                     "Model: {2}\n";

        foreach (var dev in devs)
        {
            stringBuilder.AppendFormat(format, dev.Id, dev.Name, dev.Model);
        }

        return stringBuilder.ToString();
    }

    internal static string GetUsageMessage()
    {
        return "/usage - get this message" +
               "/list - List available Lights\n" +
               "/scan - Scan network for Lights\n" +
               "/switch - Switching menu\n";
    }
}
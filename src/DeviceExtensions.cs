using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YeelightAPI;

namespace LampLackey;

public static class DeviceExtensions
{
    public static string GetPowerState(this Device dev)
    {
        if (dev.IsConnected)
        {
            return dev["power"].ToString()!.Contains("on") ? " ⚪ on" : " 🟤 off";
        }
        return " ⚫";
    }

    public static string GetNameById(this IEnumerable<Device> devs, string id) =>
        devs.Where(x => x.Id == id).ElementAt(0).Name;
}
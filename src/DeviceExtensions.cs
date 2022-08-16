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
        return "⚫";
    }
}
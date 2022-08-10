using System.Collections.Generic;
using System.Text.Json;

namespace LampLackey;

public static class Configuration
{
    public static Dictionary<string, string> Read()
    {
        using var fs = System.IO.File.OpenRead("./config.json");

        return JsonSerializer.Deserialize<Dictionary<string, string>>(
            fs, new JsonSerializerOptions() { AllowTrailingCommas = true })!;
    }
}
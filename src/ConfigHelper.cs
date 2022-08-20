using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace LampLackey;

public static class ConfigHelper
{
    private static Config config;
    public static string Token => config.Token;
    public static List<string> Users => config.Users;
    static ConfigHelper()
    {
        /* try
        {
            using var fileStream = File.OpenRead("config.json");
            config = JsonSerializer.Deserialize<Config>(fileStream);
        }
        catch (Exception ex)
        {
            config = ex switch
            {
                JsonException => ReadAsLine(),
                FileNotFoundException => CreateBlankConfig(),
                _ => throw new Exception("Can't read or create config.json")
            };
        } */

        if (File.Exists("config.json"))
        {
            try
            {
                using var fileStream = File.OpenRead("config.json");
                config = JsonSerializer.Deserialize<Config>(fileStream);
            }
            catch (JsonException)
            {
                config = ReadAsLine();
            }
        }
        else
        {
            config = CreateBlankConfig();
        }

        static Config ReadAsLine()
        {
            try
            {
                var line = File.ReadAllLines("./config.json")[0].Trim('"');

                config = new Config(line);
                Save();

                return config;
            }
            catch (IndexOutOfRangeException)
            {
                return CreateBlankConfig();
            }
        }

        static Config CreateBlankConfig()
        {
            config = new Config();
            Save();
            return config;
        }
    }

    private static void Save(Config config)
    {
        using var fileStream = File.OpenWrite("config.json");
        JsonSerializer.Serialize(fileStream, config);
    }
    public static void Save() => Save(config);

    public static void AddUser(string id)
    {
        if (!Users.Contains(id))
        {
            Users.Add(id);
        }
    }

    private struct Config
    {
        public string Token { get; set; } = string.Empty;
        public List<string> Users { get; set; } = new();

        public Config(string token) => Token = token;
        public Config() => Token = "token";
    }
}
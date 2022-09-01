using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace LampLackey;

public static class Config
{
    private static readonly ConfigHelper config;
    public static string Token => config.Token;
    public static string AdminId => config.AdminId;
    public static ReadOnlyCollection<string> Users => config.Users.AsReadOnly();
    static Config()
    {
        if (File.Exists("config.json") is not true)
        {
            throw new IndexOutOfRangeException("config.json file must exist and contain Bot Token at least as a string");
        }
        try
        {
            using var fileStream = File.OpenRead("config.json");
            config = JsonSerializer.Deserialize<ConfigHelper>(fileStream)!;
        }
        catch (JsonException)
        {
            ReadAsLine(out config);
            Save();
        }

        static void ReadAsLine(out ConfigHelper conf)
        {
            var lines = File.ReadAllLines("./config.json");
            if (lines is not { Length: > 0 } or null)
                throw new IndexOutOfRangeException("config.json file must exist and contain Bot Token at least as a string");

            var line = lines.First().Trim(new char[] { '"', ' ', ',', '{', '}', '[', ']' });

            conf = new ConfigHelper(line);
        }
    }

    public static void AddUser(string id)
    {
        if (config.Users.Contains(id) is not true)
        {
            config.Users.Add(id);
        }
    }

    public static void RemoveUser(string id)
    {
        if (config.Users.Contains(id))
        {
            config.Users.Remove(id);
        }
    }

    public static void Save() => Save(config);

    private static void Save(ConfigHelper config)
    {
        using var fileStream = File.OpenWrite("config.json");
        JsonSerializer.Serialize(fileStream, config);
    }

    private class ConfigHelper
    {
        public string Token { get; set; } = string.Empty;
        public string AdminId { get; set; } = string.Empty;
        public List<string> Users { get; set; } = new();

        public ConfigHelper(string token) => Token = token;
        public ConfigHelper() : this("null") { }
    }
}
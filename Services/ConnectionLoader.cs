using System.IO;
using System.Text.Json;
using DbQueryExplorer.Models;

namespace DbQueryExplorer.Services;

public static class ConnectionLoader
{
    private const string FileName = "connections.json";

    public static List<ConnectionProfile> Load()
    {
        var paths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, FileName),
            Path.Combine(Directory.GetCurrentDirectory(), FileName)
        };

        foreach (var path in paths)
        {
            if (!File.Exists(path)) continue;

            var json    = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var config  = JsonSerializer.Deserialize<ConnectionsConfig>(json, options);
            if (config?.Connections?.Count > 0)
                return config.Connections;
        }

        return new List<ConnectionProfile>();
    }

    public static string ConfigFilePath =>
        Path.Combine(AppContext.BaseDirectory, FileName);

    public static void SaveProfile(ConnectionProfile newProfile)
    {
        var path    = ConfigFilePath;
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        ConnectionsConfig config;
        if (File.Exists(path))
        {
            var existing = File.ReadAllText(path);
            config = JsonSerializer.Deserialize<ConnectionsConfig>(existing, options) ?? new ConnectionsConfig();
        }
        else
        {
            config = new ConnectionsConfig();
        }

        var idx = config.Connections.FindIndex(c =>
            string.Equals(c.Name, newProfile.Name, StringComparison.OrdinalIgnoreCase));

        if (idx >= 0)
            config.Connections[idx] = newProfile;
        else
            config.Connections.Add(newProfile);

        File.WriteAllText(path, JsonSerializer.Serialize(config, options));
    }
}

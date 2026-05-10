using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManagedCode.FeatureChecker.Storage;

public static class FeatureSnapshotSerializer
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    public static string Serialize(FeatureSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return JsonSerializer.Serialize(snapshot, Options);
    }

    public static FeatureSnapshot Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return JsonSerializer.Deserialize<FeatureSnapshot>(json, Options) ?? new FeatureSnapshot();
    }

    public static FeatureSnapshot Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        return JsonSerializer.Deserialize<FeatureSnapshot>(stream, Options) ?? new FeatureSnapshot();
    }

    public static void Write(Stream stream, FeatureSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(snapshot);

        JsonSerializer.Serialize(stream, snapshot, Options);
    }

    public static FeatureSnapshot Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using var stream = File.OpenRead(path);

        return Read(stream);
    }

    public static void Save(string path, FeatureSnapshot snapshot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(snapshot);

        using var stream = File.Create(path);
        Write(stream, snapshot);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }
}

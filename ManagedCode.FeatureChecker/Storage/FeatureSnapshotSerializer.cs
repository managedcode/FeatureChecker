using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManagedCode.FeatureChecker.Storage;

public static class FeatureSnapshotSerializer
{
    private const string MissingSnapshotMessage = "A feature snapshot must be a JSON object.";
    private static readonly JsonSerializerOptions Options = CreateOptions();

    public static string Serialize(FeatureSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return JsonSerializer.Serialize(snapshot, Options);
    }

    public static FeatureSnapshot Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return JsonSerializer.Deserialize<FeatureSnapshot>(json, Options) ?? throw new JsonException(MissingSnapshotMessage);
    }

    public static FeatureSnapshot Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        return JsonSerializer.Deserialize<FeatureSnapshot>(stream, Options) ?? throw new JsonException(MissingSnapshotMessage);
    }

    internal static FeatureSnapshot Read(ReadOnlySpan<byte> utf8Json)
    {
        var preamble = Encoding.UTF8.Preamble;
        if (utf8Json.StartsWith(preamble))
        {
            utf8Json = utf8Json[preamble.Length..];
        }

        return JsonSerializer.Deserialize<FeatureSnapshot>(utf8Json, Options) ?? throw new JsonException(MissingSnapshotMessage);
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

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);

        return Read(stream);
    }

    public static void Save(string path, FeatureSnapshot snapshot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(snapshot);

        var destination = new FileInfo(path);
        if (destination.LinkTarget is not null)
        {
            destination = (FileInfo)destination.ResolveLinkTarget(returnFinalTarget: true)!;
        }

        var directory = destination.DirectoryName ?? throw new IOException("A snapshot path must name a file.");
        var stagingPath = Path.Combine(directory, Path.GetRandomFileName());
        var stagingStream = CreateStagingFile(stagingPath);
        try
        {
            using (stagingStream)
            {
                Write(stagingStream, snapshot);
                stagingStream.Flush(flushToDisk: true);
            }

            Publish(stagingPath, destination.FullName);
        }
        finally
        {
            File.Delete(stagingPath);
        }
    }

    private static FileStream CreateStagingFile(string path)
    {
        var options = new FileStreamOptions
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.Write
        };
        if (!OperatingSystem.IsWindows())
        {
            options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
        }

        return new FileStream(path, options);
    }

    private static void Publish(string stagingPath, string destination)
    {
        if (File.Exists(destination))
        {
            if (OperatingSystem.IsWindows())
            {
                File.Replace(stagingPath, destination, destinationBackupFileName: null);
                return;
            }

            File.SetUnixFileMode(stagingPath, File.GetUnixFileMode(destination));
        }

        File.Move(stagingPath, destination, overwrite: true);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}

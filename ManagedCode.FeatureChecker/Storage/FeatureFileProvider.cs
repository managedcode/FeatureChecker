using System.Buffers;
using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Segments;

namespace ManagedCode.FeatureChecker.Storage;

public sealed class FeatureFileProvider : IFeatureDefinitionProvider, IFeatureSegmentProvider, IFeatureSnapshotSource
{
    private const int MaximumCachedFileBytes = 1024 * 1024;
    private CachedSnapshot? _cachedSnapshot;

    public FeatureFileProvider(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        FilePath = filePath;
    }

    public string FilePath { get; }

    public IReadOnlyCollection<FeatureDefinition> GetFeatureDefinitions()
    {
        return GetSnapshot().Features;
    }

    public IReadOnlyCollection<FeatureSegment> GetFeatureSegments()
    {
        return GetSnapshot().Segments;
    }

    public FeatureSnapshot GetSnapshot()
    {
        using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, bufferSize: 1);
        var length = stream.Length;
        return length <= MaximumCachedFileBytes
            ? GetCachedSnapshot(stream, (int)length)
            : FeatureSnapshotSerializer.Read(stream);
    }

    private FeatureSnapshot GetCachedSnapshot(FileStream stream, int length)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            var content = buffer.AsSpan(0, length);
            stream.ReadExactly(content);
            if (stream.ReadByte() != -1)
            {
                stream.Position = 0;
                return FeatureSnapshotSerializer.Read(stream);
            }

            var cached = Volatile.Read(ref _cachedSnapshot);
            if (cached is not null && content.SequenceEqual(cached.Content))
            {
                return cached.Snapshot;
            }

            var snapshot = FeatureSnapshotSerializer.Read(content);
            Volatile.Write(ref _cachedSnapshot, new CachedSnapshot(content.ToArray(), snapshot));
            return snapshot;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }

    private sealed record CachedSnapshot(byte[] Content, FeatureSnapshot Snapshot);
}

using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Segments;

namespace ManagedCode.FeatureChecker.Storage;

public sealed class FeatureFileProvider : IFeatureDefinitionProvider, IFeatureSegmentProvider, IFeatureSnapshotSource
{
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
        return FeatureSnapshotSerializer.Load(FilePath);
    }
}

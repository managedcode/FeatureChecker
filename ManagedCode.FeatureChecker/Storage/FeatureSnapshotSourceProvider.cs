using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Segments;

namespace ManagedCode.FeatureChecker.Storage;

public sealed class FeatureSnapshotSourceProvider : IFeatureDefinitionProvider, IFeatureSegmentProvider, IFeatureSnapshotSource
{
    private readonly IFeatureSnapshotSource _source;

    public FeatureSnapshotSourceProvider(IFeatureSnapshotSource source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public FeatureSnapshot GetSnapshot()
    {
        return _source.GetSnapshot();
    }

    public IReadOnlyCollection<FeatureDefinition> GetFeatureDefinitions()
    {
        return GetSnapshot().Features;
    }

    public IReadOnlyCollection<FeatureSegment> GetFeatureSegments()
    {
        return GetSnapshot().Segments;
    }
}

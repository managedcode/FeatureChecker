using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Segments;

namespace ManagedCode.FeatureChecker.Storage;

public sealed class FeatureSnapshotProvider : IFeatureDefinitionProvider, IFeatureSegmentProvider, IFeatureSnapshotSource
{
    private readonly FeatureSnapshot _snapshot;

    public FeatureSnapshotProvider(FeatureSnapshot snapshot)
    {
        _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
    }

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
        return _snapshot;
    }
}

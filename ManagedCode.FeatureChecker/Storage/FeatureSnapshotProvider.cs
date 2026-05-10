using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Segments;

namespace ManagedCode.FeatureChecker.Storage;

public sealed class FeatureSnapshotProvider : IFeatureDefinitionProvider, IFeatureSegmentProvider
{
    private readonly FeatureSnapshot _snapshot;

    public FeatureSnapshotProvider(FeatureSnapshot snapshot)
    {
        _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
    }

    public IReadOnlyCollection<FeatureDefinition> GetFeatureDefinitions()
    {
        return _snapshot.Features;
    }

    public IReadOnlyCollection<FeatureSegment> GetFeatureSegments()
    {
        return _snapshot.Segments;
    }
}

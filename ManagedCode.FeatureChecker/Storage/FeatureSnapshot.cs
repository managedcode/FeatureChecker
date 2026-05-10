using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Segments;

namespace ManagedCode.FeatureChecker.Storage;

public sealed record FeatureSnapshot
{
    public int SchemaVersion { get; init; } = 1;

    public IReadOnlyList<FeatureDefinition> Features { get; init; } = [];

    public IReadOnlyList<FeatureSegment> Segments { get; init; } = [];

    public static FeatureSnapshot FromDefinitions(IEnumerable<FeatureDefinition> features)
    {
        ArgumentNullException.ThrowIfNull(features);

        return new FeatureSnapshot
        {
            Features = features.ToList()
        };
    }
}

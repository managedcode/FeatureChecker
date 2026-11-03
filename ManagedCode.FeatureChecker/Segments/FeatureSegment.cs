using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Segments;

public sealed record FeatureSegment
{
    public string Key { get; init; } = string.Empty;

    public string? Description { get; init; }

    public IReadOnlyList<string> IncludedKeys { get; init => field = FeatureCollections.Freeze(value); } = [];

    public IReadOnlyList<string> ExcludedKeys { get; init => field = FeatureCollections.Freeze(value); } = [];

    public IReadOnlyList<FeatureTargetingRule> Rules { get; init => field = FeatureCollections.Freeze(value); } = [];

    public IReadOnlyDictionary<string, string> Metadata { get; init => field = FeatureCollections.FreezeMetadata(value); } = FeatureCollections.EmptyMetadata;
}

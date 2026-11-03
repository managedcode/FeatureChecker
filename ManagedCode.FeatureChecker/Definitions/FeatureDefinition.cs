using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Definitions;

public sealed record FeatureDefinition
{
    public string Key { get; init; } = string.Empty;

    public FeatureStatus Status { get; init; }

    public FeatureMode Mode { get; init; } = FeatureMode.Release;

    public bool TargetingEnabled { get; init; } = true;

    public string? Description { get; init; }

    public string? Value { get; init; }

    public string? FallthroughVariant { get; init; }

    public string? FallthroughValue { get; init; }

    public string? OffVariant { get; init; }

    public string? OffValue { get; init; }

    public IReadOnlyList<FeatureTarget> Targets { get; init => field = FeatureCollections.Freeze(value); } = [];

    public IReadOnlyList<FeatureTargetingRule> Rules { get; init => field = FeatureCollections.Freeze(value); } = [];

    public IReadOnlyList<FeatureDependency> Dependencies { get; init => field = FeatureCollections.Freeze(value); } = [];

    public IReadOnlyList<FeatureVariant> Variants { get; init => field = FeatureCollections.Freeze(value); } = [];

    public IReadOnlyDictionary<string, string> Metadata { get; init => field = FeatureCollections.FreezeMetadata(value); } = FeatureCollections.EmptyMetadata;

    public static FeatureDefinition Create(string key, FeatureStatus status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return new FeatureDefinition
        {
            Key = key,
            Status = status
        };
    }
}

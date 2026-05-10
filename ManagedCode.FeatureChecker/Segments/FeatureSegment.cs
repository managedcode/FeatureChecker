using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Segments;

public sealed record FeatureSegment
{
    public string Key { get; init; } = string.Empty;

    public string? Description { get; init; }

    public IReadOnlyList<string> IncludedKeys { get; init; } = [];

    public IReadOnlyList<string> ExcludedKeys { get; init; } = [];

    public IReadOnlyList<FeatureTargetingRule> Rules { get; init; } = [];

    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

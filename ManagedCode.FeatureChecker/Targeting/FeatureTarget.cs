using ManagedCode.FeatureChecker.Definitions;

namespace ManagedCode.FeatureChecker.Targeting;

public sealed record FeatureTarget
{
    public string Key { get; init; } = string.Empty;

    public string? ContextKind { get; init; }

    public FeatureStatus Status { get; init; } = FeatureStatus.Enabled;

    public string? Variant { get; init; }

    public string? Value { get; init; }
}

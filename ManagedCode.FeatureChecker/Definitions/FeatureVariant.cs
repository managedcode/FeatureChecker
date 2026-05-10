namespace ManagedCode.FeatureChecker.Definitions;

public sealed record FeatureVariant
{
    public string Name { get; init; } = string.Empty;

    public FeatureStatus Status { get; init; } = FeatureStatus.Enabled;

    public double Weight { get; init; }

    public string? Value { get; init; }
}

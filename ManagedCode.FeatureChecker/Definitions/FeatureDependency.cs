namespace ManagedCode.FeatureChecker.Definitions;

public sealed record FeatureDependency
{
    public string Key { get; init; } = string.Empty;

    public FeatureStatus RequiredStatus { get; init; } = FeatureStatus.Enabled;

    public string? RequiredVariant { get; init; }
}

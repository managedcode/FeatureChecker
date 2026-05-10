namespace ManagedCode.FeatureChecker.Storage;

public sealed class FeatureCheckerOptions
{
    public FeatureSnapshot Snapshot { get; set; } = new();

    public string? FilePath { get; set; }
}

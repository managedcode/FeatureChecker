using ManagedCode.FeatureChecker.Definitions;

namespace ManagedCode.FeatureChecker.Storage;

public sealed class FeatureFileProvider : IFeatureDefinitionProvider
{
    public FeatureFileProvider(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        FilePath = filePath;
    }

    public string FilePath { get; }

    public IReadOnlyCollection<FeatureDefinition> GetFeatureDefinitions()
    {
        return FeatureSnapshotSerializer.Load(FilePath).Features;
    }
}

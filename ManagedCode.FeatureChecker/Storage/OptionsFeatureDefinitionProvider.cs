using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Segments;
using Microsoft.Extensions.Options;

namespace ManagedCode.FeatureChecker.Storage;

public sealed class OptionsFeatureDefinitionProvider : IFeatureDefinitionProvider, IFeatureSegmentProvider
{
    private readonly IOptionsMonitor<FeatureCheckerOptions> _options;

    public OptionsFeatureDefinitionProvider(IOptionsMonitor<FeatureCheckerOptions> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public IReadOnlyCollection<FeatureDefinition> GetFeatureDefinitions()
    {
        return GetSnapshot().Features;
    }

    public IReadOnlyCollection<FeatureSegment> GetFeatureSegments()
    {
        return GetSnapshot().Segments;
    }

    private FeatureSnapshot GetSnapshot()
    {
        var options = _options.CurrentValue;

        if (!string.IsNullOrWhiteSpace(options.FilePath) && File.Exists(options.FilePath))
        {
            return FeatureSnapshotSerializer.Load(options.FilePath);
        }

        return options.Snapshot;
    }
}

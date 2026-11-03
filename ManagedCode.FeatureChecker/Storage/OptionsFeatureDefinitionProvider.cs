using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Segments;
using Microsoft.Extensions.Options;

namespace ManagedCode.FeatureChecker.Storage;

public sealed class OptionsFeatureDefinitionProvider : IFeatureDefinitionProvider, IFeatureSegmentProvider, IFeatureSnapshotSource
{
    private readonly IOptionsMonitor<FeatureCheckerOptions> _options;
    private FeatureFileProvider? _fileProvider;

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

    public FeatureSnapshot GetSnapshot()
    {
        var options = _options.CurrentValue;
        var path = options.FilePath;
        if (string.IsNullOrWhiteSpace(path))
        {
            return options.Snapshot;
        }

        var fileProvider = Volatile.Read(ref _fileProvider);
        if (fileProvider is null || !string.Equals(fileProvider.FilePath, path, StringComparison.Ordinal))
        {
            fileProvider = new FeatureFileProvider(path);
            Volatile.Write(ref _fileProvider, fileProvider);
        }

        return fileProvider.GetSnapshot();
    }
}

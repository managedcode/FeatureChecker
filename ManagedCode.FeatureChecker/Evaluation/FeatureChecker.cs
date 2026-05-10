using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Segments;
using ManagedCode.FeatureChecker.Storage;
using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Evaluation;

public class FeatureChecker : IFeatureEvaluator
{
    private readonly IReadOnlyDictionary<string, FeatureDefinition> _features;
    private readonly FeatureEvaluationEngine _engine;

    public FeatureChecker(FeatureSnapshot snapshot)
        : this(new FeatureSnapshotProvider(snapshot))
    {
    }

    public FeatureChecker(IFeatureDefinitionProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        if (provider is IFeatureSnapshotSource snapshotSource)
        {
            var snapshot = snapshotSource.GetSnapshot();
            var snapshotMaps = CreateMaps(snapshot.Features, snapshot.Segments);

            _features = snapshotMaps.Features;
            _engine = new FeatureEvaluationEngine(snapshotMaps.Features, snapshotMaps.Segments);

            return;
        }

        var maps = CreateMaps(
            provider.GetFeatureDefinitions(),
            provider is IFeatureSegmentProvider segmentProvider ? segmentProvider.GetFeatureSegments() : []);
        _features = maps.Features;
        _engine = new FeatureEvaluationEngine(maps.Features, maps.Segments);
    }

    public FeatureChecker(IEnumerable<FeatureDefinition> features)
        : this(features, [])
    {
    }

    public FeatureChecker(IEnumerable<FeatureDefinition> features, IEnumerable<FeatureSegment> segments)
    {
        var maps = CreateMaps(features, segments);
        _features = maps.Features;
        _engine = new FeatureEvaluationEngine(maps.Features, maps.Segments);
    }

    private static (IReadOnlyDictionary<string, FeatureDefinition> Features, IReadOnlyDictionary<string, FeatureSegment> Segments) CreateMaps(
        IEnumerable<FeatureDefinition> features,
        IEnumerable<FeatureSegment> segments)
    {
        ArgumentNullException.ThrowIfNull(features);
        ArgumentNullException.ThrowIfNull(segments);

        var featureMap = features.ToDictionary(
            feature => RequireKey(feature.Key),
            feature => feature,
            StringComparer.Ordinal);
        var segmentMap = segments.ToDictionary(
            segment => RequireKey(segment.Key),
            segment => segment,
            StringComparer.Ordinal);

        return (featureMap, segmentMap);
    }

    public int Count => _features.Count;

    public bool IsFeatureExists(string featureKey)
    {
        return _features.ContainsKey(RequireKey(featureKey));
    }

    public bool IsEnabled(string featureKey, FeatureEvaluationContext? context = null)
    {
        return Evaluate(featureKey, context).IsEnabled;
    }

    public bool IsDisabled(string featureKey, FeatureEvaluationContext? context = null)
    {
        return Evaluate(featureKey, context).IsDisabled;
    }

    public bool IsDebug(string featureKey, FeatureEvaluationContext? context = null)
    {
        return Evaluate(featureKey, context).IsDebug;
    }

    public bool TryGetFeatureStatus(string featureKey, out FeatureStatus status)
    {
        var evaluation = Evaluate(featureKey);
        status = evaluation.Status;

        return evaluation.Exists;
    }

    public IReadOnlyList<string> GetFeatureKeysByStatus(FeatureStatus status, FeatureEvaluationContext? context = null)
    {
        return EvaluateAll(context)
            .Where(feature => feature.Value.Status == status)
            .Select(feature => feature.Key)
            .ToList();
    }

    public FeatureEvaluation Evaluate(string featureKey, FeatureEvaluationContext? context = null)
    {
        var key = RequireKey(featureKey);
        var evaluationContext = context ?? FeatureEvaluationContext.Empty;

        return _engine.Evaluate(key, evaluationContext);
    }

    public IReadOnlyDictionary<string, FeatureEvaluation> EvaluateAll(FeatureEvaluationContext? context = null)
    {
        var evaluationContext = context ?? FeatureEvaluationContext.Empty;

        return _features.Keys.ToDictionary(
            key => key,
            key => _engine.Evaluate(key, evaluationContext),
            StringComparer.Ordinal);
    }

    private static string RequireKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return key;
    }

}

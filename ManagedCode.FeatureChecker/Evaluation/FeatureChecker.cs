using System.Runtime.CompilerServices;
using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Segments;
using ManagedCode.FeatureChecker.Storage;
using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Evaluation;

public class FeatureChecker : IFeatureEvaluator
{
    private static readonly ConditionalWeakTable<FeatureSnapshot, SnapshotIndex> SnapshotIndexes = new();
    private readonly Dictionary<string, FeatureDefinition> _features;
    private readonly FeatureEvaluationEngine _engine;

    public FeatureChecker(FeatureSnapshot snapshot)
        : this(GetSnapshotIndex(snapshot))
    {
    }

    public FeatureChecker(IFeatureDefinitionProvider provider)
        : this(CreateIndex(provider))
    {
    }

    public FeatureChecker(IEnumerable<FeatureDefinition> features)
        : this(features, [])
    {
    }

    public FeatureChecker(IEnumerable<FeatureDefinition> features, IEnumerable<FeatureSegment> segments)
        : this(CreateIndex(features, segments))
    {
    }

    private FeatureChecker(SnapshotIndex index)
    {
        _features = index.Features;
        _engine = index.Engine;
    }

    private static SnapshotIndex GetSnapshotIndex(FeatureSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        return SnapshotIndexes.GetValue(snapshot, static item => CreateIndex(item.Features, item.Segments));
    }

    private static SnapshotIndex CreateIndex(IFeatureDefinitionProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        return provider is IFeatureSnapshotSource snapshotSource
            ? GetSnapshotIndex(snapshotSource.GetSnapshot())
            : CreateIndex(provider.GetFeatureDefinitions(), provider is IFeatureSegmentProvider segmentProvider ? segmentProvider.GetFeatureSegments() : []);
    }

    private static SnapshotIndex CreateIndex(
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

        return new SnapshotIndex(featureMap, new FeatureEvaluationEngine(featureMap, segmentMap));
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
        var evaluationContext = context ?? FeatureEvaluationContext.Empty;
        var keys = new List<string>();
        Dictionary<string, FeatureEvaluation?>? dependencyResults = null;
        foreach (var (key, definition) in _features)
        {
            if (_engine.Evaluate(definition, evaluationContext, ref dependencyResults).Status == status)
            {
                keys.Add(key);
            }
        }

        return keys;
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

        var evaluations = new Dictionary<string, FeatureEvaluation>(_features.Count, StringComparer.Ordinal);
        Dictionary<string, FeatureEvaluation?>? dependencyResults = null;
        foreach (var (key, definition) in _features)
        {
            evaluations.Add(key, _engine.Evaluate(definition, evaluationContext, ref dependencyResults));
        }

        return evaluations;
    }

    private static string RequireKey(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return key;
    }

    private sealed record SnapshotIndex(Dictionary<string, FeatureDefinition> Features, FeatureEvaluationEngine Engine);
}

using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Segments;

namespace ManagedCode.FeatureChecker.Targeting;

public sealed record FeatureTargetingRule
{
    public string? Name { get; init; }

    public FeatureStatus Status { get; init; } = FeatureStatus.Enabled;

    public string? Variant { get; init; }

    public string? Value { get; init; }

    public double? Percentage { get; init; }

    public IReadOnlyList<string> IncludeSegments { get; init => field = FeatureCollections.Freeze(value); } = [];

    public IReadOnlyList<string> ExcludeSegments { get; init => field = FeatureCollections.Freeze(value); } = [];

    public IReadOnlyList<FeatureCondition> Conditions { get; init => field = FeatureCollections.Freeze(value); } = [];

    internal bool Matches(string featureKey, FeatureEvaluationContext context, Func<string, FeatureEvaluationContext, bool?> segmentMatcher)
    {
        if (!MatchesConditions(featureKey, context))
        {
            return false;
        }

        var segments = new SegmentRuleMatch(this);
        while (segments.TryMoveNext(out var segmentKey))
        {
            segments.Accept(segmentMatcher(segmentKey, context));
        }

        return segments.Result is true;
    }

    internal bool MatchesConditions(string featureKey, FeatureEvaluationContext context)
    {
        return AllConditionsMatch(context) && MatchesPercentage(featureKey, context);
    }

    private bool MatchesPercentage(string featureKey, FeatureEvaluationContext context)
    {
        if (Percentage is not { } percentage)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(context.TargetingKey)
            && FeatureRollout.IsIncluded(featureKey, context.TargetingKey, percentage);
    }

    private bool AllConditionsMatch(FeatureEvaluationContext context)
    {
        for (var index = 0; index < Conditions.Count; index++)
        {
            if (!Conditions[index].Matches(context))
            {
                return false;
            }
        }

        return true;
    }
}

using ManagedCode.FeatureChecker.Definitions;

namespace ManagedCode.FeatureChecker.Targeting;

public sealed record FeatureTargetingRule
{
    public string? Name { get; init; }

    public FeatureStatus Status { get; init; } = FeatureStatus.Enabled;

    public string? Variant { get; init; }

    public string? Value { get; init; }

    public double? Percentage { get; init; }

    public IReadOnlyList<string> IncludeSegments { get; init; } = [];

    public IReadOnlyList<string> ExcludeSegments { get; init; } = [];

    public IReadOnlyList<FeatureCondition> Conditions { get; init; } = [];

    internal bool Matches(string featureKey, FeatureEvaluationContext context, Func<string, FeatureEvaluationContext, bool>? segmentMatcher = null)
    {
        return MatchesSegments(context, segmentMatcher)
            && AllConditionsMatch(context)
            && MatchesPercentage(featureKey, context);
    }

    private bool MatchesSegments(FeatureEvaluationContext context, Func<string, FeatureEvaluationContext, bool>? segmentMatcher)
    {
        if (segmentMatcher is null)
        {
            return IncludeSegments.Count == 0 && ExcludeSegments.Count == 0;
        }

        return !ContainsMatchingSegment(ExcludeSegments, context, segmentMatcher)
            && (IncludeSegments.Count == 0 || ContainsMatchingSegment(IncludeSegments, context, segmentMatcher));
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

    private static bool ContainsMatchingSegment(
        IReadOnlyList<string> segmentKeys,
        FeatureEvaluationContext context,
        Func<string, FeatureEvaluationContext, bool> segmentMatcher)
    {
        foreach (var segmentKey in segmentKeys)
        {
            if (segmentMatcher(segmentKey, context))
            {
                return true;
            }
        }

        return false;
    }

    private bool AllConditionsMatch(FeatureEvaluationContext context)
    {
        foreach (var condition in Conditions)
        {
            if (!condition.Matches(context))
            {
                return false;
            }
        }

        return true;
    }
}

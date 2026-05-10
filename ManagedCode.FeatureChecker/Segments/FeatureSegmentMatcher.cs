using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Segments;

internal sealed class FeatureSegmentMatcher
{
    private readonly IReadOnlyDictionary<string, FeatureSegment> _segments;

    public FeatureSegmentMatcher(IReadOnlyDictionary<string, FeatureSegment> segments)
    {
        _segments = segments;
    }

    public bool Matches(string segmentKey, FeatureEvaluationContext context)
    {
        return MatchesCore(segmentKey, context, []);
    }

    private bool MatchesCore(string segmentKey, FeatureEvaluationContext context, HashSet<string> segmentStack)
    {
        if (!_segments.TryGetValue(segmentKey, out var segment) || !segmentStack.Add(segmentKey))
        {
            return false;
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(context.TargetingKey))
            {
                if (segment.ExcludedKeys.Contains(context.TargetingKey, StringComparer.Ordinal))
                {
                    return false;
                }

                if (segment.IncludedKeys.Contains(context.TargetingKey, StringComparer.Ordinal))
                {
                    return true;
                }
            }

            return MatchesAnyRule(segment, context, segmentStack);
        }
        finally
        {
            segmentStack.Remove(segmentKey);
        }
    }

    private bool MatchesAnyRule(FeatureSegment segment, FeatureEvaluationContext context, HashSet<string> segmentStack)
    {
        foreach (var rule in segment.Rules)
        {
            if (rule.Matches(segment.Key, context, (key, targetContext) => MatchesCore(key, targetContext, segmentStack)))
            {
                return true;
            }
        }

        return false;
    }
}

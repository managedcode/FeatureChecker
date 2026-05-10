namespace ManagedCode.FeatureChecker.Targeting;

public sealed record FeatureRuleMatch
{
    public string? RuleName { get; init; }

    public int RuleIndex { get; init; } = -1;

    public string? SegmentKey { get; init; }
}

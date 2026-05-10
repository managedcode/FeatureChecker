using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Evaluation;

public sealed record FeatureVariationDetail<T>
{
    public string Key { get; init; } = string.Empty;

    public T Value { get; init; } = default!;

    public int? VariationIndex { get; init; }

    public FeatureEvaluationReasonKind ReasonKind { get; init; }

    public string Reason { get; init; } = FeatureEvaluationReasons.Default;

    public FeatureRuleMatch? Rule { get; init; }

    public bool Exists { get; init; }
}

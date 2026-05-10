using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Evaluation;

public sealed record FeatureEvaluation
{
    public string Key { get; init; } = string.Empty;

    public bool Exists { get; init; }

    public FeatureStatus Status { get; init; }

    public string? Variant { get; init; }

    public string? Value { get; init; }

    public string Reason { get; init; } = FeatureEvaluationReasons.Default;

    public FeatureEvaluationReasonKind ReasonKind { get; init; } = FeatureEvaluationReasonKind.Default;

    public int? VariationIndex { get; init; }

    public FeatureRuleMatch? Rule { get; init; }

    public bool IsEnabled => Status == FeatureStatus.Enabled;

    public bool IsDisabled => Status == FeatureStatus.Disabled;

    public bool IsDebug => Status == FeatureStatus.Debug;
}

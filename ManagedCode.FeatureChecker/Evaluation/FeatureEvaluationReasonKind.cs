namespace ManagedCode.FeatureChecker.Evaluation;

public enum FeatureEvaluationReasonKind
{
    Default,
    Missing,
    Off,
    TargetMatch,
    RuleMatch,
    Fallthrough,
    Variant,
    Dependency,
    DependencyCycle,
    Error
}

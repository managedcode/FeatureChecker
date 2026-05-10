namespace ManagedCode.FeatureChecker.Evaluation;

public static class FeatureEvaluationReasons
{
    public const string Default = nameof(Default);
    public const string Off = nameof(Off);
    public const string Fallthrough = nameof(Fallthrough);
    public const string Target = nameof(Target);
    public const string Missing = nameof(Missing);
    public const string Rule = nameof(Rule);
    public const string Variant = nameof(Variant);
    public const string Dependency = nameof(Dependency);
    public const string DependencyCycle = nameof(DependencyCycle);
}

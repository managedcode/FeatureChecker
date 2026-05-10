using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Evaluation;

public interface IFeatureEvaluator
{
    int Count { get; }

    bool IsFeatureExists(string featureKey);

    bool IsEnabled(string featureKey, FeatureEvaluationContext? context = null);

    bool IsDisabled(string featureKey, FeatureEvaluationContext? context = null);

    bool IsDebug(string featureKey, FeatureEvaluationContext? context = null);

    bool TryGetFeatureStatus(string featureKey, out FeatureStatus status);

    FeatureEvaluation Evaluate(string featureKey, FeatureEvaluationContext? context = null);

    IReadOnlyDictionary<string, FeatureEvaluation> EvaluateAll(FeatureEvaluationContext? context = null);

    IReadOnlyList<string> GetFeatureKeysByStatus(FeatureStatus status, FeatureEvaluationContext? context = null);
}

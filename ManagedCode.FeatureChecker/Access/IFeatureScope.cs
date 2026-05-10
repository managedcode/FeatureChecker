using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Evaluation;
using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Access;

public interface IFeatureScope
{
    FeatureEvaluationContext Context { get; }

    int Count { get; }

    bool IsFeatureExists(string featureKey);

    bool IsEnabled(string featureKey);

    bool IsDisabled(string featureKey);

    bool IsDebug(string featureKey);

    bool TryGetFeatureStatus(string featureKey, out FeatureStatus status);

    FeatureEvaluation Evaluate(string featureKey);

    IReadOnlyDictionary<string, FeatureEvaluation> EvaluateAll();

    IReadOnlyList<string> GetFeatureKeysByStatus(FeatureStatus status);

    string GetStringValue(string featureKey, string defaultValue);

    bool GetBooleanValue(string featureKey, bool defaultValue);

    int GetInt32Value(string featureKey, int defaultValue);

    long GetInt64Value(string featureKey, long defaultValue);

    double GetDoubleValue(string featureKey, double defaultValue);
}

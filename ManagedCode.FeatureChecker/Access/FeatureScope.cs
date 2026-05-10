using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Evaluation;
using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Access;

public sealed class FeatureScope : IFeatureScope
{
    private readonly IFeatureEvaluator _evaluator;

    public FeatureScope(IFeatureEvaluator evaluator, FeatureEvaluationContext? context = null)
    {
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        Context = context ?? FeatureEvaluationContext.Empty;
    }

    public FeatureEvaluationContext Context { get; }

    public int Count => _evaluator.Count;

    public bool IsFeatureExists(string featureKey)
    {
        return _evaluator.IsFeatureExists(featureKey);
    }

    public bool IsEnabled(string featureKey)
    {
        return _evaluator.IsEnabled(featureKey, Context);
    }

    public bool IsDisabled(string featureKey)
    {
        return _evaluator.IsDisabled(featureKey, Context);
    }

    public bool IsDebug(string featureKey)
    {
        return _evaluator.IsDebug(featureKey, Context);
    }

    public bool TryGetFeatureStatus(string featureKey, out FeatureStatus status)
    {
        var evaluation = Evaluate(featureKey);
        status = evaluation.Status;

        return evaluation.Exists;
    }

    public FeatureEvaluation Evaluate(string featureKey)
    {
        return _evaluator.Evaluate(featureKey, Context);
    }

    public IReadOnlyDictionary<string, FeatureEvaluation> EvaluateAll()
    {
        return _evaluator.EvaluateAll(Context);
    }

    public IReadOnlyList<string> GetFeatureKeysByStatus(FeatureStatus status)
    {
        return _evaluator.GetFeatureKeysByStatus(status, Context);
    }

    public string GetStringValue(string featureKey, string defaultValue)
    {
        return _evaluator.GetStringValue(featureKey, defaultValue, Context);
    }

    public bool GetBooleanValue(string featureKey, bool defaultValue)
    {
        return _evaluator.GetBooleanValue(featureKey, defaultValue, Context);
    }

    public int GetInt32Value(string featureKey, int defaultValue)
    {
        return _evaluator.GetInt32Value(featureKey, defaultValue, Context);
    }

    public long GetInt64Value(string featureKey, long defaultValue)
    {
        return _evaluator.GetInt64Value(featureKey, defaultValue, Context);
    }

    public double GetDoubleValue(string featureKey, double defaultValue)
    {
        return _evaluator.GetDoubleValue(featureKey, defaultValue, Context);
    }
}

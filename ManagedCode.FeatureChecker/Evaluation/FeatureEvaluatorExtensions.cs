using System.Globalization;
using ManagedCode.FeatureChecker.Access;
using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Evaluation;

public static class FeatureEvaluatorExtensions
{
    public static IFeatureScope CreateScope(this IFeatureEvaluator evaluator, FeatureEvaluationContext? context = null)
    {
        return new FeatureScope(evaluator, context);
    }

    public static string GetStringValue(this IFeatureEvaluator evaluator, string featureKey, string defaultValue, FeatureEvaluationContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        return evaluator.Evaluate(featureKey, context).Value ?? defaultValue;
    }

    public static bool GetBooleanValue(this IFeatureEvaluator evaluator, string featureKey, bool defaultValue, FeatureEvaluationContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        var value = evaluator.Evaluate(featureKey, context).Value;

        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    public static int GetInt32Value(this IFeatureEvaluator evaluator, string featureKey, int defaultValue, FeatureEvaluationContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        var value = evaluator.Evaluate(featureKey, context).Value;

        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
    }

    public static long GetInt64Value(this IFeatureEvaluator evaluator, string featureKey, long defaultValue, FeatureEvaluationContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        var value = evaluator.Evaluate(featureKey, context).Value;

        return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
    }

    public static double GetDoubleValue(this IFeatureEvaluator evaluator, string featureKey, double defaultValue, FeatureEvaluationContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        var value = evaluator.Evaluate(featureKey, context).Value;

        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
    }

    public static FeatureVariationDetail<string> StringVariationDetail(this IFeatureEvaluator evaluator, string featureKey, string defaultValue, FeatureEvaluationContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        return CreateDetail(evaluator.Evaluate(featureKey, context), evaluation => evaluation.Value ?? defaultValue);
    }

    public static FeatureVariationDetail<bool> BoolVariationDetail(this IFeatureEvaluator evaluator, string featureKey, bool defaultValue, FeatureEvaluationContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        return CreateDetail(evaluator.Evaluate(featureKey, context), evaluation => bool.TryParse(evaluation.Value, out var result) ? result : defaultValue);
    }

    public static FeatureVariationDetail<int> IntVariationDetail(this IFeatureEvaluator evaluator, string featureKey, int defaultValue, FeatureEvaluationContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        return CreateDetail(
            evaluator.Evaluate(featureKey, context),
            evaluation => int.TryParse(evaluation.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : defaultValue);
    }

    public static FeatureVariationDetail<double> DoubleVariationDetail(this IFeatureEvaluator evaluator, string featureKey, double defaultValue, FeatureEvaluationContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        return CreateDetail(
            evaluator.Evaluate(featureKey, context),
            evaluation => double.TryParse(evaluation.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : defaultValue);
    }

    private static FeatureVariationDetail<T> CreateDetail<T>(FeatureEvaluation evaluation, Func<FeatureEvaluation, T> resolveValue)
    {
        return new FeatureVariationDetail<T>
        {
            Key = evaluation.Key,
            Value = resolveValue(evaluation),
            VariationIndex = evaluation.VariationIndex,
            ReasonKind = evaluation.ReasonKind,
            Reason = evaluation.Reason,
            Rule = evaluation.Rule,
            Exists = evaluation.Exists
        };
    }
}

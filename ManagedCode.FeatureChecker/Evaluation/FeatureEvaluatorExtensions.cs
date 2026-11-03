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

        return GetExistingValue(evaluator.Evaluate(featureKey, context)) ?? defaultValue;
    }

    public static bool GetBooleanValue(this IFeatureEvaluator evaluator, string featureKey, bool defaultValue, FeatureEvaluationContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        var value = GetExistingValue(evaluator.Evaluate(featureKey, context));

        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    public static int GetInt32Value(this IFeatureEvaluator evaluator, string featureKey, int defaultValue, FeatureEvaluationContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        var value = GetExistingValue(evaluator.Evaluate(featureKey, context));

        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
    }

    public static long GetInt64Value(this IFeatureEvaluator evaluator, string featureKey, long defaultValue, FeatureEvaluationContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        var value = GetExistingValue(evaluator.Evaluate(featureKey, context));

        return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
    }

    public static double GetDoubleValue(this IFeatureEvaluator evaluator, string featureKey, double defaultValue, FeatureEvaluationContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        var value = GetExistingValue(evaluator.Evaluate(featureKey, context));

        return GetFiniteDouble(value, defaultValue);
    }

    public static FeatureVariationDetail<string> StringVariationDetail(this IFeatureEvaluator evaluator, string featureKey, string defaultValue, FeatureEvaluationContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        var evaluation = evaluator.Evaluate(featureKey, context);
        return CreateDetail(evaluation, GetExistingValue(evaluation) ?? defaultValue);
    }

    public static FeatureVariationDetail<bool> BoolVariationDetail(this IFeatureEvaluator evaluator, string featureKey, bool defaultValue, FeatureEvaluationContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        var evaluation = evaluator.Evaluate(featureKey, context);
        return CreateDetail(evaluation, bool.TryParse(GetExistingValue(evaluation), out var result) ? result : defaultValue);
    }

    public static FeatureVariationDetail<int> IntVariationDetail(this IFeatureEvaluator evaluator, string featureKey, int defaultValue, FeatureEvaluationContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        var evaluation = evaluator.Evaluate(featureKey, context);
        return CreateDetail(evaluation, int.TryParse(GetExistingValue(evaluation), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : defaultValue);
    }

    public static FeatureVariationDetail<double> DoubleVariationDetail(this IFeatureEvaluator evaluator, string featureKey, double defaultValue, FeatureEvaluationContext? context = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        var evaluation = evaluator.Evaluate(featureKey, context);
        return CreateDetail(evaluation, GetFiniteDouble(GetExistingValue(evaluation), defaultValue));
    }

    private static string? GetExistingValue(FeatureEvaluation evaluation) => evaluation.Exists ? evaluation.Value : null;

    private static double GetFiniteDouble(string? value, double defaultValue)
    {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) && double.IsFinite(result)
            ? result
            : defaultValue;
    }

    private static FeatureVariationDetail<T> CreateDetail<T>(FeatureEvaluation evaluation, T value)
    {
        return new FeatureVariationDetail<T>
        {
            Key = evaluation.Key,
            Value = value,
            VariationIndex = evaluation.VariationIndex,
            ReasonKind = evaluation.ReasonKind,
            Reason = evaluation.Reason,
            Rule = evaluation.Rule,
            Exists = evaluation.Exists
        };
    }
}

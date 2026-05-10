using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Evaluation;

internal static class FeatureEvaluationFactory
{
    public static FeatureEvaluation Missing(string key)
    {
        return Create(key, false, FeatureStatus.Disabled, null, null, FeatureEvaluationReasons.Missing, FeatureEvaluationReasonKind.Missing);
    }

    public static FeatureEvaluation DependencyCycle(string key)
    {
        return Create(key, true, FeatureStatus.Disabled, null, null, FeatureEvaluationReasons.DependencyCycle, FeatureEvaluationReasonKind.DependencyCycle);
    }

    public static FeatureEvaluation DependencyCycle(FeatureDefinition definition)
    {
        return Create(definition.Key, true, FeatureStatus.Disabled, null, definition.Value, FeatureEvaluationReasons.DependencyCycle, FeatureEvaluationReasonKind.DependencyCycle);
    }

    public static FeatureEvaluation DependencyBlocked(FeatureDefinition definition)
    {
        return Create(definition.Key, true, FeatureStatus.Disabled, null, definition.Value, FeatureEvaluationReasons.Dependency, FeatureEvaluationReasonKind.Dependency);
    }

    public static FeatureEvaluation Off(FeatureDefinition definition)
    {
        return string.IsNullOrWhiteSpace(definition.OffVariant)
            ? Create(definition.Key, true, FeatureStatus.Disabled, null, definition.OffValue ?? definition.Value, FeatureEvaluationReasons.Off, FeatureEvaluationReasonKind.Off)
            : Variation(definition, definition.OffVariant, definition.OffValue, FeatureEvaluationReasons.Off, FeatureEvaluationReasonKind.Off);
    }

    public static FeatureEvaluation Target(FeatureDefinition definition, FeatureTarget target)
    {
        return string.IsNullOrWhiteSpace(target.Variant)
            ? Create(definition.Key, true, target.Status, null, target.Value ?? definition.Value, FeatureEvaluationReasons.Target, FeatureEvaluationReasonKind.TargetMatch)
            : Variation(definition, target.Variant, target.Value, FeatureEvaluationReasons.Target, FeatureEvaluationReasonKind.TargetMatch);
    }

    public static FeatureEvaluation Rule(FeatureDefinition definition, FeatureTargetingRule rule, int ruleIndex)
    {
        var ruleMatch = new FeatureRuleMatch
        {
            RuleName = rule.Name,
            RuleIndex = ruleIndex
        };

        return string.IsNullOrWhiteSpace(rule.Variant)
            ? Create(definition.Key, true, rule.Status, null, rule.Value ?? definition.Value, FeatureEvaluationReasons.Rule, FeatureEvaluationReasonKind.RuleMatch, null, ruleMatch)
            : Variation(definition, rule.Variant, rule.Value, FeatureEvaluationReasons.Rule, FeatureEvaluationReasonKind.RuleMatch, ruleMatch);
    }

    public static FeatureEvaluation Fallthrough(FeatureDefinition definition, FeatureEvaluationContext context)
    {
        if (!string.IsNullOrWhiteSpace(definition.FallthroughVariant))
        {
            return Variation(definition, definition.FallthroughVariant, definition.FallthroughValue, FeatureEvaluationReasons.Fallthrough, FeatureEvaluationReasonKind.Fallthrough);
        }

        if (!string.IsNullOrWhiteSpace(context.TargetingKey))
        {
            var variantMatch = SelectVariant(definition, context.TargetingKey);
            if (variantMatch.Variant is not null)
            {
                return Create(
                    definition.Key,
                    true,
                    variantMatch.Variant.Status,
                    variantMatch.Variant.Name,
                    variantMatch.Variant.Value,
                    FeatureEvaluationReasons.Variant,
                    FeatureEvaluationReasonKind.Variant,
                    variantMatch.Index);
            }
        }

        return Create(definition.Key, true, definition.Status, null, definition.Value, FeatureEvaluationReasons.Default, FeatureEvaluationReasonKind.Default);
    }

    private static FeatureEvaluation Variation(FeatureDefinition definition, string variantName, string? value, string reason, FeatureEvaluationReasonKind reasonKind, FeatureRuleMatch? rule = null)
    {
        var variantMatch = FindVariant(definition, variantName);

        return variantMatch.Variant is null
            ? Create(definition.Key, true, definition.Status, variantName, value ?? definition.Value, reason, reasonKind, null, rule)
            : Create(definition.Key, true, variantMatch.Variant.Status, variantMatch.Variant.Name, value ?? variantMatch.Variant.Value, reason, reasonKind, variantMatch.Index, rule);
    }

    private static (FeatureVariant? Variant, int? Index) SelectVariant(FeatureDefinition definition, string targetingKey)
    {
        var variant = FeatureRollout.SelectVariant(definition.Key, targetingKey, definition.Variants);

        return variant is null
            ? (null, null)
            : (variant, FindVariantIndex(definition.Variants, variant.Name));
    }

    private static (FeatureVariant? Variant, int? Index) FindVariant(FeatureDefinition definition, string variantName)
    {
        var index = FindVariantIndex(definition.Variants, variantName);

        return index < 0 ? (null, null) : (definition.Variants[index], index);
    }

    private static int FindVariantIndex(IReadOnlyList<FeatureVariant> variants, string variantName)
    {
        for (var index = 0; index < variants.Count; index++)
        {
            if (string.Equals(variants[index].Name, variantName, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    private static FeatureEvaluation Create(
        string key,
        bool exists,
        FeatureStatus status,
        string? variant,
        string? value,
        string reason,
        FeatureEvaluationReasonKind reasonKind,
        int? variationIndex = null,
        FeatureRuleMatch? rule = null)
    {
        return new FeatureEvaluation
        {
            Key = key,
            Exists = exists,
            Status = status,
            Variant = variant,
            Value = value,
            Reason = reason,
            ReasonKind = reasonKind,
            VariationIndex = variationIndex,
            Rule = rule
        };
    }
}

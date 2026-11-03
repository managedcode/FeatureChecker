using System.Runtime.CompilerServices;
using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Evaluation;

internal static class FeatureEvaluationFactory
{
    private static readonly ConditionalWeakTable<FeatureDefinition, FeatureEvaluation> ConstantResults = new();
    private static readonly ConditionalWeakTable<FeatureDefinition, FeatureEvaluation?[]> TargetResults = new();
    private static readonly ConditionalWeakTable<FeatureDefinition, FeatureEvaluation?[]> RuleResults = new();

    public static FeatureEvaluation Constant(FeatureDefinition definition)
    {
        return ConstantResults.GetValue(definition, CreateConstantResult);
    }

    private static FeatureEvaluation CreateConstantResult(FeatureDefinition definition)
    {
        if (!definition.TargetingEnabled)
        {
            return Off(definition);
        }

        if (!string.IsNullOrWhiteSpace(definition.FallthroughVariant))
        {
            return Variation(definition, definition.FallthroughVariant, definition.FallthroughValue, definition.Status, FeatureEvaluationReasons.Fallthrough, FeatureEvaluationReasonKind.Fallthrough);
        }

        return Create(definition.Key, true, definition.Status, null, definition.Value, FeatureEvaluationReasons.Default, FeatureEvaluationReasonKind.Default);
    }

    public static FeatureEvaluation Missing(string key)
    {
        return Create(key, false, FeatureStatus.Disabled, null, null, FeatureEvaluationReasons.Missing, FeatureEvaluationReasonKind.Missing);
    }

    public static FeatureEvaluation DependencyCycle(FeatureDefinition definition)
    {
        return Create(definition.Key, true, FeatureStatus.Disabled, null, definition.Value, FeatureEvaluationReasons.DependencyCycle, FeatureEvaluationReasonKind.DependencyCycle);
    }

    public static FeatureEvaluation DependencyBlocked(FeatureDefinition definition)
    {
        return Create(definition.Key, true, FeatureStatus.Disabled, null, definition.Value, FeatureEvaluationReasons.Dependency, FeatureEvaluationReasonKind.Dependency);
    }

    private static FeatureEvaluation Off(FeatureDefinition definition)
    {
        return string.IsNullOrWhiteSpace(definition.OffVariant)
            ? Create(definition.Key, true, FeatureStatus.Disabled, null, definition.OffValue ?? definition.Value, FeatureEvaluationReasons.Off, FeatureEvaluationReasonKind.Off)
            : Variation(definition, definition.OffVariant, definition.OffValue, FeatureStatus.Disabled, FeatureEvaluationReasons.Off, FeatureEvaluationReasonKind.Off);
    }

    public static FeatureEvaluation Target(FeatureDefinition definition, int targetIndex)
    {
        var results = TargetResults.GetValue(definition, static item => new FeatureEvaluation?[item.Targets.Count]);
        ref var slot = ref results[targetIndex];
        if (Volatile.Read(ref slot) is { } cached)
        {
            return cached;
        }

        var evaluation = CreateTargetResult(definition, targetIndex);
        return Interlocked.CompareExchange(ref slot, evaluation, null) ?? evaluation;
    }

    private static FeatureEvaluation CreateTargetResult(FeatureDefinition definition, int targetIndex)
    {
        var target = definition.Targets[targetIndex];
        return string.IsNullOrWhiteSpace(target.Variant)
            ? Create(definition.Key, true, target.Status, null, target.Value ?? definition.Value, FeatureEvaluationReasons.Target, FeatureEvaluationReasonKind.TargetMatch)
            : Variation(definition, target.Variant, target.Value, target.Status, FeatureEvaluationReasons.Target, FeatureEvaluationReasonKind.TargetMatch);
    }

    public static FeatureEvaluation Rule(FeatureDefinition definition, int ruleIndex)
    {
        var results = RuleResults.GetValue(definition, static item => new FeatureEvaluation?[item.Rules.Count]);
        ref var slot = ref results[ruleIndex];
        if (Volatile.Read(ref slot) is { } cached)
        {
            return cached;
        }

        var evaluation = CreateRuleResult(definition, ruleIndex);
        return Interlocked.CompareExchange(ref slot, evaluation, null) ?? evaluation;
    }

    private static FeatureEvaluation CreateRuleResult(FeatureDefinition definition, int ruleIndex)
    {
        var rule = definition.Rules[ruleIndex];
        var ruleMatch = new FeatureRuleMatch
        {
            RuleName = rule.Name,
            RuleIndex = ruleIndex
        };

        return string.IsNullOrWhiteSpace(rule.Variant)
            ? Create(definition.Key, true, rule.Status, null, rule.Value ?? definition.Value, FeatureEvaluationReasons.Rule, FeatureEvaluationReasonKind.RuleMatch, null, ruleMatch)
            : Variation(definition, rule.Variant, rule.Value, rule.Status, FeatureEvaluationReasons.Rule, FeatureEvaluationReasonKind.RuleMatch, ruleMatch);
    }

    public static FeatureEvaluation Fallthrough(FeatureDefinition definition, FeatureEvaluationContext context)
    {
        if (!string.IsNullOrWhiteSpace(definition.FallthroughVariant)
            || definition.Variants.Count == 0
            || string.IsNullOrWhiteSpace(context.TargetingKey))
        {
            return Constant(definition);
        }

        var index = FeatureRollout.SelectVariantIndex(definition.Key, context.TargetingKey, definition.Variants);
        var variant = definition.Variants[index];
        return Create(
            definition.Key,
            true,
            variant.Status,
            variant.Name,
            variant.Value,
            FeatureEvaluationReasons.Variant,
            FeatureEvaluationReasonKind.Variant,
            index);
    }

    private static FeatureEvaluation Variation(
        FeatureDefinition definition,
        string variantName,
        string? value,
        FeatureStatus clauseStatus,
        string reason,
        FeatureEvaluationReasonKind reasonKind,
        FeatureRuleMatch? rule = null)
    {
        var variantMatch = FindVariant(definition, variantName);

        return variantMatch.Variant is null
            ? Create(definition.Key, true, clauseStatus, variantName, value ?? definition.Value, reason, reasonKind, null, rule)
            : Create(definition.Key, true, variantMatch.Variant.Status, variantMatch.Variant.Name, value ?? variantMatch.Variant.Value, reason, reasonKind, variantMatch.Index, rule);
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

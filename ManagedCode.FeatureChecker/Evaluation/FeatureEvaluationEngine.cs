using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Segments;
using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Evaluation;

internal sealed class FeatureEvaluationEngine
{
    private readonly IReadOnlyDictionary<string, FeatureDefinition> _features;
    private readonly FeatureSegmentMatcher _segmentMatcher;

    public FeatureEvaluationEngine(IReadOnlyDictionary<string, FeatureDefinition> features, IReadOnlyDictionary<string, FeatureSegment> segments)
    {
        _features = features;
        _segmentMatcher = new FeatureSegmentMatcher(segments);
    }

    public FeatureEvaluation Evaluate(string featureKey, FeatureEvaluationContext context)
    {
        return EvaluateCore(featureKey, context, []);
    }

    private FeatureEvaluation EvaluateCore(string featureKey, FeatureEvaluationContext context, HashSet<string> evaluationStack)
    {
        if (!_features.TryGetValue(featureKey, out var definition))
        {
            return FeatureEvaluationFactory.Missing(featureKey);
        }

        if (!evaluationStack.Add(featureKey))
        {
            return FeatureEvaluationFactory.DependencyCycle(featureKey);
        }

        try
        {
            return EvaluateDefinition(definition, context, evaluationStack);
        }
        finally
        {
            evaluationStack.Remove(featureKey);
        }
    }

    private FeatureEvaluation EvaluateDefinition(FeatureDefinition definition, FeatureEvaluationContext context, HashSet<string> evaluationStack)
    {
        if (!definition.TargetingEnabled)
        {
            return FeatureEvaluationFactory.Off(definition);
        }

        var dependencyFailure = EvaluateDependencies(definition, context, evaluationStack);
        if (dependencyFailure is not null)
        {
            return dependencyFailure;
        }

        var target = FindTarget(definition, context);
        if (target is not null)
        {
            return FeatureEvaluationFactory.Target(definition, target);
        }

        var matchedRule = FindRule(definition, context);
        if (matchedRule.Rule is not null)
        {
            return FeatureEvaluationFactory.Rule(definition, matchedRule.Rule, matchedRule.Index);
        }

        return FeatureEvaluationFactory.Fallthrough(definition, context);
    }

    private FeatureEvaluation? EvaluateDependencies(FeatureDefinition definition, FeatureEvaluationContext context, HashSet<string> evaluationStack)
    {
        foreach (var dependency in definition.Dependencies)
        {
            var evaluation = EvaluateCore(dependency.Key, context, evaluationStack);
            var statusMatches = evaluation.Exists && evaluation.Status == dependency.RequiredStatus;
            var variantMatches = dependency.RequiredVariant is null || string.Equals(evaluation.Variant, dependency.RequiredVariant, StringComparison.Ordinal);

            if (string.Equals(evaluation.Reason, FeatureEvaluationReasons.DependencyCycle, StringComparison.Ordinal))
            {
                return FeatureEvaluationFactory.DependencyCycle(definition);
            }

            if (!statusMatches || !variantMatches)
            {
                return FeatureEvaluationFactory.DependencyBlocked(definition);
            }
        }

        return null;
    }

    private (FeatureTargetingRule? Rule, int Index) FindRule(FeatureDefinition definition, FeatureEvaluationContext context)
    {
        for (var index = 0; index < definition.Rules.Count; index++)
        {
            var rule = definition.Rules[index];

            if (rule.Matches(definition.Key, context, _segmentMatcher.Matches))
            {
                return (rule, index);
            }
        }

        return (null, -1);
    }

    private static FeatureTarget? FindTarget(FeatureDefinition definition, FeatureEvaluationContext context)
    {
        if (string.IsNullOrWhiteSpace(context.TargetingKey))
        {
            return null;
        }

        return definition.Targets.FirstOrDefault(target =>
            string.Equals(target.Key, context.TargetingKey, StringComparison.Ordinal) &&
            (target.ContextKind is null || string.Equals(target.ContextKind, context.ContextKind, StringComparison.OrdinalIgnoreCase)));
    }
}

using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Segments;
using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Evaluation;

internal sealed class FeatureEvaluationEngine
{
    private readonly IReadOnlyDictionary<string, FeatureDefinition> _features;
    private readonly Func<string, FeatureEvaluationContext, bool?> _matchSegment;

    public FeatureEvaluationEngine(IReadOnlyDictionary<string, FeatureDefinition> features, IReadOnlyDictionary<string, FeatureSegment> segments)
    {
        _features = features;
        _matchSegment = new FeatureSegmentMatcher(segments).Matches;
    }

    public FeatureEvaluation Evaluate(string featureKey, FeatureEvaluationContext context)
    {
        if (!_features.TryGetValue(featureKey, out var definition))
        {
            return FeatureEvaluationFactory.Missing(featureKey);
        }

        Dictionary<string, FeatureEvaluation?>? results = null;
        return Evaluate(definition, context, ref results);
    }

    public FeatureEvaluation Evaluate(
        FeatureDefinition definition,
        FeatureEvaluationContext context,
        ref Dictionary<string, FeatureEvaluation?>? results)
    {
        if (results?.TryGetValue(definition.Key, out var result) is true)
        {
            return result!;
        }

        return EvaluateLeaf(definition, context)
            ?? EvaluateGraph(definition, context, results ??= new Dictionary<string, FeatureEvaluation?>(StringComparer.Ordinal));
    }

    private FeatureEvaluation? EvaluateLeaf(FeatureDefinition definition, FeatureEvaluationContext context)
    {
        if (!definition.TargetingEnabled)
        {
            return FeatureEvaluationFactory.Constant(definition);
        }

        return definition.Dependencies.Count == 0 ? EvaluateDefinition(definition, context) : null;
    }

    private FeatureEvaluation EvaluateGraph(FeatureDefinition root, FeatureEvaluationContext context, Dictionary<string, FeatureEvaluation?> results)
    {
        // A null entry marks a feature on the current traversal path.
        results.Add(root.Key, null);
        var pending = new Stack<DependencyFrame>();
        pending.Push(new DependencyFrame(root, 0));

        while (pending.TryPop(out var frame))
        {
            var definition = frame.Definition;
            if (frame.Index == definition.Dependencies.Count)
            {
                results[definition.Key] = EvaluateDefinition(definition, context);
                continue;
            }

            var dependency = definition.Dependencies[frame.Index];
            if (!results.TryGetValue(dependency.Key, out var result))
            {
                if (!TryEvaluateDependency(dependency.Key, context, out var child, out result))
                {
                    results.Add(dependency.Key, null);
                    pending.Push(frame);
                    pending.Push(new DependencyFrame(child!, 0));
                    continue;
                }

                results.Add(dependency.Key, result);
            }

            var failure = GetDependencyFailure(definition, dependency, result);
            if (failure is null)
            {
                pending.Push(new DependencyFrame(definition, frame.Index + 1));
            }
            else
            {
                results[definition.Key] = failure;
            }
        }

        return results[root.Key]!;
    }

    private FeatureEvaluation EvaluateDefinition(FeatureDefinition definition, FeatureEvaluationContext context)
    {
        if (definition.Targets.Count == 0 && definition.Rules.Count == 0 && definition.Variants.Count == 0)
        {
            return FeatureEvaluationFactory.Constant(definition);
        }

        var targetIndex = FindTargetIndex(definition, context);
        if (targetIndex >= 0)
        {
            return FeatureEvaluationFactory.Target(definition, targetIndex);
        }

        var ruleIndex = FindRuleIndex(definition, context);
        if (ruleIndex >= 0)
        {
            return FeatureEvaluationFactory.Rule(definition, ruleIndex);
        }

        return FeatureEvaluationFactory.Fallthrough(definition, context);
    }

    private bool TryEvaluateDependency(string key, FeatureEvaluationContext context, out FeatureDefinition? definition, out FeatureEvaluation? result)
    {
        result = _features.TryGetValue(key, out definition)
            ? EvaluateLeaf(definition, context)
            : FeatureEvaluationFactory.Missing(key);

        return result is not null;
    }

    private static FeatureEvaluation? GetDependencyFailure(FeatureDefinition definition, FeatureDependency dependency, FeatureEvaluation? evaluation)
    {
        if (evaluation is null || evaluation.ReasonKind == FeatureEvaluationReasonKind.DependencyCycle)
        {
            return FeatureEvaluationFactory.DependencyCycle(definition);
        }

        var statusMatches = evaluation.Exists && evaluation.Status == dependency.RequiredStatus;
        var variantMatches = dependency.RequiredVariant is null || string.Equals(evaluation.Variant, dependency.RequiredVariant, StringComparison.Ordinal);
        return statusMatches && variantMatches ? null : FeatureEvaluationFactory.DependencyBlocked(definition);
    }

    private int FindRuleIndex(FeatureDefinition definition, FeatureEvaluationContext context)
    {
        for (var index = 0; index < definition.Rules.Count; index++)
        {
            var rule = definition.Rules[index];

            if (rule.Matches(definition.Key, context, _matchSegment))
            {
                return index;
            }
        }

        return -1;
    }

    private static int FindTargetIndex(FeatureDefinition definition, FeatureEvaluationContext context)
    {
        if (string.IsNullOrWhiteSpace(context.TargetingKey))
        {
            return -1;
        }

        for (var index = 0; index < definition.Targets.Count; index++)
        {
            var target = definition.Targets[index];
            if (string.Equals(target.Key, context.TargetingKey, StringComparison.Ordinal)
                && (target.ContextKind is null || string.Equals(target.ContextKind, context.ContextKind, StringComparison.OrdinalIgnoreCase)))
            {
                return index;
            }
        }

        return -1;
    }

    private readonly record struct DependencyFrame(FeatureDefinition Definition, int Index);
}

using System.Diagnostics.CodeAnalysis;
using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Segments;

internal sealed class FeatureSegmentMatcher(IReadOnlyDictionary<string, FeatureSegment> segments)
{
    public bool? Matches(string segmentKey, FeatureEvaluationContext context)
    {
        if (TryResolve(segmentKey, context, out var frame, out var result)
            || !frame.TryMoveNext(context, out var nextKey, out result))
        {
            return result;
        }

        return EvaluateGraph(frame, nextKey, context);
    }

    private bool? EvaluateGraph(SegmentFrame frame, string nextKey, FeatureEvaluationContext context)
    {
        // A null entry marks a segment on the current traversal path.
        var results = new Dictionary<string, bool?>(StringComparer.Ordinal) { [frame.Key] = null };
        HashSet<string>? unresolved = null;
        var pending = new Stack<SegmentFrame>();
        while (true)
        {
            if (results.TryGetValue(nextKey, out var match) || unresolved?.Contains(nextKey) is true)
            {
                frame.Accept(match);
            }
            else if (TryResolve(nextKey, context, out var child, out match))
            {
                results.Add(nextKey, match);
                frame.Accept(match);
            }
            else
            {
                results.Add(nextKey, null);
                pending.Push(frame);
                frame = child;
            }

            while (!frame.TryMoveNext(context, out nextKey!, out var result))
            {
                SaveResult(results, frame.Key, result, ref unresolved);
                if (!pending.TryPop(out frame))
                {
                    return result;
                }

                frame.Accept(result);
            }
        }
    }

    private static void SaveResult(
        Dictionary<string, bool?> results,
        string key,
        bool? result,
        ref HashSet<string>? unresolved)
    {
        if (result.HasValue)
        {
            results[key] = result;
            // An earlier unknown can become definite once an active segment is resolved.
            unresolved?.Clear();
        }
        else
        {
            results.Remove(key);
            (unresolved ??= new HashSet<string>(StringComparer.Ordinal)).Add(key);
        }
    }

    private bool TryResolve(string key, FeatureEvaluationContext context, out SegmentFrame frame, out bool? result)
    {
        frame = default;
        if (!segments.TryGetValue(key, out var segment))
        {
            result = false;
            return true;
        }

        result = MatchTarget(segment, context.TargetingKey);
        if (result.HasValue)
        {
            return true;
        }

        frame = new SegmentFrame(segment);
        return false;
    }

    private static bool? MatchTarget(FeatureSegment segment, string? targetingKey)
    {
        if (string.IsNullOrWhiteSpace(targetingKey))
        {
            return null;
        }

        if (ContainsKey(segment.ExcludedKeys, targetingKey))
        {
            return false;
        }

        return ContainsKey(segment.IncludedKeys, targetingKey) ? true : null;
    }

    private static bool ContainsKey(IReadOnlyList<string> keys, string key)
    {
        for (var index = 0; index < keys.Count; index++)
        {
            if (string.Equals(keys[index], key, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private struct SegmentFrame(FeatureSegment segment)
    {
        private int _ruleIndex;
        private bool _ruleStarted;
        private bool _hasUnknownRule;
        private SegmentRuleMatch _segments;

        public readonly string Key => segment.Key;

        public void Accept(bool? match) => _segments.Accept(match);

        public bool TryMoveNext(FeatureEvaluationContext context, [NotNullWhen(true)] out string? segmentKey, out bool? result)
        {
            while (_ruleIndex < segment.Rules.Count)
            {
                var rule = segment.Rules[_ruleIndex];
                if (!_ruleStarted && !StartRule(rule, context))
                {
                    _ruleIndex++;
                    continue;
                }

                if (_segments.TryMoveNext(out segmentKey))
                {
                    result = null;
                    return true;
                }

                var ruleResult = _segments.Result;
                if (ruleResult is true)
                {
                    result = true;
                    return false;
                }

                _hasUnknownRule |= ruleResult is null;
                _ruleStarted = false;
                _ruleIndex++;
            }

            segmentKey = null;
            result = _hasUnknownRule ? null : false;
            return false;
        }

        private bool StartRule(FeatureTargetingRule rule, FeatureEvaluationContext context)
        {
            if (!rule.MatchesConditions(segment.Key, context))
            {
                return false;
            }

            _segments = new SegmentRuleMatch(rule);
            _ruleStarted = true;
            return true;
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Segments;

internal struct SegmentRuleMatch(FeatureTargetingRule rule)
{
    private int _index;
    private bool? _excluded = false;
    private bool? _included = rule.IncludeSegments.Count == 0;

    public readonly bool? Result => !_excluded & _included;

    public bool TryMoveNext([NotNullWhen(true)] out string? segmentKey)
    {
        if (_excluded is not true)
        {
            if (_index < rule.ExcludeSegments.Count)
            {
                segmentKey = rule.ExcludeSegments[_index++];
                return true;
            }

            var includeIndex = _index - rule.ExcludeSegments.Count;
            if (_included is not true && includeIndex < rule.IncludeSegments.Count)
            {
                segmentKey = rule.IncludeSegments[includeIndex];
                _index++;
                return true;
            }
        }

        segmentKey = null;
        return false;
    }

    public void Accept(bool? match)
    {
        if (_index <= rule.ExcludeSegments.Count)
        {
            _excluded |= match;
        }
        else
        {
            _included |= match;
        }
    }
}

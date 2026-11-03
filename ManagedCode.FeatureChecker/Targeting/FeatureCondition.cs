using System.Globalization;

namespace ManagedCode.FeatureChecker.Targeting;

public sealed record FeatureCondition
{
    public string Attribute { get; init; } = string.Empty;

    public FeatureConditionOperator Operator { get; init; } = FeatureConditionOperator.Equals;

    public IReadOnlyList<string> Values { get; init => field = FeatureCollections.Freeze(value); } = [];

    public static FeatureCondition Equals(string attribute, string value)
    {
        return Create(attribute, FeatureConditionOperator.Equals, value);
    }

    public static FeatureCondition NotEquals(string attribute, string value)
    {
        return Create(attribute, FeatureConditionOperator.NotEquals, value);
    }

    public static FeatureCondition Contains(string attribute, string value)
    {
        return Create(attribute, FeatureConditionOperator.Contains, value);
    }

    public static FeatureCondition StartsWith(string attribute, string value)
    {
        return Create(attribute, FeatureConditionOperator.StartsWith, value);
    }

    public static FeatureCondition EndsWith(string attribute, string value)
    {
        return Create(attribute, FeatureConditionOperator.EndsWith, value);
    }

    public static FeatureCondition VersionAtLeast(string attribute, string minimumVersion)
    {
        return Create(attribute, FeatureConditionOperator.VersionAtLeast, minimumVersion);
    }

    public static FeatureCondition In(string attribute, params string[] values)
    {
        return Create(attribute, FeatureConditionOperator.In, values);
    }

    public static FeatureCondition NotIn(string attribute, params string[] values)
    {
        return Create(attribute, FeatureConditionOperator.NotIn, values);
    }

    public static FeatureCondition Exists(string attribute)
    {
        return Create(attribute, FeatureConditionOperator.Exists);
    }

    public static FeatureCondition NotExists(string attribute)
    {
        return Create(attribute, FeatureConditionOperator.NotExists);
    }

    internal bool Matches(FeatureEvaluationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.TryGetAttribute(Attribute, out var actualValue)
            ? MatchesAttributeValue(actualValue)
            : MatchesMissingAttribute();
    }

    private bool MatchesMissingAttribute()
    {
        return Operator is FeatureConditionOperator.NotExists or FeatureConditionOperator.NotEquals or FeatureConditionOperator.NotIn;
    }

    private bool MatchesAttributeValue(string actualValue)
    {
        if (Operator is FeatureConditionOperator.Exists)
        {
            return true;
        }

        if (Operator is FeatureConditionOperator.NotExists)
        {
            return false;
        }

        return MatchesComparisonValue(actualValue) ?? MatchesTextValue(actualValue);
    }

    private bool? MatchesComparisonValue(string actualValue)
    {
        return Operator switch
        {
            FeatureConditionOperator.Equals or FeatureConditionOperator.In => ValuesContainMatch(actualValue, MatchesValue),
            FeatureConditionOperator.NotEquals or FeatureConditionOperator.NotIn => !ValuesContainMatch(actualValue, MatchesValue),
            _ => null
        };
    }

    private bool MatchesTextValue(string actualValue)
    {
        return Operator switch
        {
            FeatureConditionOperator.Contains => ValuesContainMatch(actualValue, static (actual, expected) => actual.Contains(expected, StringComparison.OrdinalIgnoreCase)),
            FeatureConditionOperator.StartsWith => ValuesContainMatch(actualValue, static (actual, expected) => actual.StartsWith(expected, StringComparison.OrdinalIgnoreCase)),
            FeatureConditionOperator.EndsWith => ValuesContainMatch(actualValue, static (actual, expected) => actual.EndsWith(expected, StringComparison.OrdinalIgnoreCase)),
            FeatureConditionOperator.VersionAtLeast => ValuesContainMatch(actualValue, IsVersionAtLeast),
            _ => false
        };
    }

    private static FeatureCondition Create(string attribute, FeatureConditionOperator conditionOperator, params string[] values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(attribute);

        return new FeatureCondition
        {
            Attribute = attribute,
            Operator = conditionOperator,
            Values = values
        };
    }

    private static bool MatchesValue(string actualValue, string expectedValue)
    {
        return string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsVersionAtLeast(string actualValue, string expectedValue)
    {
        var actual = GetVersionCore(actualValue);
        var expected = GetVersionCore(expectedValue);
        var actualParts = actual.Split('.');
        var expectedParts = expected.Split('.');
        var comparison = 0;

        while (true)
        {
            var hasActual = actualParts.MoveNext();
            var hasExpected = expectedParts.MoveNext();
            if (!hasActual && !hasExpected)
            {
                return comparison >= 0;
            }

            var actualNumber = 0;
            var expectedNumber = 0;
            if ((hasActual && !TryParseVersionPart(actual[actualParts.Current], out actualNumber))
                || (hasExpected && !TryParseVersionPart(expected[expectedParts.Current], out expectedNumber)))
            {
                return false;
            }

            if (comparison == 0)
            {
                comparison = actualNumber.CompareTo(expectedNumber);
            }
        }
    }

    private static ReadOnlySpan<char> GetVersionCore(string value)
    {
        var span = value.AsSpan();
        var suffixStart = span.IndexOfAny('-', '+');
        return suffixStart < 0 ? span : span[..suffixStart];
    }

    private static bool TryParseVersionPart(ReadOnlySpan<char> part, out int value)
    {
        return int.TryParse(part.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }

    private bool ValuesContainMatch(string actualValue, Func<string, string, bool> predicate)
    {
        for (var index = 0; index < Values.Count; index++)
        {
            if (predicate(actualValue, Values[index]))
            {
                return true;
            }
        }

        return false;
    }

}

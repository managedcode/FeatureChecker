using System.Globalization;

namespace ManagedCode.FeatureChecker.Targeting;

public sealed record FeatureCondition
{
    public string Attribute { get; init; } = string.Empty;

    public FeatureConditionOperator Operator { get; init; } = FeatureConditionOperator.Equals;

    public IReadOnlyList<string> Values { get; init; } = [];

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
            FeatureConditionOperator.Equals => ValuesContainMatch(actualValue, MatchesValue),
            FeatureConditionOperator.In => ValuesContainMatch(actualValue, MatchesValue),
            FeatureConditionOperator.NotEquals => ValuesAllMatch(actualValue, static (actual, expected) => !MatchesValue(actual, expected)),
            FeatureConditionOperator.NotIn => ValuesAllMatch(actualValue, static (actual, expected) => !MatchesValue(actual, expected)),
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
            FeatureConditionOperator.VersionAtLeast => ValuesContainMatch(actualValue, static (actual, expected) => CompareVersion(actual, expected) >= 0),
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
            Values = values.ToList()
        };
    }

    private static bool MatchesValue(string actualValue, string expectedValue)
    {
        return string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase);
    }

    private static int CompareVersion(string actualValue, string expectedValue)
    {
        var actualParts = ParseVersion(actualValue);
        var expectedParts = ParseVersion(expectedValue);
        var length = Math.Max(actualParts.Count, expectedParts.Count);

        for (var index = 0; index < length; index++)
        {
            var actual = index < actualParts.Count ? actualParts[index] : 0;
            var expected = index < expectedParts.Count ? expectedParts[index] : 0;
            var comparison = actual.CompareTo(expected);

            if (comparison != 0)
            {
                return comparison;
            }
        }

        return 0;
    }

    private static IReadOnlyList<int> ParseVersion(string value)
    {
        var stableValue = value.Split(['-', '+'], 2, StringSplitOptions.TrimEntries)[0];
        var parts = stableValue.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = new List<int>(parts.Length);

        foreach (var part in parts)
        {
            if (!int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
            {
                return [];
            }

            result.Add(number);
        }

        return result;
    }

    private bool ValuesContainMatch(string actualValue, Func<string, string, bool> predicate)
    {
        foreach (var value in Values)
        {
            if (predicate(actualValue, value))
            {
                return true;
            }
        }

        return false;
    }

    private bool ValuesAllMatch(string actualValue, Func<string, string, bool> predicate)
    {
        foreach (var value in Values)
        {
            if (!predicate(actualValue, value))
            {
                return false;
            }
        }

        return true;
    }
}

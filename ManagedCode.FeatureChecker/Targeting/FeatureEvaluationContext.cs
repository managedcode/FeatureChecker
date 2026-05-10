using System.Globalization;

namespace ManagedCode.FeatureChecker.Targeting;

public sealed class FeatureEvaluationContext
{
    private static readonly IReadOnlyDictionary<string, string> EmptyAttributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public FeatureEvaluationContext(string? targetingKey = null, IReadOnlyDictionary<string, string>? attributes = null, string contextKind = FeatureContextKinds.User)
    {
        TargetingKey = targetingKey;
        ContextKind = string.IsNullOrWhiteSpace(contextKind) ? FeatureContextKinds.User : contextKind;
        Attributes = attributes is null
            ? EmptyAttributes
            : new Dictionary<string, string>(attributes, StringComparer.OrdinalIgnoreCase);
    }

    public static FeatureEvaluationContext Empty { get; } = new();

    public string? TargetingKey { get; }

    public string ContextKind { get; }

    public IReadOnlyDictionary<string, string> Attributes { get; }

    public static FeatureEvaluationContext ForTargetingKey(string targetingKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetingKey);

        return new FeatureEvaluationContext(targetingKey);
    }

    public FeatureEvaluationContext With(string name, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var attributes = new Dictionary<string, string>(Attributes, StringComparer.OrdinalIgnoreCase)
        {
            [name] = ConvertAttributeValue(value)
        };

        return new FeatureEvaluationContext(TargetingKey, attributes, ContextKind);
    }

    public FeatureEvaluationContext WithTargetingKey(string targetingKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetingKey);

        return new FeatureEvaluationContext(targetingKey, Attributes, ContextKind);
    }

    public FeatureEvaluationContext WithContextKind(string contextKind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contextKind);

        return new FeatureEvaluationContext(TargetingKey, Attributes, contextKind);
    }

    public bool TryGetAttribute(string name, out string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return Attributes.TryGetValue(name, out value!);
    }

    private static string ConvertAttributeValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            string text => text,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }
}

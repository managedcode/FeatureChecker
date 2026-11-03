using System.Collections.ObjectModel;
using System.Globalization;

namespace ManagedCode.FeatureChecker.Targeting;

public sealed class FeatureEvaluationContext
{
    private static readonly IReadOnlyDictionary<string, string> EmptyAttributes = ReadOnlyDictionary<string, string>.Empty;

    public FeatureEvaluationContext(string? targetingKey = null, IReadOnlyDictionary<string, string>? attributes = null, string contextKind = FeatureContextKinds.User)
        : this(targetingKey, contextKind, CopyAttributes(attributes))
    {
    }

    private FeatureEvaluationContext(string? targetingKey, string contextKind, IReadOnlyDictionary<string, string> attributes)
    {
        TargetingKey = targetingKey;
        ContextKind = string.IsNullOrWhiteSpace(contextKind) ? FeatureContextKinds.User : contextKind;
        Attributes = attributes;
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

        return new FeatureEvaluationContext(TargetingKey, ContextKind, new ReadOnlyDictionary<string, string>(attributes));
    }

    public FeatureEvaluationContext WithTargetingKey(string targetingKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetingKey);

        return new FeatureEvaluationContext(targetingKey, ContextKind, Attributes);
    }

    public FeatureEvaluationContext WithContextKind(string contextKind)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contextKind);

        return new FeatureEvaluationContext(TargetingKey, contextKind, Attributes);
    }

    public bool TryGetAttribute(string name, out string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return Attributes.TryGetValue(name, out value!);
    }

    private static IReadOnlyDictionary<string, string> CopyAttributes(IReadOnlyDictionary<string, string>? attributes)
    {
        return attributes is null or { Count: 0 }
            ? EmptyAttributes
            : new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(attributes, StringComparer.OrdinalIgnoreCase));
    }

    internal static string ConvertAttributeValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            string text => text,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty,
            _ => value.ToString() ?? string.Empty
        };
    }
}

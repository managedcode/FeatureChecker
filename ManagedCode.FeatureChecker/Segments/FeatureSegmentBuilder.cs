using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Segments;

public sealed class FeatureSegmentBuilder
{
    private readonly string _key;
    private readonly List<string> _includedKeys = [];
    private readonly List<string> _excludedKeys = [];
    private readonly List<FeatureTargetingRule> _rules = [];
    private readonly Dictionary<string, string> _metadata = new(StringComparer.OrdinalIgnoreCase);
    private string? _description;

    public FeatureSegmentBuilder(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _key = key;
    }

    public FeatureSegmentBuilder Describe(string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        _description = description;

        return this;
    }

    public FeatureSegmentBuilder Include(params string[] targetingKeys)
    {
        AddKeys(_includedKeys, targetingKeys);

        return this;
    }

    public FeatureSegmentBuilder Exclude(params string[] targetingKeys)
    {
        AddKeys(_excludedKeys, targetingKeys);

        return this;
    }

    public FeatureSegmentBuilder WhenAll(IEnumerable<FeatureCondition> conditions, double? percentage = null)
    {
        ArgumentNullException.ThrowIfNull(conditions);

        _rules.Add(new FeatureTargetingRule
        {
            Conditions = conditions.ToList(),
            Percentage = percentage
        });

        return this;
    }

    public FeatureSegmentBuilder WithMetadata(string name, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _metadata[name] = value;

        return this;
    }

    public FeatureSegment Build()
    {
        return new FeatureSegment
        {
            Key = _key,
            Description = _description,
            IncludedKeys = [.. _includedKeys],
            ExcludedKeys = [.. _excludedKeys],
            Rules = [.. _rules],
            Metadata = new Dictionary<string, string>(_metadata, StringComparer.OrdinalIgnoreCase)
        };
    }

    private static void AddKeys(List<string> target, IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(keys));
            target.Add(key);
        }
    }
}

using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Definitions;

public sealed class FeatureDefinitionBuilder : IFeatureDefinitionBuilder
{
    private readonly string _key;
    private readonly List<FeatureTargetingRule> _rules = [];
    private readonly List<FeatureDependency> _dependencies = [];
    private readonly List<FeatureVariant> _variants = [];
    private readonly List<FeatureTarget> _targets = [];
    private readonly Dictionary<string, string> _metadata = new(StringComparer.OrdinalIgnoreCase);
    private FeatureStatus _status;
    private FeatureMode _mode = FeatureMode.Release;
    private bool _targetingEnabled = true;
    private string? _description;
    private string? _value;
    private string? _fallthroughVariant;
    private string? _fallthroughValue;
    private string? _offVariant;
    private string? _offValue;

    public FeatureDefinitionBuilder(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _key = key;
    }

    public IFeatureDefinitionBuilder Enabled(string? value = null) => WithStatus(FeatureStatus.Enabled, value);

    public IFeatureDefinitionBuilder Disabled(string? value = null) => WithStatus(FeatureStatus.Disabled, value);

    public IFeatureDefinitionBuilder Debug(string? value = null) => WithStatus(FeatureStatus.Debug, value);

    public IFeatureDefinitionBuilder Describe(string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        _description = description;

        return this;
    }

    public IFeatureDefinitionBuilder WithMetadata(string name, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _metadata[name] = value;

        return this;
    }

    public IFeatureDefinitionBuilder Mode(FeatureMode mode) { _mode = mode; return this; }

    public IFeatureDefinitionBuilder Targeting(bool enabled) { _targetingEnabled = enabled; return this; }

    public IFeatureDefinitionBuilder OffVariation(string variant, string? value = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(variant);

        _offVariant = variant;
        _offValue = value;

        return this;
    }

    public IFeatureDefinitionBuilder FallthroughVariation(string variant, string? value = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(variant);

        _fallthroughVariant = variant;
        _fallthroughValue = value;

        return this;
    }

    public IFeatureDefinitionBuilder Target(string targetingKey, FeatureStatus status = FeatureStatus.Enabled, string? variant = null, string? value = null, string? contextKind = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetingKey);

        _targets.Add(new FeatureTarget
        {
            Key = targetingKey,
            ContextKind = contextKind,
            Status = status,
            Variant = variant,
            Value = value
        });

        return this;
    }

    public IFeatureDefinitionBuilder Require(string key, FeatureStatus requiredStatus = FeatureStatus.Enabled, string? requiredVariant = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _dependencies.Add(new FeatureDependency
        {
            Key = key,
            RequiredStatus = requiredStatus,
            RequiredVariant = requiredVariant
        });

        return this;
    }

    public IFeatureDefinitionBuilder WhenAttributeEquals(string attribute, string value, FeatureStatus status = FeatureStatus.Enabled, string? variant = null, string? featureValue = null) =>
        AddRule([FeatureCondition.Equals(attribute, value)], status, variant, featureValue);

    public IFeatureDefinitionBuilder WhenAll(IEnumerable<FeatureCondition> conditions, FeatureStatus status = FeatureStatus.Enabled, string? variant = null, string? featureValue = null, double? percentage = null)
    {
        ArgumentNullException.ThrowIfNull(conditions);

        return AddRule(conditions, status, variant, featureValue, percentage);
    }

    public IFeatureDefinitionBuilder WhenSegment(string segmentKey, FeatureStatus status = FeatureStatus.Enabled, string? variant = null, string? featureValue = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(segmentKey);

        return AddRule([], status, variant, featureValue, includeSegments: [segmentKey]);
    }

    public IFeatureDefinitionBuilder WhenApplicationVersionAtLeast(string minimumVersion, FeatureStatus status = FeatureStatus.Enabled, string? variant = null, string? featureValue = null) =>
        AddRule([FeatureCondition.VersionAtLeast(FeatureContextAttributeNames.ApplicationVersion, minimumVersion)], status, variant, featureValue);

    public IFeatureDefinitionBuilder Rollout(double percentage, FeatureStatus status = FeatureStatus.Enabled, string? variant = null, string? featureValue = null) =>
        AddRule([], status, variant, featureValue, percentage);

    public IFeatureDefinitionBuilder Variant(string name, double weight, FeatureStatus status = FeatureStatus.Enabled, string? value = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _variants.Add(new FeatureVariant
        {
            Name = name,
            Weight = weight,
            Status = status,
            Value = value
        });

        return this;
    }

    public FeatureDefinition Build()
    {
        return new FeatureDefinition
        {
            Key = _key,
            Status = _status,
            Mode = _mode,
            TargetingEnabled = _targetingEnabled,
            Description = _description,
            Value = _value,
            FallthroughVariant = _fallthroughVariant,
            FallthroughValue = _fallthroughValue,
            OffVariant = _offVariant,
            OffValue = _offValue,
            Targets = [.. _targets],
            Rules = [.. _rules],
            Dependencies = [.. _dependencies],
            Variants = [.. _variants],
            Metadata = new Dictionary<string, string>(_metadata, StringComparer.OrdinalIgnoreCase)
        };
    }

    private IFeatureDefinitionBuilder AddRule(
        IEnumerable<FeatureCondition> conditions,
        FeatureStatus status,
        string? variant,
        string? featureValue,
        double? percentage = null,
        IEnumerable<string>? includeSegments = null,
        IEnumerable<string>? excludeSegments = null)
    {
        _rules.Add(new FeatureTargetingRule
        {
            Status = status,
            Variant = variant,
            Value = featureValue,
            Percentage = percentage,
            IncludeSegments = includeSegments?.ToList() ?? [],
            ExcludeSegments = excludeSegments?.ToList() ?? [],
            Conditions = conditions.ToList()
        });

        return this;
    }

    private IFeatureDefinitionBuilder WithStatus(FeatureStatus status, string? value)
    {
        _status = status;
        _value = value;

        return this;
    }
}

using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Definitions;

public interface IFeatureDefinitionBuilder
{
    IFeatureDefinitionBuilder Enabled(string? value = null);

    IFeatureDefinitionBuilder Disabled(string? value = null);

    IFeatureDefinitionBuilder Debug(string? value = null);

    IFeatureDefinitionBuilder Describe(string description);

    IFeatureDefinitionBuilder WithMetadata(string name, string value);

    IFeatureDefinitionBuilder Mode(FeatureMode mode);

    IFeatureDefinitionBuilder Targeting(bool enabled);

    IFeatureDefinitionBuilder OffVariation(string variant, string? value = null);

    IFeatureDefinitionBuilder FallthroughVariation(string variant, string? value = null);

    IFeatureDefinitionBuilder Target(string targetingKey, FeatureStatus status = FeatureStatus.Enabled, string? variant = null, string? value = null, string? contextKind = null);

    IFeatureDefinitionBuilder Require(string key, FeatureStatus requiredStatus = FeatureStatus.Enabled, string? requiredVariant = null);

    IFeatureDefinitionBuilder WhenAttributeEquals(string attribute, string value, FeatureStatus status = FeatureStatus.Enabled, string? variant = null, string? featureValue = null);

    IFeatureDefinitionBuilder WhenAll(IEnumerable<FeatureCondition> conditions, FeatureStatus status = FeatureStatus.Enabled, string? variant = null, string? featureValue = null, double? percentage = null);

    IFeatureDefinitionBuilder WhenSegment(string segmentKey, FeatureStatus status = FeatureStatus.Enabled, string? variant = null, string? featureValue = null);

    IFeatureDefinitionBuilder WhenApplicationVersionAtLeast(string minimumVersion, FeatureStatus status = FeatureStatus.Enabled, string? variant = null, string? featureValue = null);

    IFeatureDefinitionBuilder Rollout(double percentage, FeatureStatus status = FeatureStatus.Enabled, string? variant = null, string? featureValue = null);

    IFeatureDefinitionBuilder Variant(string name, double weight, FeatureStatus status = FeatureStatus.Enabled, string? value = null);

    FeatureDefinition Build();
}

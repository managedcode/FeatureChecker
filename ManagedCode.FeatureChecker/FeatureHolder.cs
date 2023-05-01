using System.Collections.Generic;
using System.Collections.Immutable;

namespace ManagedCode.FeatureChecker;
public class FeatureHolder
{
    public readonly Dictionary<string, FeatureStatus> _features;

    public ImmutableDictionary<string, FeatureStatus> Features =>
        _features.ToImmutableDictionary();

    public FeatureHolder()
    {
        _features = new Dictionary<string, FeatureStatus>();
    }

    public bool TryAddFeature(string featureName, FeatureStatus status)
    {
        return ValidateFeatureName(featureName)
            ? _features.TryAdd(featureName, status)
            : false;
    }

    public bool TryGetFeatureStatus(string featureName, out FeatureStatus status)
    {
        status = default;

        return ValidateFeatureName(featureName)
            ? _features.TryGetValue(featureName, out status)
            : false;
    }

    public bool TryRemoveFeature(string featureName)
    {
        return ValidateFeatureName(featureName)
            ? _features.Remove(featureName)
            : false;
    }

    public void UpdateFeatureStatus(string featureName, FeatureStatus status)
    {
        if(!ValidateFeatureName(featureName))
        {
            return;
        }

        _features[featureName] = status;
    }


    private bool ValidateFeatureName(string featureName)
    {
        return !string.IsNullOrWhiteSpace(featureName);
    }
}

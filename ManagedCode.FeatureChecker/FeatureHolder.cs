using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace ManagedCode.FeatureChecker;
public class FeatureHolder
{
    private Dictionary<string, FeatureStatus> _features;

    [JsonInclude]
    public ImmutableDictionary<string, FeatureStatus> Features
    {
        get => _features.ToImmutableDictionary();
        private set => _features = new Dictionary<string, FeatureStatus>(value);
    }


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

    public void RemoveFeature(string featureName)
    {
        if(ValidateFeatureName(featureName))
        {
            _features.Remove(featureName);
        }
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

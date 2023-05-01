using System.Collections.Immutable;

namespace ManagedCode.FeatureChecker;

public class FeatureChecker
{
    private readonly ImmutableDictionary<string, FeatureStatus> _features;

    public int Count => _features.Count;

    public FeatureChecker(FeatureHolder featureHolder)
    {
        _features = featureHolder.Features;
    }

    public bool IsFeatureExists(string name)
    {
        return ValidateFeatureName(name)
            ? _features.ContainsKey(name)
            : false;
    }

    public bool TryGetFeatureStatus(string name, out FeatureStatus status)
    {
        status = default;

        return ValidateFeatureName(name)
            ? _features.TryGetValue(name, out status)
            : false;
    }

    public List<string> GetFeaturesByStatus(FeatureStatus status)
    {
        return _features
            .Where(x => x.Value == status)
            .Select(x => x.Key)
            .ToList();
    }


    private bool ValidateFeatureName(string featureName)
    {
        return !string.IsNullOrWhiteSpace(featureName);
    }

}

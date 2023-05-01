using System.Collections.Immutable;

namespace ManagedCode.FeatureChecker;

public class FeatureChecker
{
    private readonly ImmutableDictionary<string, FeatureStatus> _features;

    public FeatureChecker(FeatureHolder featureHolder)
    {
        _features = featureHolder.Features;
    }

    public bool IsFeatureExists(string name)
    {
        ThrowIfNullOrEmpty(name, nameof(name));

        return _features.ContainsKey(name);
    }
    

    private static void ThrowIfNullOrEmpty(string arg, string argName)
    {
        if(string.IsNullOrWhiteSpace(arg))
        {
            throw new ArgumentException($"Invalid parameter '{argName}': {arg}.");
        }
    }

}

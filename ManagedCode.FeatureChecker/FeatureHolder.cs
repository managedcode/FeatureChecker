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
}

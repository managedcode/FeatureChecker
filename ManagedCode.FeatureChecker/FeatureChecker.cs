namespace ManagedCode.FeatureChecker;

public class FeatureChecker : IFeatureChecker
{
    private readonly IReadOnlyDictionary<Enum, FeatureStatus> _features;

    public FeatureChecker(IReadOnlyDictionary<Enum, FeatureStatus> featureHolder)
    {
        _features = new Dictionary<Enum, FeatureStatus>(featureHolder);
    }

    public int Count => _features.Count;

    public bool IsFeatureExists(Enum feature)
    {
        return _features.ContainsKey(feature);
    }

    public bool IsEnabled(Enum feature)
    {
        if (_features.TryGetValue(feature, out var status))
            return status == FeatureStatus.Enabled;

        return false;
    }

    public bool IsDisabled(Enum feature)
    {
        if (_features.TryGetValue(feature, out var status))
            return status == FeatureStatus.Disabled;

        return false;
    }

    public bool IsDebug(Enum feature)
    {
        if (_features.TryGetValue(feature, out var status))
            return status == FeatureStatus.Debug;

        return false;
    }

    public bool TryGetFeatureStatus(Enum feature, out FeatureStatus status)
    {
        return _features.TryGetValue(feature, out status);
    }

    public List<Enum> GetFeaturesByStatus(FeatureStatus status)
    {
        return _features
            .Where(x => x.Value == status)
            .Select(x => x.Key)
            .ToList();
    }
}

namespace ManagedCode.FeatureChecker;

public interface IFeatureChecker
{
    int Count { get; }
    bool IsFeatureExists(Enum feature);
    bool IsEnabled(Enum feature);
    bool IsDisabled(Enum feature);
    bool IsDebug(Enum feature);
    bool TryGetFeatureStatus(Enum feature, out FeatureStatus status);
    List<Enum> GetFeaturesByStatus(FeatureStatus status);
}
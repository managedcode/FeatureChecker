namespace ManagedCode.FeatureChecker;

public class FeatureChecker
{
    public event EventHandler FeatureAdded;
    public event EventHandler FeatureRemoved;
    public event EventHandler FeatureStatusChanged;

    public FeatureChecker() { }

    public void AddFeature(string name, FeatureStatus status) 
        => throw new NotImplementedException();

    public void AddFeature(Feature feature)
        => throw new NotImplementedException();

    public void AddFutureRange(IEnumerable<Feature> features)
        => throw new NotImplementedException();

    public void RemoveFeature(string name) 
        => throw new NotImplementedException();

    public void RemoveFeature(Feature feature)
        => throw new NotImplementedException();

    public void RemoveFeatureRange(IEnumerable<string> features)
        => throw new NotImplementedException();

    public void RemoveFeatureRange(IEnumerable<Feature> features)
        => throw new NotImplementedException();

    public bool IsFeatureExists(string name) 
        => throw new NotImplementedException();

    public bool IsFeatureEnabled(string name)
        => throw new NotImplementedException();

    public FeatureStatus GetFeatureStatus(string name) 
        => throw new NotImplementedException();

    public bool TryGetFeatureStatus(string name, out FeatureStatus status) 
        => throw new NotImplementedException();  

    public IEnumerable<Feature> GetExistFeatures(string name) 
        => throw new NotImplementedException();

    public IEnumerable<Feature> GetFeaturesByStatus(FeatureStatus status)
        => throw new NotImplementedException();
}

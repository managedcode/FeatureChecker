namespace ManagedCode.FeatureChecker;

public class FeatureChecker
{
    private readonly List<Feature> _features;

    public FeatureChecker()
    {
        _features = new List<Feature>();
    }

    public event EventHandler FeatureAdded;
    public event EventHandler FeatureRemoved;
    public event EventHandler FeatureStatusChanged;

    public void AddFeature(string name, FeatureStatus status, bool ignoreDuplicates = true)
    {
        if(!IsFeatureExists(name))
        {
            var feature = new Feature()
            {
                Name = name,
                Status = status
            };

            _features.Add(feature);
        }

        if(!ignoreDuplicates)
        {
            throw new ArgumentException($"Feature '{nameof(name)}' is already added.");
        }
    }

    public void AddFeature(Feature feature, bool ignoreDuplicates = true)
    {
        if(feature == null)
        {
            throw new ArgumentNullException(nameof(feature), $"Parameter is null.");
        }

        AddFeature(feature.Name, feature.Status, ignoreDuplicates);
    }

    public void AddFeaturesRange(IEnumerable<Feature> features, bool ignoreDuplicates = true)
    {
        if(features == null)
        {
            throw new ArgumentNullException(nameof(features), $"Parameter is null.");
        }

        foreach(var feature in features)
        {
            AddFeature(feature, ignoreDuplicates);
        }
    }

    public void RemoveFeature(string name) 
        => throw new NotImplementedException();

    public void RemoveFeature(Feature feature)
        => throw new NotImplementedException();

    public void RemoveFeatureRange(IEnumerable<string> features)
        => throw new NotImplementedException();

    public void RemoveFeatureRange(IEnumerable<Feature> features)
        => throw new NotImplementedException();

    public bool IsFeatureExists(string? name)
    {
        if(string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException($"Invalid parameter '{nameof(name)}': {name}.");
        }

        return _features.Any(x =>
            x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsFeatureEnabled(string name)
    {
        var feature = _features.FirstOrDefault(x =>
            x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        return feature?.Status == FeatureStatus.Eanabled;
    }

    public FeatureStatus GetFeatureStatus(string name)
    {
        var feature = _features.FirstOrDefault(x =>
            x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if(feature == null)
        {
            throw new ArgumentException($"Feature '{nameof(name)}' does not exist.");
        }

        return feature.Status;
    }

    public bool TryGetFeatureStatus(string name, out FeatureStatus status)
    {
        var feature = _features.FirstOrDefault(x =>
            x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        status = feature == null
            ? default
            : feature.Status;

        return feature != null;
    }

    public IEnumerable<Feature> GetExistFeatures() => _features.AsReadOnly();

    public IEnumerable<Feature> GetFeaturesByStatus(FeatureStatus status)
    {
        return _features.Where(x => x.Status == status);
    }

    public void UpdateFeatureStatus(string name, FeatureStatus status)
    {
        if(!IsFeatureExists(name))
        {
            throw new ArgumentException($"Feature '{nameof(name)}' does not exist.");
        }

        _features.First(x 
                => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            .Status = status;
    }

    public void UpdateFeatureStatus(Feature feature)
    {
        if(feature == null)
        {
            throw new ArgumentNullException(nameof(feature), $"Parameter is null.");
        }

        UpdateFeatureStatus(feature.Name, feature.Status);
    }
}

namespace ManagedCode.FeatureChecker;

public class FeatureChecker
{
    private readonly Dictionary<string, Feature> _features;

    public FeatureChecker()
    {
        _features = new Dictionary<string, Feature>();
    }

    public event EventHandler FeatureAdded;
    public event EventHandler FeatureRemoved;
    public event EventHandler FeatureStatusChanged;


    public bool TryAddFeature(string name, FeatureStatus status)
    {
        var feature = new Feature()
        {
            Name = name,
            Status = status
        };

        return TryAddFeature(feature);
    }

    public bool TryAddFeature(Feature newFeature)
    {
        ThrowIfNull(newFeature, nameof(newFeature));
        ValidateModel(newFeature);

        return _features.TryAdd(newFeature.Name, newFeature);
    }


    public void RemoveFeature(string name)
    {
        if(string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"Invalid parameter '{nameof(name)}': {name}.");
        }

        _features.Remove(name);
    }

    public void RemoveAllFeatures() => _features.Clear();


    public bool IsFeatureExists(string name)
    {
        if(string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"Invalid parameter '{nameof(name)}': {name}.");
        }

        return _features.ContainsKey(name);
    }



    public bool TryGetFeatureStatus(string name, out FeatureStatus status)
    {
        var result = TryGetFeatureByName(name, out Feature? feature);
        status = feature?.Status ?? default;

        return result;
    }


    public bool TryGetFeatureByName(string name, out Feature? feature)
    {
        if(string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"Invalid parameter '{nameof(name)}': {name}.");
        }

        return _features.TryGetValue(name, out feature);
    }

    public Feature[] GetExistFeatures() => _features.Values.ToArray();

    public IEnumerable<Feature> GetFeaturesByStatus(FeatureStatus status)
    {
        return _features.Values.Where(x => x.Status == status);
    }


    public bool TryUpdateFeatureStatus(string name, FeatureStatus status)
    {
        var result = TryGetFeatureByName(name, out Feature? feature);

        if(result && feature != null)
        {
            feature.Status = FeatureStatus.Disabled;
        }

        return result;
    }


    private static void ThrowIfNull(object obj, string paramName)
    {
        if(obj == null)
        {
            throw new ArgumentNullException(paramName, $"Parameter is null.");
        }
    }

    private static void ValidateModel(Feature feature)
    {
        //do some validation
    }
}

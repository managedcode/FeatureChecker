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

    public void AddFeature(Feature newFeature, bool ignoreDuplicates = true)
    {
        ThrowIfNull(newFeature, nameof(newFeature));

        if(!IsFeatureExists(newFeature.Name))
        {
            _features.Add(newFeature);
        }

        if(!ignoreDuplicates)
        {
            throw new ArgumentException($"Feature '{nameof(newFeature)}' is already added.");
        }
    }

    public void AddFeaturesRange(IEnumerable<Feature> newFeatures, bool ignoreDuplicates = true)
    {
        ThrowIfNull(newFeatures, nameof(newFeatures));

        foreach(var feature in newFeatures)
        {
            AddFeature(feature, ignoreDuplicates);
        }
    }


    public void RemoveFeature(string name, bool ignoreMissing = true)
    {
        if(IsFeatureExists(name))
        {
            var feature = _features.First(x => AreNamesEqual(name, x));
            RemoveFeature(feature);

            return;
        }

        if(!ignoreMissing)
        {
            throw new ArgumentException($"Feature '{nameof(name)}' does not exist.");
        }
    }

    public void RemoveFeature(Feature feature)
    {
        ThrowIfNull(feature, nameof(feature));
        _features.Remove(feature);
    }

    public void RemoveFeatureRange(IEnumerable<string> features, bool ignoreMissing = true)
    {
        ThrowIfNull(features, nameof(features));

        foreach(var feature in features)
        {
            RemoveFeature(feature, ignoreMissing);
        }
    }

    public void RemoveFeatureRange(IEnumerable<Feature> features)
    {
        ThrowIfNull(features, nameof(features));

        foreach(var feature in features)
        {
            RemoveFeature(feature);
        }
    }

    public void RemoveAllFeatures() => _features.Clear();


    public bool IsFeatureExists(string? name)
    {
        if(string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"Invalid parameter '{nameof(name)}': {name}.");
        }

        return _features.Any(x => AreNamesEqual(name, x));
    }

    public bool IsFeatureEnabled(string name)
    {
        var feature = _features.FirstOrDefault(x => AreNamesEqual(name, x));

        return feature?.Status == FeatureStatus.Eanabled;
    }


    public FeatureStatus GetFeatureStatus(string name)
    {
        var feature = _features.FirstOrDefault(x => AreNamesEqual(name, x));

        if(feature == null)
        {
            throw new ArgumentException($"Feature '{nameof(name)}' does not exist.");
        }

        return feature.Status;
    }

    public bool TryGetFeatureStatus(string name, out FeatureStatus status)
    {
        var feature = _features.FirstOrDefault(x => AreNamesEqual(name, x));

        status = feature == null
            ? default
            : feature.Status;

        return feature != null;
    }


    public Feature? GetFeatureByName(string name)
    {
        return _features.FirstOrDefault(x => AreNamesEqual(name, x));
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

        _features.First(x => AreNamesEqual(name, x)).Status = status;
    }


    private static bool AreNamesEqual(string name, Feature fName)
    {
        return fName.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
    }

    private static void ThrowIfNull(object obj, string paramName)
    {
        if(obj == null)
        {
            throw new ArgumentNullException(paramName, $"Parameter is null.");
        }
    }
}

# FeatureChecker

## Usage

```js
        var holder = new FeatureHolder();

        holder.TryAddFeature("feature 1", default);
        holder.TryAddFeature("feature 2", FeatureStatus.Enabled);
        holder.TryAddFeature("feature 3", FeatureStatus.Debug);

        holder.UpdateFeatureStatus("feature 3", FeatureStatus.Enabled);


        var checker = new FeatureChecker(holder);

        if(checker.IsFeatureExists("feature_name"))
        {
            //do some things...
        }


        var enabledFeatures = checker.GetFeaturesByStatus(FeatureStatus.Enabled);

        foreach(var feat in enabledFeatures)
        {
            Console.WriteLine(feat);
            //other code...
        }


        bool result = checker.TryGetFeatureStatus("myFeature", out FeatureStatus status);

        if(result)
        {
            Console.WriteLine(status);
            //other code...
        }

```
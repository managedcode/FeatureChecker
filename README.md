# FeatureChecker

Small enum-based feature checker for application feature contracts.

## Usage

```csharp
using ManagedCode.FeatureChecker;

var holder = new FeatureHolder
{
    [ApplicationFeature.MarketplaceConnect] = FeatureStatus.Enabled,
    [ApplicationFeature.ExperimentalReports] = FeatureStatus.Debug,
    [ApplicationFeature.LegacyExport] = FeatureStatus.Disabled
};

var checker = new FeatureChecker(holder);

if (checker.IsEnabled(ApplicationFeature.MarketplaceConnect))
{
    // Run the paid feature.
}

var enabledFeatures = checker.GetFeaturesByStatus(FeatureStatus.Enabled);

if (checker.TryGetFeatureStatus(ApplicationFeature.ExperimentalReports, out var status))
{
    Console.WriteLine(status);
}

public enum ApplicationFeature
{
    MarketplaceConnect,
    ExperimentalReports,
    LegacyExport
}
```

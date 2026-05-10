using ManagedCode.FeatureChecker.Definitions;

namespace ManagedCode.FeatureChecker.Storage;

public interface IFeatureDefinitionProvider
{
    IReadOnlyCollection<FeatureDefinition> GetFeatureDefinitions();
}

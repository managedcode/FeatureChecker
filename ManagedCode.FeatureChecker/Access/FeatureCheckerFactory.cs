using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Evaluation;
using ManagedCode.FeatureChecker.Storage;
using ManagedCode.FeatureChecker.Targeting;
using Microsoft.Extensions.DependencyInjection;
using FeatureCheckerEvaluator = ManagedCode.FeatureChecker.Evaluation.FeatureChecker;

namespace ManagedCode.FeatureChecker.Access;

public sealed class FeatureCheckerFactory : IFeatureCheckerFactory
{
    private readonly IFeatureDefinitionProvider _provider;

    public FeatureCheckerFactory(FeatureSnapshot snapshot)
        : this(new FeatureSnapshotProvider(snapshot))
    {
    }

    public FeatureCheckerFactory(IEnumerable<FeatureDefinition> features)
        : this(FeatureSnapshot.FromDefinitions(features))
    {
    }

    [ActivatorUtilitiesConstructor]
    public FeatureCheckerFactory(IFeatureDefinitionProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public IFeatureEvaluator Create()
    {
        return new FeatureCheckerEvaluator(_provider);
    }

    public IFeatureScope CreateScope(FeatureEvaluationContext? context = null)
    {
        return new FeatureScope(Create(), context);
    }
}

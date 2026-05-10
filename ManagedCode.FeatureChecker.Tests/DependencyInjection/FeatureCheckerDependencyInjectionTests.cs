using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.FeatureChecker.Tests.DependencyInjection;

public sealed class FeatureCheckerDependencyInjectionTests
{
    [Test]
    public void ServiceCollection_ShouldRegisterFeatureCheckerFromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["Snapshot:Features:0:Key"] = FeatureNames.Checkout,
                ["Snapshot:Features:0:Status"] = nameof(FeatureStatus.Disabled),
                ["Snapshot:Features:0:Rules:0:Status"] = nameof(FeatureStatus.Enabled),
                ["Snapshot:Features:0:Rules:0:Conditions:0:Attribute"] = FeatureContextAttributeNames.Plan,
                ["Snapshot:Features:0:Rules:0:Conditions:0:Operator"] = nameof(FeatureConditionOperator.Equals),
                ["Snapshot:Features:0:Rules:0:Conditions:0:Values:0"] = Values.Enterprise
            })
            .Build();
        using var services = new ServiceCollection()
            .AddFeatureChecker(configuration)
            .BuildServiceProvider();

        var checker = services.GetRequiredService<IFeatureEvaluator>();
        var context = FeatureEvaluationContextBuilder.Create().ForUser(UserIds.Included).WithPlan(Values.Enterprise).Build();

        checker.IsEnabled(FeatureNames.Checkout, context).ShouldBeTrue();
    }

    [Test]
    public void ServiceCollection_ShouldRegisterFeatureCheckerFromSnapshotSource()
    {
        var source = new MutableFeatureSnapshotSource(FeatureStatus.Disabled);
        using var services = new ServiceCollection()
            .AddSingleton(source)
            .AddFeatureCheckerSnapshotSource<MutableFeatureSnapshotSource>()
            .BuildServiceProvider();

        var factory = services.GetRequiredService<IFeatureCheckerFactory>();

        factory.Create().IsDisabled(FeatureNames.Checkout).ShouldBeTrue();

        source.Replace(FeatureStatus.Enabled);

        factory.Create().IsEnabled(FeatureNames.Checkout).ShouldBeTrue();
        services.GetRequiredService<IFeatureEvaluator>().IsEnabled(FeatureNames.Checkout).ShouldBeTrue();
    }

    [Test]
    public void ServiceCollection_ShouldPreferExplicitSnapshotSourceOverConfiguredOptions()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["Snapshot:Features:0:Key"] = FeatureNames.Checkout,
                ["Snapshot:Features:0:Status"] = nameof(FeatureStatus.Disabled)
            })
            .Build();
        var source = new MutableFeatureSnapshotSource(FeatureStatus.Enabled);
        using var services = new ServiceCollection()
            .AddFeatureChecker(configuration)
            .AddFeatureCheckerSnapshotSource(source)
            .BuildServiceProvider();

        services.GetRequiredService<IFeatureEvaluator>().IsEnabled(FeatureNames.Checkout).ShouldBeTrue();
    }

    [Test]
    public void ServiceCollection_ShouldRegisterFeatureCheckerFromDefinitionProvider()
    {
        using var services = new ServiceCollection()
            .AddFeatureCheckerProvider<StaticFeatureDefinitionProvider>()
            .BuildServiceProvider();

        services.GetRequiredService<IFeatureEvaluator>().IsEnabled(FeatureNames.Checkout).ShouldBeTrue();
    }

    private static class FeatureNames
    {
        public const string Checkout = "checkout.new-flow";
    }

    private static class UserIds
    {
        public const string Included = "user-included";
    }

    private static class Values
    {
        public const string Enterprise = "enterprise";
    }

    private sealed class MutableFeatureSnapshotSource : IFeatureSnapshotSource
    {
        private FeatureSnapshot _snapshot;

        public MutableFeatureSnapshotSource(FeatureStatus status)
        {
            _snapshot = CreateSnapshot(status);
        }

        public FeatureSnapshot GetSnapshot()
        {
            return _snapshot;
        }

        public void Replace(FeatureStatus status)
        {
            _snapshot = CreateSnapshot(status);
        }

        private static FeatureSnapshot CreateSnapshot(FeatureStatus status)
        {
            return FeatureSnapshot.FromDefinitions(
                [
                    FeatureDefinition.Create(FeatureNames.Checkout, status)
                ]);
        }
    }

    private sealed class StaticFeatureDefinitionProvider : IFeatureDefinitionProvider
    {
        public IReadOnlyCollection<FeatureDefinition> GetFeatureDefinitions()
        {
            return
            [
                FeatureDefinition.Create(FeatureNames.Checkout, FeatureStatus.Enabled)
            ];
        }
    }
}

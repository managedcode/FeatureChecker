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
}

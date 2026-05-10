using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.FeatureChecker.Tests.Integration;

[Category(TestCategories.Integration)]
public sealed class FeatureCheckerIntegrationTests
{
    [Test]
    public void FeatureChecker_ShouldEvaluateConfigurationBackedScopeThroughDependencyInjection()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
            {
                ["Snapshot:Features:0:Key"] = FeatureNames.Checkout,
                ["Snapshot:Features:0:Status"] = nameof(FeatureStatus.Disabled),
                ["Snapshot:Features:0:Value"] = DefaultValues.Control,
                ["Snapshot:Features:0:Rules:0:Status"] = nameof(FeatureStatus.Enabled),
                ["Snapshot:Features:0:Rules:0:Value"] = DefaultValues.Treatment,
                ["Snapshot:Features:0:Rules:0:Conditions:0:Attribute"] = FeatureContextAttributeNames.Plan,
                ["Snapshot:Features:0:Rules:0:Conditions:0:Operator"] = nameof(FeatureConditionOperator.Equals),
                ["Snapshot:Features:0:Rules:0:Conditions:0:Values:0"] = AttributeValues.Enterprise
            })
            .Build();
        using var services = new ServiceCollection()
            .AddFeatureChecker(configuration)
            .BuildServiceProvider();
        var factory = services.GetRequiredService<IFeatureCheckerFactory>();
        var context = FeatureEvaluationContextBuilder
            .Create()
            .ForUser(TargetingKeys.UserA)
            .WithPlan(AttributeValues.Enterprise)
            .Build();

        var evaluation = factory.CreateScope(context).Evaluate(FeatureNames.Checkout);

        evaluation.Reason.ShouldBe(FeatureEvaluationReasons.Rule);
        evaluation.Rule.ShouldNotBeNull();
        evaluation.Status.ShouldBe(FeatureStatus.Enabled);
        evaluation.Value.ShouldBe(DefaultValues.Treatment);
    }

    private static class FeatureNames
    {
        public const string Checkout = "checkout.new-flow";
    }

    private static class AttributeValues
    {
        public const string Enterprise = "enterprise";
    }

    private static class TargetingKeys
    {
        public const string UserA = "user-a";
    }

    private static class DefaultValues
    {
        public const string Control = "control-value";
        public const string Treatment = "treatment-value";
    }
}

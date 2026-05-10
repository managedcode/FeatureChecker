namespace ManagedCode.FeatureChecker.Tests.Access;

public sealed class FeatureAccessTests
{
    [Test]
    public void FeatureSetBuilder_ShouldComposeFeaturesThroughPublicInterfaces()
    {
        IFeatureSetBuilder builder = new FeatureSetBuilder();
        builder
            .Feature(FeatureNames.InterfaceBuiltFeature)
            .Disabled()
            .WhenAttributeEquals(FeatureContextAttributeNames.Plan, AttributeValues.Enterprise);

        var checker = builder.ToChecker();
        var context = FeatureEvaluationContextBuilder
            .Create()
            .ForUser(TargetingKeys.UserA)
            .WithPlan(AttributeValues.Enterprise)
            .Build();

        checker.IsEnabled(FeatureNames.InterfaceBuiltFeature, context).ShouldBeTrue();
    }

    [Test]
    public void FeatureCheckerFactory_ShouldIssueFreshCheckersFromFileProvider()
    {
        var path = CreateTempJsonPath();

        try
        {
            SaveSnapshot(path, FeatureStatus.Disabled);
            var factory = new FeatureCheckerFactory(new FeatureFileProvider(path));

            factory.Create().IsDisabled(FeatureNames.MarketplaceConnect).ShouldBeTrue();

            SaveSnapshot(path, FeatureStatus.Enabled);

            factory.Create().IsEnabled(FeatureNames.MarketplaceConnect).ShouldBeTrue();
        }
        finally
        {
            DeleteFile(path);
        }
    }

    [Test]
    public void FeatureCheckerFactory_ShouldCreateUserTenantAndSessionScopesForControllerCode()
    {
        var builder = new FeatureSetBuilder();
        builder
            .Feature(FeatureNames.ControllerFeature)
            .Disabled(DefaultValues.FalseValue)
            .WhenAll(
                [
                    FeatureCondition.Equals(FeatureContextAttributeNames.Plan, AttributeValues.Enterprise),
                    FeatureCondition.Equals(FeatureContextAttributeNames.TenantId, TenantIds.TenantA),
                    FeatureCondition.Equals(FeatureContextAttributeNames.SessionId, SessionIds.SessionA)
                ],
                featureValue: DefaultValues.TrueValue);
        var factory = new FeatureCheckerFactory(builder.Build());

        var scope = factory.ForUser(
            TargetingKeys.UserA,
            context => context
                .WithTenantId(TenantIds.TenantA)
                .WithSessionId(SessionIds.SessionA)
                .WithPlan(AttributeValues.Enterprise)
                .WithRole(RoleNames.Admin)
                .WithRegion(AttributeValues.Eu));

        scope.Context.TargetingKey.ShouldBe(TargetingKeys.UserA);
        scope.Context.Attributes[FeatureContextAttributeNames.UserId].ShouldBe(TargetingKeys.UserA);
        scope.Context.Attributes[FeatureContextAttributeNames.TenantId].ShouldBe(TenantIds.TenantA);
        scope.Context.Attributes[FeatureContextAttributeNames.SessionId].ShouldBe(SessionIds.SessionA);
        scope.IsEnabled(FeatureNames.ControllerFeature).ShouldBeTrue();
        scope.GetBooleanValue(FeatureNames.ControllerFeature, false).ShouldBeTrue();
    }

    [Test]
    public void FeatureScope_ShouldEvaluateAgainstBoundContextWithoutRepeatingIt()
    {
        var builder = new FeatureSetBuilder();
        builder.Feature(FeatureNames.ExperimentalReports).Disabled().WhenAttributeEquals(FeatureContextAttributeNames.Role, RoleNames.Admin);
        builder.Feature(FeatureNames.RetiredExport).Disabled();
        var checker = builder.ToChecker();
        var context = FeatureEvaluationContextBuilder.Create().ForUser(TargetingKeys.UserA).WithRole(RoleNames.Admin).Build();
        var scope = checker.CreateScope(context);

        scope.IsEnabled(FeatureNames.ExperimentalReports).ShouldBeTrue();
        scope.GetFeatureKeysByStatus(FeatureStatus.Enabled).ShouldBe([FeatureNames.ExperimentalReports]);
        scope.TryGetFeatureStatus(FeatureNames.Unknown, out _).ShouldBeFalse();
    }

    private static void SaveSnapshot(string path, FeatureStatus status)
    {
        FeatureSnapshotSerializer.Save(
            path,
            FeatureSnapshot.FromDefinitions(
                [
                    FeatureDefinition.Create(FeatureNames.MarketplaceConnect, status)
                ]));
    }

    private static string CreateTempJsonPath()
    {
        return Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
    }

    private static void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static class FeatureNames
    {
        public const string InterfaceBuiltFeature = "interface.built";
        public const string MarketplaceConnect = "marketplace.connect";
        public const string ControllerFeature = "controller.feature";
        public const string ExperimentalReports = "reports.experimental";
        public const string RetiredExport = "retired.export";
        public const string Unknown = "unknown.feature";
    }

    private static class AttributeValues
    {
        public const string Enterprise = "enterprise";
        public const string Eu = "eu";
    }

    private static class TargetingKeys
    {
        public const string UserA = "user-a";
    }

    private static class TenantIds
    {
        public const string TenantA = "tenant-a";
    }

    private static class SessionIds
    {
        public const string SessionA = "session-a";
    }

    private static class RoleNames
    {
        public const string Admin = "admin";
    }

    private static class DefaultValues
    {
        public const string TrueValue = "true";
        public const string FalseValue = "false";
    }
}

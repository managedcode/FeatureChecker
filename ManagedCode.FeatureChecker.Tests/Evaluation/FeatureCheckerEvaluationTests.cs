namespace ManagedCode.FeatureChecker.Tests.Evaluation;

public sealed class FeatureCheckerEvaluationTests
{
    [Test]
    public void FeatureChecker_ShouldEvaluateStringKeysThroughPublicInterface()
    {
        IFeatureEvaluator checker = new FeatureCheckerEvaluator(
            [
                FeatureDefinition.Create(FeatureNames.MarketplaceConnect, FeatureStatus.Enabled),
                FeatureDefinition.Create(FeatureNames.RetiredExport, FeatureStatus.Disabled)
            ]);

        checker.IsFeatureExists(FeatureNames.MarketplaceConnect).ShouldBeTrue();
        checker.IsEnabled(FeatureNames.MarketplaceConnect).ShouldBeTrue();
        checker.IsDisabled(FeatureNames.RetiredExport).ShouldBeTrue();
        checker.TryGetFeatureStatus(FeatureNames.MarketplaceConnect, out var status).ShouldBeTrue();
        status.ShouldBe(FeatureStatus.Enabled);
        checker.Evaluate(FeatureNames.Unknown).Reason.ShouldBe(FeatureEvaluationReasons.Missing);
    }

    [Test]
    public void FeatureChecker_ShouldRespectFeatureDependencies()
    {
        var builder = new FeatureSetBuilder();
        builder.Feature(FeatureNames.ParentKillSwitch).Disabled();
        builder.Feature(FeatureNames.ChildCheckout).Enabled().Require(FeatureNames.ParentKillSwitch);

        var evaluation = builder.ToChecker().Evaluate(FeatureNames.ChildCheckout);

        evaluation.IsDisabled.ShouldBeTrue();
        evaluation.Reason.ShouldBe(FeatureEvaluationReasons.Dependency);
    }

    [Test]
    public void FeatureChecker_ShouldReportDependencyCycles()
    {
        var builder = new FeatureSetBuilder();
        builder.Feature(FeatureNames.ParentKillSwitch).Enabled().Require(FeatureNames.ChildCheckout);
        builder.Feature(FeatureNames.ChildCheckout).Enabled().Require(FeatureNames.ParentKillSwitch);

        var evaluation = builder.ToChecker().Evaluate(FeatureNames.ParentKillSwitch);

        evaluation.IsDisabled.ShouldBeTrue();
        evaluation.Reason.ShouldBe(FeatureEvaluationReasons.DependencyCycle);
    }

    [Test]
    public void FeatureChecker_ShouldEvaluateVariantsAndRemoteConfigValues()
    {
        var builder = new FeatureSetBuilder();
        builder
            .Feature(FeatureNames.PricingCard)
            .Disabled(DefaultValues.Control)
            .Variant(VariantNames.Control, 0d, FeatureStatus.Disabled, DefaultValues.Control)
            .Variant(VariantNames.Treatment, 100d, FeatureStatus.Enabled, DefaultValues.Treatment);

        var evaluation = builder.ToChecker().Evaluate(
            FeatureNames.PricingCard,
            FeatureEvaluationContext.ForTargetingKey(TargetingKeys.UserA));

        evaluation.IsEnabled.ShouldBeTrue();
        evaluation.Variant.ShouldBe(VariantNames.Treatment);
        evaluation.Value.ShouldBe(DefaultValues.Treatment);
    }

    [Test]
    public void FeatureChecker_ShouldEvaluateAllFeaturesForContext()
    {
        var builder = new FeatureSetBuilder();
        builder.Feature(FeatureNames.MarketplaceConnect).Enabled();
        builder.Feature(FeatureNames.ExperimentalReports).Disabled().WhenAttributeEquals(AttributeNames.Plan, AttributeValues.Enterprise);
        var checker = builder.ToChecker();
        var context = FeatureEvaluationContext.ForTargetingKey(TargetingKeys.UserA).With(AttributeNames.Plan, AttributeValues.Enterprise);

        var evaluations = checker.EvaluateAll(context);

        evaluations[FeatureNames.MarketplaceConnect].IsEnabled.ShouldBeTrue();
        evaluations[FeatureNames.ExperimentalReports].IsEnabled.ShouldBeTrue();
        checker.GetFeatureKeysByStatus(FeatureStatus.Enabled, context).ShouldBe([FeatureNames.MarketplaceConnect, FeatureNames.ExperimentalReports]);
    }

    [Test]
    public void FeatureEvaluatorExtensions_ShouldReturnTypedValuesWithExplicitDefaults()
    {
        var checker = new FeatureCheckerEvaluator(
            [
                new FeatureDefinition { Key = FeatureNames.BooleanValueFeature, Status = FeatureStatus.Enabled, Value = DefaultValues.TrueValue },
                new FeatureDefinition { Key = FeatureNames.IntValueFeature, Status = FeatureStatus.Enabled, Value = DefaultValues.IntValue },
                new FeatureDefinition { Key = FeatureNames.LongValueFeature, Status = FeatureStatus.Enabled, Value = DefaultValues.LongValue },
                new FeatureDefinition { Key = FeatureNames.DoubleValueFeature, Status = FeatureStatus.Enabled, Value = DefaultValues.DoubleValue },
                new FeatureDefinition { Key = FeatureNames.InvalidNumberFeature, Status = FeatureStatus.Enabled, Value = DefaultValues.InvalidNumber }
            ]);

        checker.GetBooleanValue(FeatureNames.BooleanValueFeature, false).ShouldBeTrue();
        checker.GetInt32Value(FeatureNames.IntValueFeature, 0).ShouldBe(42);
        checker.GetInt64Value(FeatureNames.LongValueFeature, 0L).ShouldBe(9_000_000_000L);
        checker.GetDoubleValue(FeatureNames.DoubleValueFeature, 0d).ShouldBe(12.5d);
        checker.GetStringValue(FeatureNames.Unknown, DefaultValues.DefaultConfig).ShouldBe(DefaultValues.DefaultConfig);
        checker.GetInt32Value(FeatureNames.InvalidNumberFeature, 7).ShouldBe(7);
    }

    [Test]
    public void FeatureEvaluatorExtensions_ShouldReturnTypedVariationDetails()
    {
        var builder = new FeatureSetBuilder();
        builder.Feature(FeatureNames.Template)
            .Enabled()
            .FallthroughVariation(VariantNames.Treatment, DefaultValues.Treatment)
            .Variant(VariantNames.Treatment, 100, FeatureStatus.Enabled, DefaultValues.Treatment);

        var detail = builder.ToChecker().StringVariationDetail(
            FeatureNames.Template,
            DefaultValues.DefaultConfig,
            FeatureEvaluationContext.ForTargetingKey(TargetingKeys.UserA));

        detail.Exists.ShouldBeTrue();
        detail.Value.ShouldBe(DefaultValues.Treatment);
        detail.ReasonKind.ShouldBe(FeatureEvaluationReasonKind.Fallthrough);
        detail.VariationIndex.ShouldBe(0);
    }

    private static class FeatureNames
    {
        public const string MarketplaceConnect = "marketplace.connect";
        public const string ExperimentalReports = "reports.experimental";
        public const string RetiredExport = "retired.export";
        public const string ParentKillSwitch = "parent.kill-switch";
        public const string ChildCheckout = "checkout.child";
        public const string PricingCard = "pricing.card";
        public const string BooleanValueFeature = "value.bool";
        public const string IntValueFeature = "value.int";
        public const string LongValueFeature = "value.long";
        public const string DoubleValueFeature = "value.double";
        public const string InvalidNumberFeature = "value.invalid-number";
        public const string Template = "ui.template";
        public const string Unknown = "unknown.feature";
    }

    private static class AttributeNames
    {
        public const string Plan = "plan";
    }

    private static class AttributeValues
    {
        public const string Enterprise = "enterprise";
    }

    private static class TargetingKeys
    {
        public const string UserA = "user-a";
    }

    private static class VariantNames
    {
        public const string Control = "control";
        public const string Treatment = "treatment";
    }

    private static class DefaultValues
    {
        public const string Control = "control-config";
        public const string Treatment = "treatment-config";
        public const string TrueValue = "true";
        public const string IntValue = "42";
        public const string LongValue = "9000000000";
        public const string DoubleValue = "12.5";
        public const string InvalidNumber = "not-a-number";
        public const string DefaultConfig = "default-config";
    }
}

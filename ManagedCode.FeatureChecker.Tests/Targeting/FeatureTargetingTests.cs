namespace ManagedCode.FeatureChecker.Tests.Targeting;

public sealed class FeatureTargetingTests
{
    [Test]
    public void FeatureChecker_ShouldApplyContextualTargetingRules()
    {
        var checker = new FeatureCheckerEvaluator(
            FeatureSnapshot.FromDefinitions(
                [
                    new FeatureDefinition
                    {
                        Key = FeatureNames.ExperimentalReports,
                        Status = FeatureStatus.Disabled,
                        Rules =
                        [
                            new FeatureTargetingRule
                            {
                                Status = FeatureStatus.Enabled,
                                Conditions =
                                [
                                    FeatureCondition.Equals(AttributeNames.Plan, AttributeValues.Enterprise),
                                    FeatureCondition.In(AttributeNames.Region, AttributeValues.Eu, AttributeValues.Us)
                                ]
                            }
                        ]
                    }
                ]));
        var matchingContext = FeatureEvaluationContext
            .ForTargetingKey(TargetingKeys.UserA)
            .With(AttributeNames.Plan, AttributeValues.Enterprise)
            .With(AttributeNames.Region, AttributeValues.Eu);
        var nonMatchingContext = FeatureEvaluationContext
            .ForTargetingKey(TargetingKeys.UserB)
            .With(AttributeNames.Plan, AttributeValues.Free)
            .With(AttributeNames.Region, AttributeValues.Eu);

        checker.IsEnabled(FeatureNames.ExperimentalReports, matchingContext).ShouldBeTrue();
        checker.IsDisabled(FeatureNames.ExperimentalReports, nonMatchingContext).ShouldBeTrue();
    }

    [Test]
    public void FeatureChecker_ShouldSupportConditionOperators()
    {
        var checker = new FeatureCheckerEvaluator(
            FeatureSnapshot.FromDefinitions(
                [
                    new FeatureDefinition
                    {
                        Key = FeatureNames.ConditionFeature,
                        Status = FeatureStatus.Disabled,
                        Rules =
                        [
                            new FeatureTargetingRule
                            {
                                Status = FeatureStatus.Enabled,
                                Conditions =
                                [
                                    FeatureCondition.Exists(AttributeNames.Plan),
                                    FeatureCondition.NotIn(AttributeNames.Region, AttributeValues.Apac),
                                    FeatureCondition.Contains(AttributeNames.Email, AttributeValues.CompanyDomain)
                                ]
                            }
                        ]
                    }
                ]));
        var context = FeatureEvaluationContext
            .ForTargetingKey(TargetingKeys.UserA)
            .With(AttributeNames.Plan, AttributeValues.Enterprise)
            .With(AttributeNames.Region, AttributeValues.Eu)
            .With(AttributeNames.Email, "person@managed-code.com");

        checker.IsEnabled(FeatureNames.ConditionFeature, context).ShouldBeTrue();
    }

    [Test]
    public void FeatureChecker_ShouldSupportMissingAttributeConditionOperators()
    {
        var checker = new FeatureCheckerEvaluator(
            FeatureSnapshot.FromDefinitions(
                [
                    new FeatureDefinition
                    {
                        Key = FeatureNames.MissingAttributeFeature,
                        Status = FeatureStatus.Disabled,
                        Rules =
                        [
                            new FeatureTargetingRule
                            {
                                Status = FeatureStatus.Enabled,
                                Conditions =
                                [
                                    FeatureCondition.NotExists(AttributeNames.Plan),
                                    FeatureCondition.NotEquals(AttributeNames.Plan, AttributeValues.Enterprise),
                                    FeatureCondition.NotIn(AttributeNames.Region, AttributeValues.Apac)
                                ]
                            }
                        ]
                    },
                    new FeatureDefinition
                    {
                        Key = FeatureNames.RequiredAttributeFeature,
                        Status = FeatureStatus.Disabled,
                        Rules =
                        [
                            new FeatureTargetingRule
                            {
                                Status = FeatureStatus.Enabled,
                                Conditions =
                                [
                                    FeatureCondition.Exists(AttributeNames.Plan),
                                    FeatureCondition.In(AttributeNames.Region, AttributeValues.Eu)
                                ]
                            }
                        ]
                    }
                ]));
        var context = FeatureEvaluationContext.ForTargetingKey(TargetingKeys.UserA);

        checker.IsEnabled(FeatureNames.MissingAttributeFeature, context).ShouldBeTrue();
        checker.IsDisabled(FeatureNames.RequiredAttributeFeature, context).ShouldBeTrue();
    }

    [Test]
    public void FeatureChecker_ShouldSupportTextConditionOperators()
    {
        var checker = new FeatureCheckerEvaluator(
            FeatureSnapshot.FromDefinitions(
                [
                    new FeatureDefinition
                    {
                        Key = FeatureNames.TextConditionFeature,
                        Status = FeatureStatus.Disabled,
                        Rules =
                        [
                            new FeatureTargetingRule
                            {
                                Status = FeatureStatus.Enabled,
                                Conditions =
                                [
                                    FeatureCondition.StartsWith(AttributeNames.Email, "person@"),
                                    FeatureCondition.EndsWith(AttributeNames.Email, ".com")
                                ]
                            }
                        ]
                    }
                ]));
        var context = FeatureEvaluationContext
            .ForTargetingKey(TargetingKeys.UserA)
            .With(AttributeNames.Email, "person@managed-code.com");

        checker.IsEnabled(FeatureNames.TextConditionFeature, context).ShouldBeTrue();
    }

    [Test]
    public void FeatureChecker_ShouldTargetIndividualContextBeforeRules()
    {
        var builder = new FeatureSetBuilder();
        builder.Feature(FeatureNames.Checkout)
            .Disabled()
            .Target(TargetingKeys.Blocked, FeatureStatus.Disabled, VariantNames.Control, DefaultValues.Control)
            .WhenAttributeEquals(FeatureContextAttributeNames.Plan, AttributeValues.Enterprise, variant: VariantNames.Treatment, featureValue: DefaultValues.Treatment)
            .Variant(VariantNames.Control, 50, FeatureStatus.Disabled, DefaultValues.Control)
            .Variant(VariantNames.Treatment, 50, FeatureStatus.Enabled, DefaultValues.Treatment);

        var context = FeatureEvaluationContextBuilder.Create()
            .ForUser(TargetingKeys.Blocked)
            .WithPlan(AttributeValues.Enterprise)
            .Build();

        var evaluation = builder.ToChecker().Evaluate(FeatureNames.Checkout, context);

        evaluation.IsDisabled.ShouldBeTrue();
        evaluation.ReasonKind.ShouldBe(FeatureEvaluationReasonKind.TargetMatch);
        evaluation.Variant.ShouldBe(VariantNames.Control);
        evaluation.Value.ShouldBe(DefaultValues.Control);
    }

    [Test]
    public void FeatureChecker_ShouldSupportMinimumApplicationVersionRules()
    {
        var builder = new FeatureSetBuilder();
        builder.Feature(FeatureNames.MobilePay)
            .Disabled()
            .WhenApplicationVersionAtLeast("2.3.0");

        var current = FeatureEvaluationContextBuilder.Create()
            .ForUser(TargetingKeys.UserA)
            .WithApplication(AppNames.Storefront, "2.3.1")
            .Build();
        var old = FeatureEvaluationContextBuilder.Create()
            .ForUser(TargetingKeys.UserB)
            .WithApplication(AppNames.Storefront, "2.2.9")
            .Build();

        var checker = builder.ToChecker();

        checker.IsEnabled(FeatureNames.MobilePay, current).ShouldBeTrue();
        checker.IsDisabled(FeatureNames.MobilePay, old).ShouldBeTrue();
    }

    private static class FeatureNames
    {
        public const string ExperimentalReports = "reports.experimental";
        public const string ConditionFeature = "condition.feature";
        public const string MissingAttributeFeature = "condition.missing-attribute";
        public const string RequiredAttributeFeature = "condition.required-attribute";
        public const string TextConditionFeature = "condition.text";
        public const string Checkout = "checkout.new-flow";
        public const string MobilePay = "mobile.pay";
    }

    private static class AttributeNames
    {
        public const string Plan = "plan";
        public const string Region = "region";
        public const string Email = "email";
    }

    private static class AttributeValues
    {
        public const string Enterprise = "enterprise";
        public const string Free = "free";
        public const string Eu = "eu";
        public const string Us = "us";
        public const string Apac = "apac";
        public const string CompanyDomain = "managed-code.com";
    }

    private static class TargetingKeys
    {
        public const string UserA = "user-a";
        public const string UserB = "user-b";
        public const string Blocked = "user-blocked";
    }

    private static class VariantNames
    {
        public const string Control = "control";
        public const string Treatment = "treatment";
    }

    private static class DefaultValues
    {
        public const string Control = "control-value";
        public const string Treatment = "treatment-value";
    }

    private static class AppNames
    {
        public const string Storefront = "storefront";
    }
}

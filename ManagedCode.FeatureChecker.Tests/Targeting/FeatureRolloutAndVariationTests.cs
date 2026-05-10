namespace ManagedCode.FeatureChecker.Tests.Targeting;

public sealed class FeatureRolloutAndVariationTests
{
    [Test]
    public void FeatureChecker_ShouldUseStickyPercentageRollout()
    {
        var builder = new FeatureSetBuilder();
        builder.Feature(FeatureNames.RolloutFeature).Disabled().Rollout(100d);

        var checker = builder.ToChecker();
        var context = FeatureEvaluationContext.ForTargetingKey(TargetingKeys.UserA);

        checker.IsEnabled(FeatureNames.RolloutFeature, context).ShouldBeTrue();
        checker.Evaluate(FeatureNames.RolloutFeature).IsDisabled.ShouldBeTrue();
    }

    [Test]
    public void FeatureChecker_ShouldUseOffAndFallthroughVariations()
    {
        var builder = new FeatureSetBuilder();
        builder.Feature(FeatureNames.KillSwitch)
            .Enabled()
            .Targeting(false)
            .OffVariation(VariantNames.Off, DefaultValues.Off)
            .Variant(VariantNames.Off, 100, FeatureStatus.Disabled, DefaultValues.Off);
        builder.Feature(FeatureNames.Template)
            .Enabled()
            .FallthroughVariation(VariantNames.Treatment, DefaultValues.Treatment)
            .Variant(VariantNames.Treatment, 100, FeatureStatus.Enabled, DefaultValues.Treatment);

        var checker = builder.ToChecker();
        var context = FeatureEvaluationContext.ForTargetingKey(TargetingKeys.UserA);

        var offEvaluation = checker.Evaluate(FeatureNames.KillSwitch, context);
        var fallthrough = checker.Evaluate(FeatureNames.Template, context);

        offEvaluation.IsDisabled.ShouldBeTrue();
        offEvaluation.ReasonKind.ShouldBe(FeatureEvaluationReasonKind.Off);
        offEvaluation.Variant.ShouldBe(VariantNames.Off);
        fallthrough.ReasonKind.ShouldBe(FeatureEvaluationReasonKind.Fallthrough);
        fallthrough.VariationIndex.ShouldBe(0);
        fallthrough.Value.ShouldBe(DefaultValues.Treatment);
    }

    private static class FeatureNames
    {
        public const string RolloutFeature = "rollout.feature";
        public const string KillSwitch = "ops.kill-switch";
        public const string Template = "ui.template";
    }

    private static class TargetingKeys
    {
        public const string UserA = "user-a";
    }

    private static class VariantNames
    {
        public const string Treatment = "treatment";
        public const string Off = "off";
    }

    private static class DefaultValues
    {
        public const string Treatment = "treatment-value";
        public const string Off = "off-value";
    }
}

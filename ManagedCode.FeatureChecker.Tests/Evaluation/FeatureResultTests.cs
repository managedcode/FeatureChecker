namespace ManagedCode.FeatureChecker.Tests.Evaluation;

public sealed class FeatureResultTests
{
    private const string Key = "checkout";
    private const string User = "user";

    [Test]
    [Arguments("NaN", 7d)]
    [Arguments("Infinity", 7d)]
    [Arguments("1e999", 7d)]
    [Arguments("1.25e2", 125d)]
    public void DoubleResults_UseExplicitDefaultsForNonFiniteValues(string text, double expected)
    {
        var builder = new FeatureSetBuilder();
        builder.Feature(Key).Enabled(text);
        var checker = builder.ToChecker();

        checker.GetDoubleValue(Key, 7d).ShouldBe(expected);
        var detail = checker.DoubleVariationDetail(Key, 7d);
        detail.Value.ShouldBe(expected);
        detail.Exists.ShouldBeTrue();
        detail.ReasonKind.ShouldBe(FeatureEvaluationReasonKind.Default);
    }

    [Test]
    public void UnresolvedNamedVariations_UseTheSelectingClauseStatus()
    {
        var builder = new FeatureSetBuilder();
        builder.Feature("off").Enabled().Targeting(false).OffVariation("unknown", "safe");
        builder.Feature("target").Enabled().Target(User, FeatureStatus.Disabled, "unknown");
        builder.Feature("rule").Disabled().WhenAttributeEquals("plan", "paid", variant: "unknown");
        var checker = builder.ToChecker();
        var context = FeatureEvaluationContext.ForTargetingKey(User).With("plan", "paid");

        var off = checker.Evaluate("off", context);
        off.IsDisabled.ShouldBeTrue();
        off.Value.ShouldBe("safe");
        off.VariationIndex.ShouldBeNull();
        checker.IsDisabled("target", context).ShouldBeTrue();
        checker.IsEnabled("rule", context).ShouldBeTrue();
    }
}

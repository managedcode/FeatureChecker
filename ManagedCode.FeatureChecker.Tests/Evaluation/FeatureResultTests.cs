namespace ManagedCode.FeatureChecker.Tests.Evaluation;

public sealed class FeatureResultTests
{
    private const string Key = "checkout";
    private const string User = "user";

    [Test]
    public void TargetResults_KeepTheSelectedVariantAndContextPrecedence()
    {
        const string secondKey = "other";
        const string variant = "targeted";
        var sharedTarget = new FeatureTarget { Key = User, Variant = variant };
        var priority = new FeatureTarget { Key = User, ContextKind = "USER", Status = FeatureStatus.Debug };
        var checker = new FeatureCheckerEvaluator(
            [
                new FeatureDefinition
                {
                    Key = Key,
                    Targets = [sharedTarget],
                    Variants = [new FeatureVariant { Name = "first" }, new FeatureVariant { Name = variant, Value = "first-value" }]
                },
                new FeatureDefinition
                {
                    Key = secondKey,
                    Targets = [priority, sharedTarget],
                    Variants = [new FeatureVariant { Name = variant, Value = "second-value" }]
                }
            ]);
        var user = FeatureEvaluationContext.ForTargetingKey(User);
        var tenant = user.WithContextKind(FeatureContextKinds.Tenant);

        Parallel.For(0, 32, _ =>
        {
            var first = checker.Evaluate(Key, user);
            first.Value.ShouldBe("first-value");
            first.VariationIndex.ShouldBe(1);
            var second = checker.Evaluate(secondKey, tenant);
            second.Value.ShouldBe("second-value");
            second.VariationIndex.ShouldBe(0);
            checker.Evaluate(secondKey, user).IsDebug.ShouldBeTrue();
        });
    }

    [Test]
    public void MissingResults_UseTheCallersDefaultEvenIfTheEvaluatorSuppliesAValue()
    {
        IFeatureEvaluator evaluator = new MissingValueEvaluator();
        var scope = evaluator.CreateScope(FeatureEvaluationContext.ForTargetingKey(User));

        scope.GetStringValue(Key, "default").ShouldBe("default");
        scope.GetBooleanValue("boolean", false).ShouldBeFalse();
        scope.GetInt32Value(Key, 7).ShouldBe(7);
        scope.GetInt64Value(Key, 7L).ShouldBe(7L);
        scope.GetDoubleValue(Key, 7d).ShouldBe(7d);
    }

    [Test]
    public void MissingVariationDetails_UseTheCallersDefaultAndKeepTheReason()
    {
        IFeatureEvaluator evaluator = new MissingValueEvaluator();

        evaluator.StringVariationDetail(Key, "default").Value.ShouldBe("default");
        evaluator.BoolVariationDetail("boolean", false).Value.ShouldBeFalse();
        evaluator.IntVariationDetail(Key, 7).Value.ShouldBe(7);
        var detail = evaluator.DoubleVariationDetail(Key, 7d);
        detail.Value.ShouldBe(7d);
        detail.Exists.ShouldBeFalse();
        detail.ReasonKind.ShouldBe(FeatureEvaluationReasonKind.Missing);
    }

    [Test]
    public void Fallthrough_DefaultDoesNotReplaceTheWeightedChoiceForTargetedContexts()
    {
        var builder = new FeatureSetBuilder();
        builder.Feature(Key).Disabled("default").Variant("treatment", 100, FeatureStatus.Enabled, "targeted");
        var checker = builder.ToChecker();

        var first = checker.Evaluate(Key);
        first.ReasonKind.ShouldBe(FeatureEvaluationReasonKind.Default);
        first.Value.ShouldBe("default");
        var targeted = checker.Evaluate(Key, FeatureEvaluationContext.ForTargetingKey(User));
        targeted.ReasonKind.ShouldBe(FeatureEvaluationReasonKind.Variant);
        targeted.VariationIndex.ShouldBe(0);
        targeted.Value.ShouldBe("targeted");
        checker.Evaluate(Key).ShouldBe(first);
    }

    [Test]
    public void RuleResults_KeepTheirDefinitionAndPositionAcrossConcurrentEvaluations()
    {
        const string secondKey = "other";
        var sharedRule = new FeatureTargetingRule { Name = "paid", Conditions = [FeatureCondition.Equals("plan", "paid")] };
        var priorityRule = new FeatureTargetingRule { Status = FeatureStatus.Debug, Conditions = [FeatureCondition.Equals("role", "staff")] };
        var checker = new FeatureCheckerEvaluator(
            [
                new FeatureDefinition { Key = Key, Value = "first", Rules = [sharedRule] },
                new FeatureDefinition { Key = secondKey, Value = "second", Rules = [priorityRule, sharedRule] }
            ]);
        var paid = FeatureEvaluationContext.ForTargetingKey(User).With("plan", "paid");
        var staff = paid.With("role", "staff");

        Parallel.For(0, 32, _ =>
        {
            var first = checker.Evaluate(Key, paid);
            first.Value.ShouldBe("first");
            (first.Rule?.RuleIndex).ShouldBe(0);
            var second = checker.Evaluate(secondKey, paid);
            second.Value.ShouldBe("second");
            (second.Rule?.RuleIndex).ShouldBe(1);
            (second.Rule?.RuleName).ShouldBe("paid");
            var priority = checker.Evaluate(secondKey, staff);
            priority.IsDebug.ShouldBeTrue();
            (priority.Rule?.RuleIndex).ShouldBe(0);
        });
    }

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

    private sealed class MissingValueEvaluator() : FeatureCheckerEvaluator(Array.Empty<FeatureDefinition>()), IFeatureEvaluator
    {
        FeatureEvaluation IFeatureEvaluator.Evaluate(string featureKey, FeatureEvaluationContext? context) =>
            new()
            {
                Key = featureKey,
                Value = string.Equals(featureKey, "boolean", StringComparison.Ordinal) ? "true" : "42",
                Reason = FeatureEvaluationReasons.Missing,
                ReasonKind = FeatureEvaluationReasonKind.Missing
            };
    }
}

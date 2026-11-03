using System.Globalization;

namespace ManagedCode.FeatureChecker.Tests.Evaluation;

public sealed class FeatureDependencyGraphTests
{
    [Test]
    public void Evaluate_HandlesDeepDependencyChains()
    {
        const int depth = 20_000;
        var builder = new FeatureSetBuilder();
        builder.Feature(Key(0)).Enabled();

        for (var index = 1; index <= depth; index++)
        {
            builder.Feature(Key(index)).Enabled().Require(Key(index - 1));
        }

        var checker = builder.ToChecker();

        checker.Evaluate(Key(depth)).IsEnabled.ShouldBeTrue();
        checker.EvaluateAll()[Key(depth)].IsEnabled.ShouldBeTrue();
        checker.GetFeatureKeysByStatus(FeatureStatus.Enabled).Count.ShouldBe(depth + 1);
    }

    [Test]
    public void Evaluate_HandlesSharedDependenciesAndKeepsFirstFailure()
    {
        var builder = new FeatureSetBuilder();
        builder.Feature("shared").Enabled();
        builder.Feature("left").Enabled().Require("shared");
        builder.Feature("right").Enabled().Require("shared");
        builder.Feature("both").Enabled().Require("left").Require("right");
        builder.Feature("cycle").Enabled().Require("cycle");
        builder.Feature("blocked").Enabled().Require("unknown").Require("cycle");
        var checker = builder.ToChecker();

        checker.Evaluate("both").IsEnabled.ShouldBeTrue();
        checker.Evaluate("cycle").ReasonKind.ShouldBe(FeatureEvaluationReasonKind.DependencyCycle);
        checker.Evaluate("blocked").ReasonKind.ShouldBe(FeatureEvaluationReasonKind.Dependency);

        var evaluations = checker.EvaluateAll();
        foreach (var (key, evaluation) in evaluations)
        {
            evaluation.ShouldBe(checker.Evaluate(key));
        }

        checker.GetFeatureKeysByStatus(FeatureStatus.Disabled).ShouldBe(["cycle", "blocked"]);
    }

    private static string Key(int index) => index.ToString(CultureInfo.InvariantCulture);
}

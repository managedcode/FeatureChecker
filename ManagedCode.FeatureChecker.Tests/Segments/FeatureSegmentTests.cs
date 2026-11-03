namespace ManagedCode.FeatureChecker.Tests.Segments;

public sealed class FeatureSegmentTests
{
    [Test]
    public void RejectedSegmentKeys_LeaveBothMembershipListsUnchanged()
    {
        var builder = new FeatureSegmentBuilder(Segments.Beta).Include(UserIds.Included).Exclude(UserIds.Excluded);

        Should.Throw<ArgumentException>(() => builder.Include("prefix", " "));
        Should.Throw<ArgumentException>(() => builder.Exclude("prefix", " "));

        var unchanged = builder.Build();
        unchanged.IncludedKeys.ShouldBe([UserIds.Included]);
        unchanged.ExcludedKeys.ShouldBe([UserIds.Excluded]);
        builder.Include("retry").Build().IncludedKeys.ShouldBe([UserIds.Included, "retry"]);
    }

    [Test]
    public void SegmentGraph_RevisitsUnresolvedMembershipAfterAnAlternativeRuleMatches()
    {
        var first = new FeatureSegment
        {
            Key = "first",
            Rules = [new FeatureTargetingRule { IncludeSegments = ["second"] }, new FeatureTargetingRule()]
        };
        var second = new FeatureSegment
        {
            Key = "second",
            Rules = [new FeatureTargetingRule { IncludeSegments = ["first"] }]
        };
        var root = new FeatureSegment
        {
            Key = "root",
            Rules =
            [
                new FeatureTargetingRule { ExcludeSegments = ["first"] },
                new FeatureTargetingRule { IncludeSegments = ["second"] }
            ]
        };
        var feature = new FeatureDefinition
        {
            Key = FeatureNames.Reports,
            Rules = [new FeatureTargetingRule { IncludeSegments = ["root"] }]
        };

        new FeatureCheckerEvaluator([feature], [root, first, second]).IsEnabled(FeatureNames.Reports).ShouldBeTrue();
    }

    [Test]
    public void SegmentCycles_CannotGrantMembershipThroughExclusions()
    {
        const string cycle = "cycle";
        const string throughInclude = "through-include";
        const string throughExclude = "through-exclude";
        var segment = new FeatureSegment
        {
            Key = cycle,
            Rules = [new FeatureTargetingRule { ExcludeSegments = [cycle] }]
        };
        var checker = new FeatureCheckerEvaluator(
            [
                new FeatureDefinition { Key = throughInclude, Rules = [new FeatureTargetingRule { IncludeSegments = [cycle] }] },
                new FeatureDefinition { Key = throughExclude, Rules = [new FeatureTargetingRule { ExcludeSegments = [cycle] }] }
            ],
            [segment]);

        checker.IsEnabled(throughInclude).ShouldBeFalse();
        checker.IsEnabled(throughExclude).ShouldBeFalse();
    }

    [Test]
    public void NestedSegments_HandleDeepAcyclicChains()
    {
        const int depth = 12_000;
        var segments = new List<FeatureSegment>(depth + 1)
        {
            new() { Key = Segments.Beta, IncludedKeys = [UserIds.Included] }
        };
        for (var index = 1; index <= depth; index++)
        {
            var child = segments[^1].Key;
            segments.Add(new FeatureSegment
            {
                Key = index.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Rules = [new FeatureTargetingRule { IncludeSegments = [child] }]
            });
        }

        var feature = new FeatureDefinition
        {
            Key = FeatureNames.Reports,
            Rules = [new FeatureTargetingRule { IncludeSegments = [segments[^1].Key] }]
        };
        var checker = new FeatureCheckerEvaluator([feature], segments);

        checker.IsEnabled(FeatureNames.Reports, FeatureEvaluationContext.ForTargetingKey(UserIds.Included)).ShouldBeTrue();
        checker.IsEnabled(FeatureNames.Reports, FeatureEvaluationContext.ForTargetingKey(UserIds.Excluded)).ShouldBeFalse();
    }

    [Test]
    public void FeatureChecker_ShouldEvaluateListAndRuleBasedSegments()
    {
        var builder = new FeatureSetBuilder();
        builder.Segment(Segments.Beta)
            .Include(UserIds.Included)
            .Exclude(UserIds.Excluded)
            .WhenAll([FeatureCondition.Equals(FeatureContextAttributeNames.Plan, Values.Enterprise)]);
        builder.Feature(FeatureNames.Reports).Disabled().WhenSegment(Segments.Beta);

        var checker = builder.ToChecker();
        var included = FeatureEvaluationContextBuilder.Create().ForUser(UserIds.Included).Build();
        var excluded = FeatureEvaluationContextBuilder.Create().ForUser(UserIds.Excluded).WithPlan(Values.Enterprise).Build();
        var ruleMatched = FeatureEvaluationContextBuilder.Create().ForUser(UserIds.RuleMatched).WithPlan(Values.Enterprise).Build();

        checker.IsEnabled(FeatureNames.Reports, included).ShouldBeTrue();
        checker.IsDisabled(FeatureNames.Reports, excluded).ShouldBeTrue();
        checker.IsEnabled(FeatureNames.Reports, ruleMatched).ShouldBeTrue();
    }

    private static class FeatureNames
    {
        public const string Reports = "reports.beta";
    }

    private static class Segments
    {
        public const string Beta = "beta-users";
    }

    private static class UserIds
    {
        public const string Included = "user-included";
        public const string Excluded = "user-excluded";
        public const string RuleMatched = "user-rule-matched";
    }

    private static class Values
    {
        public const string Enterprise = "enterprise";
    }
}

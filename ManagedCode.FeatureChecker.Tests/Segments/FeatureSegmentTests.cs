namespace ManagedCode.FeatureChecker.Tests.Segments;

public sealed class FeatureSegmentTests
{
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

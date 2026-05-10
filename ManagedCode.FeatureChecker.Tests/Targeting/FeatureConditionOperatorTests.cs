namespace ManagedCode.FeatureChecker.Tests.Targeting;

public sealed class FeatureConditionOperatorTests
{
    [Test]
    public void FeatureChecker_ShouldRejectMissingAttributesForPositiveConditionOperators()
    {
        var checker = CreateChecker(
            (FeatureNames.EqualsMissing, FeatureCondition.Equals(AttributeNames.Plan, AttributeValues.Enterprise)),
            (FeatureNames.ContainsMissing, FeatureCondition.Contains(AttributeNames.Email, AttributeValues.CompanyDomain)),
            (FeatureNames.StartsWithMissing, FeatureCondition.StartsWith(AttributeNames.Email, "person@")),
            (FeatureNames.EndsWithMissing, FeatureCondition.EndsWith(AttributeNames.Email, ".com")),
            (FeatureNames.VersionMissing, FeatureCondition.VersionAtLeast(AttributeNames.Version, "2.0.0")));
        var context = FeatureEvaluationContext.ForTargetingKey(TargetingKeys.UserA);

        checker.IsDisabled(FeatureNames.EqualsMissing, context).ShouldBeTrue();
        checker.IsDisabled(FeatureNames.ContainsMissing, context).ShouldBeTrue();
        checker.IsDisabled(FeatureNames.StartsWithMissing, context).ShouldBeTrue();
        checker.IsDisabled(FeatureNames.EndsWithMissing, context).ShouldBeTrue();
        checker.IsDisabled(FeatureNames.VersionMissing, context).ShouldBeTrue();
    }

    [Test]
    public void FeatureChecker_ShouldEvaluateNegativeOperatorsAgainstActualAttributes()
    {
        var checker = CreateChecker(
            (FeatureNames.NotEqualsMatch, FeatureCondition.NotEquals(AttributeNames.Plan, AttributeValues.Free)),
            (FeatureNames.NotEqualsMismatch, FeatureCondition.NotEquals(AttributeNames.Plan, AttributeValues.Enterprise)),
            (FeatureNames.NotInMatch, FeatureCondition.NotIn(AttributeNames.Region, AttributeValues.Apac)),
            (FeatureNames.NotInMismatch, FeatureCondition.NotIn(AttributeNames.Region, AttributeValues.Eu)),
            (FeatureNames.NotExistsMismatch, FeatureCondition.NotExists(AttributeNames.Plan)),
            (FeatureNames.InvalidVersion, FeatureCondition.VersionAtLeast(AttributeNames.Version, "2.0.0")));
        var context = FeatureEvaluationContext
            .ForTargetingKey(TargetingKeys.UserA)
            .With(AttributeNames.Plan, AttributeValues.Enterprise)
            .With(AttributeNames.Region, AttributeValues.Eu)
            .With(AttributeNames.Version, AttributeValues.InvalidVersion);

        checker.IsEnabled(FeatureNames.NotEqualsMatch, context).ShouldBeTrue();
        checker.IsDisabled(FeatureNames.NotEqualsMismatch, context).ShouldBeTrue();
        checker.IsEnabled(FeatureNames.NotInMatch, context).ShouldBeTrue();
        checker.IsDisabled(FeatureNames.NotInMismatch, context).ShouldBeTrue();
        checker.IsDisabled(FeatureNames.NotExistsMismatch, context).ShouldBeTrue();
        checker.IsDisabled(FeatureNames.InvalidVersion, context).ShouldBeTrue();
    }

    private static FeatureCheckerEvaluator CreateChecker(params (string Key, FeatureCondition Condition)[] cases)
    {
        var definitions = new List<FeatureDefinition>(cases.Length);

        foreach (var testCase in cases)
        {
            definitions.Add(
                new FeatureDefinition
                {
                    Key = testCase.Key,
                    Status = FeatureStatus.Disabled,
                    Rules =
                    [
                        new FeatureTargetingRule
                        {
                            Status = FeatureStatus.Enabled,
                            Conditions = [testCase.Condition]
                        }
                    ]
                });
        }

        return new FeatureCheckerEvaluator(FeatureSnapshot.FromDefinitions(definitions));
    }

    private static class FeatureNames
    {
        public const string EqualsMissing = "condition.equals.missing";
        public const string ContainsMissing = "condition.contains.missing";
        public const string StartsWithMissing = "condition.starts-with.missing";
        public const string EndsWithMissing = "condition.ends-with.missing";
        public const string VersionMissing = "condition.version.missing";
        public const string NotEqualsMatch = "condition.not-equals.match";
        public const string NotEqualsMismatch = "condition.not-equals.mismatch";
        public const string NotInMatch = "condition.not-in.match";
        public const string NotInMismatch = "condition.not-in.mismatch";
        public const string NotExistsMismatch = "condition.not-exists.mismatch";
        public const string InvalidVersion = "condition.version.invalid";
    }

    private static class AttributeNames
    {
        public const string Email = "email";
        public const string Plan = "plan";
        public const string Region = "region";
        public const string Version = "version";
    }

    private static class AttributeValues
    {
        public const string Enterprise = "enterprise";
        public const string Free = "free";
        public const string Eu = "eu";
        public const string Apac = "apac";
        public const string CompanyDomain = "managed-code.com";
        public const string InvalidVersion = "preview";
    }

    private static class TargetingKeys
    {
        public const string UserA = "user-a";
    }
}

namespace ManagedCode.FeatureChecker.Tests.Definitions;

public sealed class FeatureDefinitionOwnershipTests
{
    [Test]
    public void Evaluation_UsesTheDefinitionAndSegmentDataCapturedAtInitialization()
    {
        const string key = "reports";
        const string segmentKey = "beta";
        const string target = "user";
        var values = new List<string> { "paid" };
        var conditions = new List<FeatureCondition>
        {
            new() { Attribute = "plan", Values = values }
        };
        var rules = new List<FeatureTargetingRule>
        {
            new() { Conditions = conditions, IncludeSegments = new List<string> { segmentKey } }
        };
        var members = new List<string> { target };
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["owner"] = "product" };
        var definition = new FeatureDefinition { Key = key, Rules = rules, Metadata = metadata };
        var segment = new FeatureSegment { Key = segmentKey, IncludedKeys = members };

        values[0] = "free";
        conditions.Clear();
        rules.Clear();
        members.Clear();
        metadata["owner"] = "other";
        var checker = new FeatureCheckerEvaluator([definition], [segment]);

        definition.Metadata["OWNER"].ShouldBe("product");
        checker.IsEnabled(key, FeatureEvaluationContext.ForTargetingKey(target).With("plan", "paid")).ShouldBeTrue();
        checker.IsEnabled(key, FeatureEvaluationContext.ForTargetingKey(target).With("plan", "free")).ShouldBeFalse();
    }
}

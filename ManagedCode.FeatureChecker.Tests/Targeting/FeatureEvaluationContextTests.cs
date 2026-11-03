namespace ManagedCode.FeatureChecker.Tests.Targeting;

public sealed class FeatureEvaluationContextTests
{
    [Test]
    public void FeatureEvaluationContext_ShouldKeepAttributesImmutableAcrossDerivedContexts()
    {
        var input = new Dictionary<string, string>(StringComparer.Ordinal) { [AttributeNames.Count] = "42" };
        var original = new FeatureEvaluationContext(TargetingKeys.UserA, input);
        var derived = original.WithTargetingKey(TargetingKeys.TenantA).WithContextKind(FeatureContextKinds.Tenant);
        input[AttributeNames.Count] = "43";

        if (derived.Attributes is IDictionary<string, string> { IsReadOnly: false } writable)
        {
            writable[AttributeNames.Count] = "44";
        }

        original.Attributes["COUNT"].ShouldBe("42");
        derived.Attributes["COUNT"].ShouldBe("42");
        derived.With("COUNT", 45).Attributes[AttributeNames.Count].ShouldBe("45");
        derived.Attributes[AttributeNames.Count].ShouldBe("42");
    }

    [Test]
    public void FeatureEvaluationContext_ShouldConvertAttributeValues()
    {
        var context = FeatureEvaluationContext.ForTargetingKey(TargetingKeys.UserA)
            .With(AttributeNames.Empty, null)
            .With(AttributeNames.Count, 42)
            .With(AttributeNames.Ratio, 1.25m)
            .With(AttributeNames.Custom, new CustomValue());

        context.Attributes[AttributeNames.Empty].ShouldBe(string.Empty);
        context.Attributes[AttributeNames.Count].ShouldBe("42");
        context.Attributes[AttributeNames.Ratio].ShouldBe("1.25");
        context.Attributes[AttributeNames.Custom].ShouldBe(CustomValue.Text);
    }

    [Test]
    public void FeatureEvaluationContextBuilder_ShouldConvertAttributeValues()
    {
        var context = FeatureEvaluationContextBuilder.Create()
            .ForTenant(TargetingKeys.TenantA)
            .With(AttributeNames.Empty, null)
            .With(AttributeNames.Count, 42)
            .With(AttributeNames.Ratio, 1.25m)
            .With(AttributeNames.Custom, new CustomValue())
            .Build();

        context.ContextKind.ShouldBe(FeatureContextKinds.Tenant);
        context.TargetingKey.ShouldBe(TargetingKeys.TenantA);
        context.Attributes[AttributeNames.Empty].ShouldBe(string.Empty);
        context.Attributes[AttributeNames.Count].ShouldBe("42");
        context.Attributes[AttributeNames.Ratio].ShouldBe("1.25");
        context.Attributes[AttributeNames.Custom].ShouldBe(CustomValue.Text);
    }

    private sealed class CustomValue
    {
        public const string Text = "custom-value";

        public override string ToString()
        {
            return Text;
        }
    }

    private static class AttributeNames
    {
        public const string Empty = "empty";
        public const string Count = "count";
        public const string Ratio = "ratio";
        public const string Custom = "custom";
    }

    private static class TargetingKeys
    {
        public const string UserA = "user-a";
        public const string TenantA = "tenant-a";
    }
}

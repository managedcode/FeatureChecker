using FluentAssertions;
using Xunit;

namespace ManagedCode.FeatureChecker.Tests;

public class Tests
{
    [Fact]
    public void TestDetectorTest()
    {
        var holder = new FeatureHolder();
        holder.Add(MyEnum.feature1, FeatureStatus.Enabled);
        holder.Add(MyEnum.feature2, FeatureStatus.Disabled);
        holder.Add(MyEnum.feature3, FeatureStatus.Enabled);
        holder.Add(MyEnum.feature4, FeatureStatus.Disabled);
        holder.Add(MyEnum.feature5, FeatureStatus.Debug);

        var checker = new FeatureChecker(holder);

        checker.IsDebug(MyEnum.feature5).Should().BeTrue();
        checker.IsDebug(MyEnum.feature1).Should().BeFalse();

        checker.IsEnabled(MyEnum.feature1).Should().BeTrue();
        checker.IsEnabled(MyEnum.feature5).Should().BeFalse();
        checker.IsEnabled(MyEnum.feature2).Should().BeFalse();

        checker.IsDisabled(MyEnum.feature2).Should().BeTrue();
        checker.IsDisabled(MyEnum.feature3).Should().BeFalse();
        checker.IsDisabled(MyEnum.feature5).Should().BeFalse();
    }
}
using Shouldly;

namespace ManagedCode.FeatureChecker.Tests;

public class FeatureCheckerTests
{
    [Test]
    public void FeatureChecker_ShouldReportConfiguredFeatureStatuses()
    {
        var holder = new FeatureHolder
        {
            [MyEnum.Feature1] = FeatureStatus.Enabled,
            [MyEnum.Feature2] = FeatureStatus.Disabled,
            [MyEnum.Feature3] = FeatureStatus.Enabled,
            [MyEnum.Feature4] = FeatureStatus.Disabled,
            [MyEnum.Feature5] = FeatureStatus.Debug
        };

        var checker = new FeatureChecker(holder);

        checker.Count.ShouldBe(5);
        checker.IsFeatureExists(MyEnum.Feature1).ShouldBeTrue();
        checker.IsFeatureExists(MyEnum.Unknown).ShouldBeFalse();

        checker.IsDebug(MyEnum.Feature5).ShouldBeTrue();
        checker.IsDebug(MyEnum.Feature1).ShouldBeFalse();

        checker.IsEnabled(MyEnum.Feature1).ShouldBeTrue();
        checker.IsEnabled(MyEnum.Feature5).ShouldBeFalse();
        checker.IsEnabled(MyEnum.Feature2).ShouldBeFalse();

        checker.IsDisabled(MyEnum.Feature2).ShouldBeTrue();
        checker.IsDisabled(MyEnum.Feature3).ShouldBeFalse();
        checker.IsDisabled(MyEnum.Feature5).ShouldBeFalse();
    }

    [Test]
    public void FeatureChecker_ShouldReturnFeatureStatusAndFilterByStatus()
    {
        var checker = new FeatureChecker(
            new FeatureHolder
            {
                [MyEnum.Feature1] = FeatureStatus.Enabled,
                [MyEnum.Feature2] = FeatureStatus.Disabled,
                [MyEnum.Feature5] = FeatureStatus.Debug
            });

        checker.TryGetFeatureStatus(MyEnum.Feature5, out var status).ShouldBeTrue();
        status.ShouldBe(FeatureStatus.Debug);

        checker.TryGetFeatureStatus(MyEnum.Unknown, out _).ShouldBeFalse();
        checker.GetFeaturesByStatus(FeatureStatus.Enabled).ShouldBe([MyEnum.Feature1]);
    }
}

namespace ManagedCode.FeatureChecker.Tests.Integration;

[Category(TestCategories.Integration)]
public sealed class FeatureSnapshotFileIntegrationTests
{
    private const string FeatureKey = "checkout";

    [Test]
    public void ConcurrentFileUpdates_ExposeCompleteSnapshotsAndPreserveExistingScopes()
    {
        var path = Path.Combine(AppContext.BaseDirectory, $"{Guid.NewGuid():N}.json");
        var enabled = FeatureSnapshot.FromDefinitions([FeatureDefinition.Create(FeatureKey, FeatureStatus.Enabled)]);
        var disabled = FeatureSnapshot.FromDefinitions([FeatureDefinition.Create(FeatureKey, FeatureStatus.Disabled)]);
        try
        {
            FeatureSnapshotSerializer.Save(path, enabled);
            var factory = new FeatureCheckerFactory(new FeatureFileProvider(path));
            var original = factory.CreateScope();

            Parallel.Invoke(
                () => ReplaceSnapshots(path, enabled, disabled),
                () => ReadSnapshots(factory));

            original.IsEnabled(FeatureKey).ShouldBeTrue();
            factory.CreateScope().IsDisabled(FeatureKey).ShouldBeTrue();
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static void ReplaceSnapshots(string path, FeatureSnapshot enabled, FeatureSnapshot disabled)
    {
        for (var index = 0; index < 32; index++)
        {
            FeatureSnapshotSerializer.Save(path, index % 2 == 0 ? enabled : disabled);
        }
    }

    private static void ReadSnapshots(FeatureCheckerFactory factory)
    {
        for (var index = 0; index < 256; index++)
        {
            var evaluation = factory.CreateScope().Evaluate(FeatureKey);
            evaluation.Exists.ShouldBeTrue();
            evaluation.ReasonKind.ShouldBe(FeatureEvaluationReasonKind.Default);
        }
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.FeatureChecker.Tests.Integration;

[Category(TestCategories.Integration)]
public sealed class FeatureSnapshotSourceIntegrationTests
{
    private const string FeatureKey = "checkout";
    private const string User = "user";

    [Test]
    public void ConcurrentScopes_UseOneCompleteBackendSnapshotEach()
    {
        const int scopes = 64;
        var source = new AlternatingSource();
        using var services = new ServiceCollection()
            .AddFeatureCheckerSnapshotSource(source)
            .BuildServiceProvider();
        var factory = services.GetRequiredService<IFeatureCheckerFactory>();
        var context = FeatureEvaluationContext.ForTargetingKey(User);

        Parallel.For(0, scopes, _ => factory.CreateScope(context).IsEnabled(FeatureKey).ShouldBeTrue());

        source.Calls.ShouldBe(scopes);
    }

    private sealed class AlternatingSource : IFeatureSnapshotSource
    {
        private readonly FeatureSnapshot _first = CreateSnapshot("first");
        private readonly FeatureSnapshot _second = CreateSnapshot("second");
        private int _calls;

        public int Calls => Volatile.Read(ref _calls);

        public FeatureSnapshot GetSnapshot() => Interlocked.Increment(ref _calls) % 2 == 0 ? _first : _second;

        private static FeatureSnapshot CreateSnapshot(string segment)
        {
            var builder = new FeatureSetBuilder();
            builder.Segment(segment).Include(User);
            builder.Feature(FeatureKey).Disabled().WhenSegment(segment);
            return builder.Build();
        }
    }
}

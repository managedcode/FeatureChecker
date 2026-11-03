using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.FeatureChecker.Tests.DependencyInjection;

public sealed class FeatureCheckerProviderLifetimeTests
{
    private const string FeatureKey = "checkout";

    [Test]
    public void GenericSnapshotSource_IsDisposedOnceWhenResolvedThroughItsContracts()
    {
        var services = new ServiceCollection()
            .AddFeatureCheckerSnapshotSource<DisposableSnapshotSource>()
            .BuildServiceProvider();
        DisposableSnapshotSource source;
        using (services)
        {
            source = services.GetRequiredService<DisposableSnapshotSource>();
            services.GetRequiredService<IFeatureSnapshotSource>().GetSnapshot().Features.Count.ShouldBe(1);
            services.GetRequiredService<IFeatureEvaluator>().IsEnabled(FeatureKey).ShouldBeTrue();
        }

        source.Disposals.ShouldBe(1);
    }

    [Test]
    public void DefinitionProvider_IsDisposedOnceWhenResolvedThroughItsContracts()
    {
        var services = new ServiceCollection()
            .AddFeatureCheckerProvider<DisposableDefinitionProvider>()
            .BuildServiceProvider();
        DisposableDefinitionProvider source;
        using (services)
        {
            source = services.GetRequiredService<DisposableDefinitionProvider>();
            services.GetRequiredService<IFeatureEvaluator>().IsEnabled(FeatureKey).ShouldBeTrue();
            services.GetRequiredService<IFeatureSegmentProvider>().GetFeatureSegments().ShouldBeEmpty();
        }

        source.Disposals.ShouldBe(1);
    }

    private sealed class DisposableSnapshotSource : IFeatureSnapshotSource, IDisposable
    {
        public int Disposals { get; private set; }

        public FeatureSnapshot GetSnapshot() => FeatureSnapshot.FromDefinitions([FeatureDefinition.Create(FeatureKey, FeatureStatus.Enabled)]);

        public void Dispose() => Disposals++;
    }

    private sealed class DisposableDefinitionProvider : IFeatureDefinitionProvider, IFeatureSegmentProvider, IDisposable
    {
        public int Disposals { get; private set; }

        public IReadOnlyCollection<FeatureDefinition> GetFeatureDefinitions() => [FeatureDefinition.Create(FeatureKey, FeatureStatus.Enabled)];

        public IReadOnlyCollection<FeatureSegment> GetFeatureSegments() => [];

        public void Dispose() => Disposals++;
    }
}

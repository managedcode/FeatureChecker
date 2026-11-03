using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.FeatureChecker.Tests.DependencyInjection;

public sealed class FeatureCheckerProviderLifetimeTests
{
    private const string FeatureKey = "checkout";

    [Test]
    public void InterfaceRegistration_ReusesAnExistingDefinitionProvider()
    {
        using var provider = new DisposableDefinitionProvider();
        var registrations = new ServiceCollection().AddSingleton<IFeatureDefinitionProvider>(provider);
        var original = registrations.Single();

        registrations.AddFeatureCheckerProvider<IFeatureDefinitionProvider>();

        registrations.ShouldContain(original);
        using (var services = registrations.BuildServiceProvider())
        {
            services.GetRequiredService<IFeatureDefinitionProvider>().ShouldBeSameAs(provider);
            services.GetRequiredService<IFeatureEvaluator>().IsEnabled(FeatureKey).ShouldBeTrue();
        }

        provider.Disposals.ShouldBe(0);
    }

    [Test]
    public void InterfaceRegistration_ReusesAnExistingSnapshotSource()
    {
        using var source = new DisposableSnapshotSource();
        var registrations = new ServiceCollection().AddSingleton<IFeatureSnapshotSource>(source);
        var original = registrations.Single();

        registrations.AddFeatureCheckerSnapshotSource<IFeatureSnapshotSource>();

        registrations.ShouldContain(original);
        using (var services = registrations.BuildServiceProvider())
        {
            services.GetRequiredService<IFeatureSnapshotSource>().ShouldBeSameAs(source);
            services.GetRequiredService<IFeatureCheckerFactory>().Create().IsEnabled(FeatureKey).ShouldBeTrue();
        }

        source.Disposals.ShouldBe(0);
    }

    [Test]
    public void InterfaceRegistration_RequiresAnExistingDefaultContract()
    {
        var services = new ServiceCollection();

        Should.Throw<InvalidOperationException>(() => services.AddFeatureCheckerProvider<IFeatureDefinitionProvider>());
        Should.Throw<InvalidOperationException>(() => services.AddFeatureCheckerSnapshotSource<IFeatureSnapshotSource>());
        services.ShouldBeEmpty();
    }

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

    [Test]
    public void CombinedProvider_PreservesDirectCallsAndUsesOneSnapshotForEvaluation()
    {
        var services = new ServiceCollection()
            .AddFeatureCheckerProvider<DisposableCombinedProvider>()
            .BuildServiceProvider();
        DisposableCombinedProvider source;
        using (services)
        {
            source = services.GetRequiredService<DisposableCombinedProvider>();
            services.GetRequiredService<IFeatureDefinitionProvider>().GetFeatureDefinitions().Single().Key.ShouldBe("direct");
            services.GetRequiredService<IFeatureSegmentProvider>().GetFeatureSegments().Single().Key.ShouldBe("direct-segment");
            source.SnapshotReads.ShouldBe(0);

            services.GetRequiredService<IFeatureEvaluator>().IsEnabled(FeatureKey).ShouldBeTrue();
            source.SnapshotReads.ShouldBe(1);
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

    private sealed class DisposableCombinedProvider : IFeatureDefinitionProvider, IFeatureSegmentProvider, IFeatureSnapshotSource, IDisposable
    {
        public int Disposals { get; private set; }

        public int SnapshotReads { get; private set; }

        public IReadOnlyCollection<FeatureDefinition> GetFeatureDefinitions() => [FeatureDefinition.Create("direct", FeatureStatus.Debug)];

        public IReadOnlyCollection<FeatureSegment> GetFeatureSegments() => [new FeatureSegment { Key = "direct-segment" }];

        public FeatureSnapshot GetSnapshot()
        {
            SnapshotReads++;
            return FeatureSnapshot.FromDefinitions([FeatureDefinition.Create(FeatureKey, FeatureStatus.Enabled)]);
        }

        public void Dispose() => Disposals++;
    }
}

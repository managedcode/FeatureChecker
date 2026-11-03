using ManagedCode.FeatureChecker.Access;
using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Evaluation;
using ManagedCode.FeatureChecker.Segments;
using ManagedCode.FeatureChecker.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ManagedCode.FeatureChecker.DependencyInjection;

public static class FeatureCheckerServiceCollectionExtensions
{
    public static IServiceCollection AddFeatureChecker(this IServiceCollection services, Action<FeatureCheckerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<FeatureCheckerOptions>().Configure(configure);

        return AddOptionsFeatureProvider(services).AddFeatureCheckerCore();
    }

    public static IServiceCollection AddFeatureChecker(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<FeatureCheckerOptions>(configuration);

        return AddOptionsFeatureProvider(services).AddFeatureCheckerCore();
    }

    public static IServiceCollection AddFeatureCheckerProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IFeatureDefinitionProvider
    {
        ArgumentNullException.ThrowIfNull(services);

        if (typeof(TProvider) == typeof(IFeatureDefinitionProvider))
        {
            RequireDefaultRegistration<TProvider>(services);
            return services.AddFeatureCheckerCore();
        }

        services.TryAddSingleton<TProvider>();
        services.RemoveAll<IFeatureDefinitionProvider>();
        services.AddSingleton<IFeatureDefinitionProvider>(provider => GetDefinitionContract(provider.GetRequiredService<TProvider>()));
        services.RemoveAll<IFeatureSegmentProvider>();

        if (typeof(IFeatureSegmentProvider).IsAssignableFrom(typeof(TProvider)))
        {
            services.AddSingleton<IFeatureSegmentProvider>(provider => (IFeatureSegmentProvider)provider.GetRequiredService<IFeatureDefinitionProvider>());
        }

        return services.AddFeatureCheckerCore();
    }

    public static IServiceCollection AddFeatureCheckerSnapshotSource<TSource>(this IServiceCollection services)
        where TSource : class, IFeatureSnapshotSource
    {
        ArgumentNullException.ThrowIfNull(services);

        if (typeof(TSource) == typeof(IFeatureSnapshotSource))
        {
            RequireDefaultRegistration<TSource>(services);
            return AddSnapshotSourceProvider(services, static provider => provider.GetRequiredService<TSource>()).AddFeatureCheckerCore();
        }

        services.TryAddSingleton<TSource>();
        services.RemoveAll<IFeatureSnapshotSource>();
        services.AddSingleton<IFeatureSnapshotSource>(provider =>
        {
            var source = provider.GetRequiredService<TSource>();
            return source is IDisposable or IAsyncDisposable
                ? provider.GetRequiredService<FeatureSnapshotSourceProvider>()
                : source;
        });

        return AddSnapshotSourceProvider(services, static provider => provider.GetRequiredService<TSource>()).AddFeatureCheckerCore();
    }

    public static IServiceCollection AddFeatureCheckerSnapshotSource(this IServiceCollection services, IFeatureSnapshotSource source)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(source);

        services.RemoveAll<IFeatureSnapshotSource>();
        services.AddSingleton(source);

        return AddSnapshotSourceProvider(services, static provider => provider.GetRequiredService<IFeatureSnapshotSource>()).AddFeatureCheckerCore();
    }

    private static IServiceCollection AddOptionsFeatureProvider(IServiceCollection services)
    {
        if (HasDefaultRegistration<IFeatureDefinitionProvider>(services))
        {
            return services;
        }

        services.TryAddSingleton<OptionsFeatureDefinitionProvider>();
        services.TryAddSingleton<IFeatureDefinitionProvider>(provider => provider.GetRequiredService<OptionsFeatureDefinitionProvider>());
        services.TryAddSingleton<IFeatureSegmentProvider>(provider => provider.GetRequiredService<OptionsFeatureDefinitionProvider>());

        return services;
    }

    private static void RequireDefaultRegistration<TService>(IServiceCollection services)
    {
        if (!HasDefaultRegistration<TService>(services))
        {
            throw new InvalidOperationException($"Register '{typeof(TService).Name}' before selecting it as the feature checker source.");
        }
    }

    private static bool HasDefaultRegistration<TService>(IServiceCollection services) =>
        services.Any(static service => !service.IsKeyedService && service.ServiceType == typeof(TService));

    private static IServiceCollection AddSnapshotSourceProvider(
        IServiceCollection services,
        Func<IServiceProvider, IFeatureSnapshotSource> resolveSource)
    {
        services.RemoveAll<FeatureSnapshotSourceProvider>();
        services.AddSingleton(provider => new FeatureSnapshotSourceProvider(resolveSource(provider)));
        services.RemoveAll<IFeatureDefinitionProvider>();
        services.AddSingleton<IFeatureDefinitionProvider>(provider => provider.GetRequiredService<FeatureSnapshotSourceProvider>());
        services.RemoveAll<IFeatureSegmentProvider>();
        services.AddSingleton<IFeatureSegmentProvider>(provider => provider.GetRequiredService<FeatureSnapshotSourceProvider>());

        return services;
    }

    private static IFeatureDefinitionProvider GetDefinitionContract(IFeatureDefinitionProvider provider)
    {
        // Disposable aliases would register the same object with the container more than once.
        if (provider is not (IDisposable or IAsyncDisposable))
        {
            return provider;
        }

        var definitions = new DefinitionProviderAdapter(provider);
        return provider is IFeatureSnapshotSource source
            ? new SnapshotDefinitionProviderAdapter(definitions, source)
            : definitions;
    }

    private static IServiceCollection AddFeatureCheckerCore(this IServiceCollection services)
    {
        services.TryAddSingleton<IFeatureCheckerFactory>(provider => new FeatureCheckerFactory(provider.GetRequiredService<IFeatureDefinitionProvider>()));
        services.TryAddTransient<IFeatureEvaluator>(provider => provider.GetRequiredService<IFeatureCheckerFactory>().Create());

        return services;
    }

    private sealed class DefinitionProviderAdapter(IFeatureDefinitionProvider provider) : IFeatureDefinitionProvider, IFeatureSegmentProvider
    {
        public IReadOnlyCollection<FeatureDefinition> GetFeatureDefinitions() => provider.GetFeatureDefinitions();

        public IReadOnlyCollection<FeatureSegment> GetFeatureSegments() =>
            provider is IFeatureSegmentProvider segmentProvider ? segmentProvider.GetFeatureSegments() : [];
    }

    private sealed class SnapshotDefinitionProviderAdapter(DefinitionProviderAdapter definitions, IFeatureSnapshotSource source)
        : IFeatureDefinitionProvider, IFeatureSegmentProvider, IFeatureSnapshotSource
    {
        public IReadOnlyCollection<FeatureDefinition> GetFeatureDefinitions() => definitions.GetFeatureDefinitions();

        public IReadOnlyCollection<FeatureSegment> GetFeatureSegments() => definitions.GetFeatureSegments();

        public FeatureSnapshot GetSnapshot() => source.GetSnapshot();
    }
}

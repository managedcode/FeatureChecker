using ManagedCode.FeatureChecker.Access;
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

        services.TryAddSingleton<TProvider>();
        services.RemoveAll<IFeatureDefinitionProvider>();
        services.AddSingleton<IFeatureDefinitionProvider>(provider => provider.GetRequiredService<TProvider>());
        services.RemoveAll<IFeatureSegmentProvider>();

        if (typeof(IFeatureSegmentProvider).IsAssignableFrom(typeof(TProvider)))
        {
            services.AddSingleton<IFeatureSegmentProvider>(provider => (IFeatureSegmentProvider)provider.GetRequiredService<TProvider>());
        }

        return services.AddFeatureCheckerCore();
    }

    public static IServiceCollection AddFeatureCheckerSnapshotSource<TSource>(this IServiceCollection services)
        where TSource : class, IFeatureSnapshotSource
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<TSource>();
        services.RemoveAll<IFeatureSnapshotSource>();
        services.AddSingleton<IFeatureSnapshotSource>(provider => provider.GetRequiredService<TSource>());

        return AddSnapshotSourceProvider(services).AddFeatureCheckerCore();
    }

    public static IServiceCollection AddFeatureCheckerSnapshotSource(this IServiceCollection services, IFeatureSnapshotSource source)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(source);

        services.RemoveAll<IFeatureSnapshotSource>();
        services.AddSingleton(source);

        return AddSnapshotSourceProvider(services).AddFeatureCheckerCore();
    }

    private static IServiceCollection AddOptionsFeatureProvider(IServiceCollection services)
    {
        services.TryAddSingleton<OptionsFeatureDefinitionProvider>();
        services.TryAddSingleton<IFeatureDefinitionProvider>(provider => provider.GetRequiredService<OptionsFeatureDefinitionProvider>());
        services.TryAddSingleton<IFeatureSegmentProvider>(provider => provider.GetRequiredService<OptionsFeatureDefinitionProvider>());

        return services;
    }

    private static IServiceCollection AddSnapshotSourceProvider(IServiceCollection services)
    {
        services.RemoveAll<FeatureSnapshotSourceProvider>();
        services.AddSingleton<FeatureSnapshotSourceProvider>();
        services.RemoveAll<IFeatureDefinitionProvider>();
        services.AddSingleton<IFeatureDefinitionProvider>(provider => provider.GetRequiredService<FeatureSnapshotSourceProvider>());
        services.RemoveAll<IFeatureSegmentProvider>();
        services.AddSingleton<IFeatureSegmentProvider>(provider => provider.GetRequiredService<FeatureSnapshotSourceProvider>());

        return services;
    }

    private static IServiceCollection AddFeatureCheckerCore(this IServiceCollection services)
    {
        services.TryAddSingleton<IFeatureCheckerFactory>(provider => new FeatureCheckerFactory(provider.GetRequiredService<IFeatureDefinitionProvider>()));
        services.TryAddTransient<IFeatureEvaluator>(provider => provider.GetRequiredService<IFeatureCheckerFactory>().Create());

        return services;
    }
}

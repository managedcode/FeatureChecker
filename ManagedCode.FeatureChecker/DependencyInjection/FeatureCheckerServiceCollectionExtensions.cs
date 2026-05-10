using ManagedCode.FeatureChecker.Access;
using ManagedCode.FeatureChecker.Evaluation;
using ManagedCode.FeatureChecker.Segments;
using ManagedCode.FeatureChecker.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.FeatureChecker.DependencyInjection;

public static class FeatureCheckerServiceCollectionExtensions
{
    public static IServiceCollection AddFeatureChecker(this IServiceCollection services, Action<FeatureCheckerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<FeatureCheckerOptions>().Configure(configure);

        return AddFeatureCheckerCore(services);
    }

    public static IServiceCollection AddFeatureChecker(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<FeatureCheckerOptions>(configuration);

        return AddFeatureCheckerCore(services);
    }

    private static IServiceCollection AddFeatureCheckerCore(IServiceCollection services)
    {
        services.AddSingleton<OptionsFeatureDefinitionProvider>();
        services.AddSingleton<IFeatureDefinitionProvider>(provider => provider.GetRequiredService<OptionsFeatureDefinitionProvider>());
        services.AddSingleton<IFeatureSegmentProvider>(provider => provider.GetRequiredService<OptionsFeatureDefinitionProvider>());
        services.AddSingleton<IFeatureCheckerFactory>(provider => new FeatureCheckerFactory(provider.GetRequiredService<IFeatureDefinitionProvider>()));
        services.AddTransient<IFeatureEvaluator>(provider => provider.GetRequiredService<IFeatureCheckerFactory>().Create());

        return services;
    }
}

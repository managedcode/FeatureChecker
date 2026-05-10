using ManagedCode.FeatureChecker.Targeting;

namespace ManagedCode.FeatureChecker.Access;

public static class FeatureCheckerFactoryExtensions
{
    public static IFeatureScope ForUser(this IFeatureCheckerFactory factory, string userId, Action<FeatureEvaluationContextBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return CreateScope(factory, FeatureEvaluationContextBuilder.Create().ForUser(userId), configure);
    }

    public static IFeatureScope ForTenant(this IFeatureCheckerFactory factory, string tenantId, Action<FeatureEvaluationContextBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return CreateScope(factory, FeatureEvaluationContextBuilder.Create().ForTenant(tenantId), configure);
    }

    public static IFeatureScope ForSession(this IFeatureCheckerFactory factory, string sessionId, Action<FeatureEvaluationContextBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return CreateScope(factory, FeatureEvaluationContextBuilder.Create().ForSession(sessionId), configure);
    }

    private static IFeatureScope CreateScope(IFeatureCheckerFactory factory, FeatureEvaluationContextBuilder builder, Action<FeatureEvaluationContextBuilder>? configure)
    {
        configure?.Invoke(builder);

        return factory.CreateScope(builder.Build());
    }
}

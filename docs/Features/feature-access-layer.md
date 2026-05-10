# Feature Access Layer

This document captures the application-facing component layer on top of the core evaluator.

## Component Map

| Need | Component |
| --- | --- |
| Provider-backed feature definitions | `IFeatureDefinitionProvider` |
| Application-facing evaluation | `IFeatureEvaluator` |
| Request-bound evaluation | `IFeatureCheckerFactory` and `IFeatureScope` |
| Typed values with explicit defaults | `FeatureEvaluatorExtensions` |
| Context construction | `FeatureEvaluationContextBuilder` |
| Ordered targeting rules | `FeatureTargetingRule` and `FeatureCondition` |
| Reusable audiences | `FeatureSegment` and `FeatureSegmentBuilder` |

## Components

- `IFeatureSetBuilder` and `IFeatureDefinitionBuilder`: interface seams for composing flags.
- `IFeatureCheckerFactory`: issues fresh evaluators from a definition provider.
- `IFeatureScope`: binds one evaluator to one context for a request or operation.
- `FeatureEvaluationContextBuilder`: builds user, tenant, session, plan, role, and region context without framework dependencies.
- `FeatureCheckerFactoryExtensions`: controller-friendly `ForUser`, `ForTenant`, and `ForSession` helpers.
- `FeatureEvaluatorExtensions`: typed value helpers with explicit caller-provided default values.

## Controller Shape

```csharp
public sealed class CheckoutController(IFeatureCheckerFactory featureCheckers)
{
    public bool CanUseNewCheckout(string userId, string tenantId)
    {
        var features = featureCheckers.ForUser(
            userId,
            context => context.WithTenantId(tenantId).WithPlan("enterprise"));

        return features.IsEnabled("checkout.new-flow");
    }
}
```

The core package intentionally does not reference ASP.NET Core. A future adapter package can map `HttpContext`, claims, route values, and dependency injection registrations onto these core interfaces.

## Typed Values

Typed helpers use explicit caller-provided defaults:

1. Evaluate the feature.
2. Read the string `Value`.
3. Parse using invariant culture where relevant.
4. Return the caller-supplied default when the feature is missing, has no value, or cannot be parsed.

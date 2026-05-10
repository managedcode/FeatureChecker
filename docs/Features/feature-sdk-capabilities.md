# SDK-Side Feature Management Capabilities

## Intent

This feature slice adds SDK-side capabilities that are useful inside .NET applications without cloning a hosted control plane.

## Implemented Capabilities

| Capability | Library API |
| --- | --- |
| Individual context targeting | `FeatureDefinition.Targets`, `FeatureDefinitionBuilder.Target` |
| Reusable user/group segments | `FeatureSegment`, `FeatureSegmentBuilder`, `FeatureTargetingRule.IncludeSegments` |
| Rule-based segments | `FeatureSegment.Rules` with `FeatureCondition` |
| Segment exclusion | `FeatureSegment.ExcludedKeys` wins over includes and rules |
| Context kinds | `FeatureEvaluationContext.ContextKind`, `FeatureContextKinds` |
| App/device targeting attributes | `WithApplication`, `WithDevice`, `WithEnvironment`, `WithCountry`, `WithGroup`, `WithEmail` |
| Minimum version gates | `FeatureCondition.VersionAtLeast`, `WhenApplicationVersionAtLeast` |
| Flag modes | `FeatureMode.Release`, `Experiment`, `Operational`, `Permission`, `Migration`, `Maintenance` |
| Off behavior | `Targeting(false)` plus `OffVariation` |
| Default rule behavior | `FallthroughVariation` and weighted variants |
| Evaluation details | `FeatureEvaluation.ReasonKind`, `VariationIndex`, `Rule` |
| Typed details | `StringVariationDetail`, `BoolVariationDetail`, `IntVariationDetail`, `DoubleVariationDetail` |
| .NET host integration | `AddFeatureChecker(IConfiguration)` and `AddFeatureChecker(Action<FeatureCheckerOptions>)` |

## Evaluation Order

1. Missing key returns a missing evaluation.
2. Targeting-off flags return the off variation/value.
3. Dependencies are evaluated recursively with cycle protection.
4. Individual targets are checked by context key and optional context kind.
5. Rules are evaluated in order, including segment include/exclude requirements.
6. Fallthrough variation is used when configured.
7. Weighted variants are selected deterministically by feature key and targeting key.
8. Default status/value is returned.

## Deferred Platform Capabilities

The core package does not implement dashboards, approvals, audit logs, scheduled changes, event export, OpenTelemetry hooks, automatic rollback, or hosted experiment analysis. ADR 0001 keeps those as adapter/control-plane responsibilities.

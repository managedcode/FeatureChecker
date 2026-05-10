# ManagedCode.FeatureChecker

[![CI](https://github.com/managedcode/FeatureChecker/actions/workflows/dotnet.yml/badge.svg)](https://github.com/managedcode/FeatureChecker/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/ManagedCode.FeatureChecker.svg)](https://www.nuget.org/packages/ManagedCode.FeatureChecker)

`ManagedCode.FeatureChecker` is a provider-agnostic .NET feature flag SDK for feature management, feature toggles, targeting, segmentation, percentage rollouts, variants, remote configuration values, and deterministic local evaluation.

It is built for .NET developers who want a clean in-process API first: no dashboard dependency, no cloud SDK dependency in the core package, and no hidden runtime fallback path. Feature definitions can be built in code, bound from Microsoft.Extensions configuration, loaded from JSON snapshots, or supplied by your own storage/provider adapter.

## Why FeatureChecker

- .NET-native feature flag evaluator with explicit interfaces for application code.
- Target features by user, tenant, session, role, plan, region, country, environment, app version, device, or any custom attribute.
- Use individual targets, reusable segments, ordered targeting rules, dependencies, kill switches, and deterministic percentage rollouts.
- Return evaluation reasons, matching rules, selected variants, variation indexes, and typed remote-config values.
- Keep the core package storage-neutral through `FeatureSnapshot` and `IFeatureDefinitionProvider`.
- Integrate directly with `Microsoft.Extensions.DependencyInjection`, `Microsoft.Extensions.Configuration`, and options binding.
- Ship through CI with format, build, analyzer, test, coverage, pack, NuGet, and GitHub Release gates.

## Keywords

feature flags, feature toggles, feature management, .NET SDK, C#, targeting, segmentation, percentage rollout, gradual rollout, variants, remote config, kill switch, A/B testing, experiments, permissions, migrations, JSON snapshots, Microsoft.Extensions, deterministic local evaluation.

## Install

```bash
dotnet add package ManagedCode.FeatureChecker
```

## Quick Start

```csharp
using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Evaluation;
using ManagedCode.FeatureChecker.Targeting;

var features = new FeatureSetBuilder();

features.Feature("checkout.new-flow")
    .Disabled()
    .WhenAll(
        [FeatureCondition.Equals("plan", "enterprise")],
        percentage: 25);

IFeatureEvaluator checker = features.ToChecker();

var context = FeatureEvaluationContext
    .ForTargetingKey("user-123")
    .With("plan", "enterprise")
    .With("region", "eu");

if (checker.IsEnabled("checkout.new-flow", context))
{
    // Run the new checkout flow.
}

var evaluation = checker.Evaluate("checkout.new-flow", context);
Console.WriteLine($"{evaluation.Status} {evaluation.ReasonKind} {evaluation.Reason}");
```

Percentage rollout is deterministic and sticky for the same feature key and targeting key. If no targeting key is supplied, percentage rules do not match.

## Core Concepts

| Concept | Purpose |
| --- | --- |
| `FeatureDefinition` | A feature flag, toggle, experiment, permission, operational switch, migration flag, or remote-config value. |
| `FeatureEvaluationContext` | The user, tenant, session, application, device, or custom context used for targeting. |
| `FeatureCondition` | Attribute checks such as equals, not equals, contains, starts with, ends with, in, not in, exists, not exists, and minimum version. |
| `FeatureTargetingRule` | Ordered rule with conditions, include/exclude segments, percentage rollout, status, variant, and value. |
| `FeatureSegment` | Reusable audience definition with included keys, excluded keys, and segment rules. |
| `FeatureVariant` | Weighted variant with status and optional string value for remote configuration. |
| `FeatureEvaluation` | Decision result with status, reason, selected rule, selected variant, variation index, value, and existence flag. |
| `IFeatureEvaluator` | Main application-facing evaluation contract. |
| `IFeatureCheckerFactory` | Creates fresh evaluators or scoped checkers from a definition provider. |
| `IFeatureDefinitionProvider` | Storage boundary for JSON files, databases, object storage, cloud adapters, or custom providers. |

## Feature Modes

Use modes to document intent and keep flags easier to operate:

- `Release` - gradual feature delivery.
- `Experiment` - A/B tests and variant selection.
- `Operational` - kill switches and runtime controls.
- `Permission` - plan, role, group, or tenant gates.
- `Migration` - staged infrastructure or data migrations.
- `Maintenance` - temporary maintenance controls.

## Targeting

Rules are evaluated in order after dependencies and individual targets. The first matching rule wins.

```csharp
using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Evaluation;
using ManagedCode.FeatureChecker.Storage;
using ManagedCode.FeatureChecker.Targeting;

var snapshot = FeatureSnapshot.FromDefinitions(
[
    new FeatureDefinition
    {
        Key = "reports.experimental",
        Status = FeatureStatus.Disabled,
        Rules =
        [
            new FeatureTargetingRule
            {
                Status = FeatureStatus.Enabled,
                Percentage = 10,
                Conditions =
                [
                    FeatureCondition.Equals("plan", "enterprise"),
                    FeatureCondition.In("region", "eu", "us"),
                    FeatureCondition.VersionAtLeast("applicationVersion", "2.3.0")
                ]
            }
        ]
    }
]);

var checker = new FeatureChecker(snapshot);
var context = FeatureEvaluationContext
    .ForTargetingKey("user-123")
    .With("plan", "enterprise")
    .With("region", "eu")
    .With("applicationVersion", "2.4.1");

var enabled = checker.IsEnabled("reports.experimental", context);
```

## Users, Tenants, Sessions, And Scoped Access

Use `FeatureCheckerFactory` when controllers, endpoints, background jobs, or services need a context-bound checker for a request.

```csharp
using ManagedCode.FeatureChecker.Access;
using ManagedCode.FeatureChecker.Storage;

var factory = new FeatureCheckerFactory(new FeatureFileProvider("features.json"));

var userFeatures = factory.ForUser(
    userId: "user-123",
    context => context
        .WithTenantId("tenant-456")
        .WithSessionId("session-789")
        .WithPlan("enterprise")
        .WithRole("admin")
        .WithRegion("eu"));

if (userFeatures.IsEnabled("checkout.new-flow"))
{
    // Run the user-scoped feature.
}

var template = userFeatures.GetStringValue("checkout.template", "default");
var maxItems = userFeatures.GetInt32Value("checkout.max-items", 10);
```

For application code, depend on interfaces:

```csharp
using ManagedCode.FeatureChecker.Access;

public sealed class CheckoutController(IFeatureCheckerFactory featureCheckers)
{
    public bool CanUseNewCheckout(string userId, string tenantId)
    {
        var features = featureCheckers.ForUser(userId, context => context.WithTenantId(tenantId));

        return features.IsEnabled("checkout.new-flow");
    }
}
```

## Segments

Segments define reusable audiences. They can include or exclude targeting keys directly and can also contain attribute rules.

```csharp
using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Targeting;

var builder = new FeatureSetBuilder();

builder.Segment("beta-enterprise-users")
    .Include("user-123")
    .WhenAll([FeatureCondition.Equals("plan", "enterprise")]);

builder.Feature("checkout.new-flow")
    .Mode(FeatureMode.Release)
    .Disabled()
    .WhenSegment("beta-enterprise-users", variant: "treatment", featureValue: "new-checkout")
    .FallthroughVariation("control", "classic-checkout")
    .Variant("control", 50, FeatureStatus.Disabled, "classic-checkout")
    .Variant("treatment", 50, FeatureStatus.Enabled, "new-checkout");
```

## Individual Targets And Dependencies

Individual targets are evaluated before general rules. Dependencies let one feature require another feature status and, optionally, a selected variant.

```csharp
var builder = new FeatureSetBuilder();

builder.Feature("billing.enabled").Enabled();

builder.Feature("checkout.new-flow")
    .Enabled()
    .Target("blocked-user", FeatureStatus.Disabled, "control", "classic-checkout")
    .Require("billing.enabled");
```

If a dependency is missing, disabled, cyclic, or does not match the required variant, the dependent feature evaluates as disabled with reason `Dependency` or `DependencyCycle`.

## Variants And Remote Config Values

Variants support deterministic weighted selection and can carry string values for remote configuration.

```csharp
using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Evaluation;
using ManagedCode.FeatureChecker.Targeting;

var builder = new FeatureSetBuilder();

builder.Feature("pricing.card")
    .Mode(FeatureMode.Experiment)
    .Disabled("control-config")
    .Variant("control", 50, FeatureStatus.Disabled, "control-config")
    .Variant("treatment", 50, FeatureStatus.Enabled, "treatment-config");

var checker = builder.ToChecker();
var context = FeatureEvaluationContext.ForTargetingKey("user-123");

FeatureVariationDetail<string> detail = checker.StringVariationDetail(
    "pricing.card",
    "default-config",
    context);

Console.WriteLine($"{detail.Value} {detail.ReasonKind} {detail.VariationIndex}");
```

Typed helpers are available for strings, booleans, integers, long integers, and doubles.

## Microsoft.Extensions Integration

Register FeatureChecker in any `Microsoft.Extensions` host:

```csharp
using ManagedCode.FeatureChecker.DependencyInjection;

builder.Services.AddFeatureChecker(builder.Configuration.GetSection("FeatureChecker"));
```

Inject the contracts you need:

```csharp
using ManagedCode.FeatureChecker.Access;
using ManagedCode.FeatureChecker.Evaluation;
using ManagedCode.FeatureChecker.Storage;

public sealed class MyService(
    IFeatureEvaluator evaluator,
    IFeatureCheckerFactory factory,
    IFeatureDefinitionProvider provider)
{
}
```

Configuration shape:

```json
{
  "FeatureChecker": {
    "snapshot": {
      "features": [
        {
          "key": "checkout.new-flow",
          "status": "Disabled",
          "rules": [
            {
              "status": "Enabled",
              "percentage": 25,
              "conditions": [
                { "attribute": "plan", "operator": "Equals", "values": [ "enterprise" ] }
              ]
            }
          ]
        }
      ],
      "segments": []
    }
  }
}
```

You can also configure definitions in code:

```csharp
builder.Services.AddFeatureChecker(options =>
{
    var features = new FeatureSetBuilder();

    features.Feature("ops.kill-switch").Enabled();

    options.Snapshot = features.Build();
});
```

## JSON Snapshots And Storage

The core package deliberately avoids cloud SDK dependencies. Storage integration lives behind `FeatureSnapshot`, `FeatureSnapshotSerializer`, and `IFeatureDefinitionProvider`.

```csharp
using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Evaluation;
using ManagedCode.FeatureChecker.Storage;

var builder = new FeatureSetBuilder();
builder.Feature("checkout.new-flow").Enabled();

var snapshot = builder.Build();

FeatureSnapshotSerializer.Save("features.json", snapshot);

var loaded = FeatureSnapshotSerializer.Load("features.json");
var checker = new FeatureChecker(loaded);
```

Reload from a JSON file:

```csharp
var checker = new FeatureChecker(new FeatureFileProvider("features.json"));
```

Implement your own provider for ManagedCode.Storage, object storage, databases, edge config, or other backends:

```csharp
using ManagedCode.FeatureChecker.Definitions;
using ManagedCode.FeatureChecker.Storage;

public sealed class MyFeatureProvider : IFeatureDefinitionProvider
{
    public IReadOnlyCollection<FeatureDefinition> GetFeatureDefinitions()
    {
        return FeatureSnapshotSerializer.Load("features.json").Features;
    }
}
```

## Evaluation Reasons

Every evaluation returns a reason so callers can inspect and log decisions:

- `Missing`
- `Off`
- `TargetMatch`
- `RuleMatch`
- `Fallthrough`
- `Variant`
- `Dependency`
- `DependencyCycle`
- `Default`
- `Error`

Use `FeatureEvaluation.ReasonKind`, `Reason`, `Rule`, `Variant`, `VariationIndex`, `Value`, and `Exists` for diagnostics, audit logging, and tests.

## Project Structure

The repository uses vertical slices:

- `ManagedCode.FeatureChecker/Access`
- `ManagedCode.FeatureChecker/Definitions`
- `ManagedCode.FeatureChecker/DependencyInjection`
- `ManagedCode.FeatureChecker/Evaluation`
- `ManagedCode.FeatureChecker/Segments`
- `ManagedCode.FeatureChecker/Storage`
- `ManagedCode.FeatureChecker/Targeting`
- `ManagedCode.FeatureChecker.Tests/*`

The solution file is `ManagedCode.FeatureChecker.slnx`.

## Development

```bash
dotnet restore ManagedCode.FeatureChecker.slnx
dotnet format ManagedCode.FeatureChecker.slnx --verify-no-changes
dotnet build ManagedCode.FeatureChecker.slnx --configuration Release --no-restore
dotnet build ManagedCode.FeatureChecker.slnx --configuration Release --no-restore -p:RunAnalyzers=true
dotnet test --solution ManagedCode.FeatureChecker.slnx --configuration Release --no-restore --verbosity normal
dotnet test --solution ManagedCode.FeatureChecker.slnx --configuration Release --no-build --verbosity normal -- --coverage --coverage-output-format cobertura
dotnet pack ManagedCode.FeatureChecker/ManagedCode.FeatureChecker.csproj --configuration Release --no-build --output ./artifacts
```

## Release Flow

CI runs restore, format, build, analyzer, test, and coverage gates. The release workflow runs on `v*` tags or manual dispatch, validates the version from `Directory.Build.props`, packs the NuGet package, publishes to NuGet, uploads package artifacts, and creates a GitHub Release.

## Package Metadata

- Package ID: `ManagedCode.FeatureChecker`
- Target framework: `net10.0`
- License: MIT
- Repository: [github.com/managedcode/FeatureChecker](https://github.com/managedcode/FeatureChecker)

## Design Principles

- Core evaluation is deterministic and side-effect-free.
- Public APIs prefer explicit caller-provided defaults over hidden fallback behavior.
- The core package stays provider-agnostic; storage and cloud integrations should sit behind provider interfaces.
- Tests exercise caller-visible behavior through public contracts.

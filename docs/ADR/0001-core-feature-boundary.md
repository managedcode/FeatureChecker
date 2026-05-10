# ADR 0001: Core Feature Boundary

## Status

Accepted

## Context

The library should become a strong .NET feature flag SDK with deterministic local evaluation, targeting rules, dependencies, individual targets, segments, context kinds, variations, percentage rollouts, evaluation reasons, experiments, migration flags, scheduled changes, approvals, audit logs, dashboards, and integrations.

Only part of that surface belongs in a dependency-light .NET library. Hosted product capabilities such as dashboards, approvals, scheduled UI changes, audit logs, metric dashboards, and automatic rollback require persistence, identity, event ingestion, and control-plane workflows. Placing those concerns in the core package would make the package heavy and hard to embed.

## Decision

The core package owns deterministic SDK evaluation capabilities:

- feature definitions and snapshots
- prerequisites/dependencies
- individual context targets
- segment/group matching
- targeting rules and version rules
- fallthrough and off variation behavior
- deterministic weighted rollout
- typed variation and detail APIs
- .NET dependency injection and configuration registration

The core package exposes seams for platform capabilities but does not implement hosted product workflows:

- event export and experimentation metrics
- OpenTelemetry hooks
- ASP.NET request/claims mapping
- ManagedCode.Storage providers
- external feature-configuration import/export
- approval workflows, dashboards, scheduled changes, and audit logs

Those capabilities should be implemented in adapter packages or a management application on top of the core model.

## Consequences

- Applications can use the package directly with `IServiceCollection`, `IConfiguration`, JSON snapshots, and public evaluator interfaces.
- Feature decisions remain local, deterministic, and easy to test.
- The package gains small Microsoft.Extensions dependencies for .NET host integration.
- Hosted-control-plane features remain possible without forcing every consumer to carry those dependencies.

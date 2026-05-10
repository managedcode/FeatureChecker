# ADR 0002: Backend Snapshot Source Boundary

## Status

Accepted

## Context

Applications need feature configuration that can be managed outside the application binary. A backend, admin service, table, object store, API, or GitOps process can assemble a `FeatureSnapshot`; application services then need to read that snapshot and evaluate features without hardcoded definitions.

The core package must remain storage-neutral. Adding SQL, HTTP, Azure, Redis, or other provider dependencies to the core library would make all consumers carry dependencies they may not use.

## Decision

The core package exposes `IFeatureSnapshotSource` as the full-snapshot provider boundary.

- Backend adapters return a complete `FeatureSnapshot`.
- `FeatureSnapshotSourceProvider` adapts a snapshot source to `IFeatureDefinitionProvider` and `IFeatureSegmentProvider`.
- `FeatureChecker` reads features and segments from the same snapshot when a provider supports `IFeatureSnapshotSource`.
- Microsoft.Extensions registration supports custom snapshot sources through `AddFeatureCheckerSnapshotSource<TSource>()`.
- Concrete storage refresh strategies such as polling, pub/sub, SQL, Redis, object storage, HTTP, or Azure App Configuration remain adapter responsibilities.

## Consequences

- Feature configuration can be updated by a backend and consumed by application services without redeploying hardcoded feature definitions.
- Runtime freshness is controlled by the application/provider: create fresh evaluators or scopes from `IFeatureCheckerFactory`, and implement caching or refresh inside the snapshot source when direct backend reads are too expensive.
- The core package keeps deterministic local evaluation and avoids provider-specific runtime dependencies.

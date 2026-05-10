# ManagedCode.FeatureChecker Agent Guide

Project: ManagedCode.FeatureChecker
Parent: `../AGENTS.md`

## Purpose

- Provides the public feature evaluation library, including provider-backed definitions, context-aware evaluation, SDK-side feature management capabilities, and application-facing access helpers.
- Owns the packageable API surface published as `ManagedCode.FeatureChecker`.

## Entry Points

- `Access/` - factories and context-bound scopes for application/controller code.
- `Definitions/` - feature definition model and fluent builders.
- `Evaluation/` - context-aware evaluator contract, default evaluator, engine, result model, reasons, and typed value helpers.
- `Targeting/` - contexts, attributes, individual targets, rules, conditions, and rollout logic.
- `Segments/` - reusable segment model, builder, provider contract, and matcher.
- `Storage/` - snapshots, JSON/file providers, options-backed provider, and provider boundary.
- `DependencyInjection/` - Microsoft.Extensions registration.
- `ManagedCode.FeatureChecker.csproj` - NuGet package metadata and packability.

## Boundaries

- In scope: public feature-checking contracts, provider-agnostic evaluation, default implementation, package metadata, and README examples for the library API.
- Out of scope: test-only fixtures, CI workflow implementation, external service integrations, ASP.NET-specific helpers, and provider-specific storage packages.
- High risk: public API changes, value semantics, targeting semantics, package identity, and behaviour that existing consumers may depend on.

## Project Commands

- `build`: `dotnet build ManagedCode.FeatureChecker/ManagedCode.FeatureChecker.csproj --configuration Release`
- `test`: `dotnet test --project ManagedCode.FeatureChecker.Tests/ManagedCode.FeatureChecker.Tests.csproj --configuration Release --verbosity normal`
- `format`: `dotnet format ManagedCode.FeatureChecker.slnx --verify-no-changes`
- `analyze`: `dotnet build ManagedCode.FeatureChecker/ManagedCode.FeatureChecker.csproj --configuration Release -p:RunAnalyzers=true`

.NET specifics:

- Target framework: `net10.0` from `../Directory.Build.props`.
- Language version: `14` from `../Directory.Build.props`.
- Analyzer severity owner: repo-root `.editorconfig`.
- Tests: TUnit tests in `../ManagedCode.FeatureChecker.Tests/`.
- Runner model: `Microsoft.Testing.Platform`.

## Applicable Skills

- `mcaf-solid-maintainability`
- `mcaf-testing`
- `mcaf-feature-spec`
- `mcaf-documentation`
- `dotnet`
- `modern-csharp`
- `code-analysis`
- `analyzer-config`
- `format`

## Local Constraints

- Keep the core package dependency-light. Add runtime dependencies only with a documented API and packaging reason.
- Public API changes require matching tests and README updates.
- Preserve deterministic, side-effect-free feature evaluation unless an ADR documents a new provider model.
- Local maintainability limits match the root defaults.

## Local Rules

- Keep namespaces aligned with vertical-slice folders under `ManagedCode.FeatureChecker.<Slice>`.
- Prefer immutable or copy-on-construction data for public checker state.
- Do not add cloud-provider or storage-specific code to this project unless the API is explicitly provider-agnostic.
- Keep examples aligned with the real public API.

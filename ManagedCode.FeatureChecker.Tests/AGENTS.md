# ManagedCode.FeatureChecker.Tests Agent Guide

Project: ManagedCode.FeatureChecker.Tests
Parent: `../AGENTS.md`

## Purpose

- Verifies the public behaviour of `ManagedCode.FeatureChecker`.
- Provides regression coverage for public API semantics and package-facing examples.

## Entry Points

- `Access/FeatureAccessTests.cs` - factory, builder-interface, scope, and file-backed freshness tests.
- `DependencyInjection/FeatureCheckerDependencyInjectionTests.cs` - Microsoft.Extensions configuration and service registration tests.
- `Evaluation/FeatureCheckerEvaluationTests.cs` - evaluator, dependencies, variants, typed values, and evaluation collection tests.
- `Integration/FeatureCheckerIntegrationTests.cs` - end-to-end public API tests across storage, dependency injection, factories, scopes, and evaluation.
- `Segments/FeatureSegmentTests.cs` - list and rule-based segment tests.
- `Storage/FeatureStorageTests.cs` - snapshot serialization and file-backed provider tests.
- `Targeting/FeatureConditionOperatorTests.cs` - positive, negative, missing-attribute, and version condition operator tests.
- `Targeting/FeatureEvaluationContextTests.cs` - context and context-builder attribute conversion tests.
- `Targeting/FeatureTargetingTests.cs` - context rules, individual targets, and version rule tests.
- `Targeting/FeatureRolloutAndVariationTests.cs` - rollout, off-variation, and fallthrough-variation tests.
- `GlobalUsings.cs` - test-only namespace imports and aliases.
- `ManagedCode.FeatureChecker.Tests.csproj` - test dependencies, project reference, and coverage collector.

## Boundaries

- In scope: TUnit tests, test fixtures, coverage configuration, and behaviour-focused regression cases.
- Out of scope: production API implementation, package metadata, and GitHub Actions workflow authoring.
- High risk: tests that only mirror implementation details, fragile ordering assumptions, or missing negative cases for public behaviour.

## Project Commands

- `build`: `dotnet build ManagedCode.FeatureChecker.Tests/ManagedCode.FeatureChecker.Tests.csproj --configuration Release`
- `test`: `dotnet test --project ManagedCode.FeatureChecker.Tests/ManagedCode.FeatureChecker.Tests.csproj --configuration Release --verbosity normal`
- `coverage`: `dotnet test --project ManagedCode.FeatureChecker.Tests/ManagedCode.FeatureChecker.Tests.csproj --configuration Release -- --coverage --coverage-output-format cobertura`
- `format`: `dotnet format ManagedCode.FeatureChecker.slnx --verify-no-changes`
- `analyze`: `dotnet build ManagedCode.FeatureChecker.Tests/ManagedCode.FeatureChecker.Tests.csproj --configuration Release -p:RunAnalyzers=true`

.NET specifics:

- Test framework: TUnit.
- Runner model: `Microsoft.Testing.Platform`.
- Coverage driver: `Microsoft.Testing.Platform` `--coverage` extension syntax. The project references `coverlet.collector`, but repo commands must not use VSTest `--collect` syntax while the runner model is `Microsoft.Testing.Platform`.
- Analyzer severity owner: repo-root `.editorconfig`.

## Applicable Skills

- `mcaf-testing`
- `mcaf-solid-maintainability`
- `dotnet-test-frameworks`
- `run-tests`
- `tunit`
- `coverlet`
- `coverage-analysis`
- `test-anti-patterns`

## Local Constraints

- Tests should exercise caller-visible behaviour through public APIs.
- Add regression tests before fixing behaviour defects.
- Keep fixtures small and explicit.
- Local maintainability limits match the root defaults.

## Local Rules

- Keep test namespaces aligned with vertical-slice folders under `ManagedCode.FeatureChecker.Tests.<Slice>`.
- Keep TUnit test methods as instance methods. The root `.editorconfig` disables Meziantou `MA0038` for this project because it conflicts with TUnit `TUnit0048`.
- Prefer meaningful assertions over count-only tests.
- Cover unknown features, all feature states, status filtering, targeting rules, scope helpers, and typed default-value semantics when feature semantics change.
- Do not weaken assertions to fit an implementation change.
- Integration tests must use TUnit `[Category("Integration")]` and remain runnable through the repo's integration filter in CI.

# ManagedCode.FeatureChecker.Tests Agent Guide

Project: ManagedCode.FeatureChecker.Tests
Parent: `../AGENTS.md`

## Purpose

- Verifies the public behaviour of `ManagedCode.FeatureChecker`.
- Provides regression coverage for public API semantics and package-facing examples.

## Entry Points

- `Tests.cs` - TUnit tests for feature status lookup and filtering.
- `MyEnum.cs` - test enum fixture.
- `ManagedCode.FeatureChecker.Tests.csproj` - test dependencies, project reference, and coverage collector.

## Boundaries

- In scope: TUnit tests, test fixtures, coverage configuration, and behaviour-focused regression cases.
- Out of scope: production API implementation, package metadata, and GitHub Actions workflow authoring.
- High risk: tests that only mirror implementation details, fragile ordering assumptions, or missing negative cases for public behaviour.

## Project Commands

- `build`: `dotnet build ManagedCode.FeatureChecker.Tests/ManagedCode.FeatureChecker.Tests.csproj --configuration Release`
- `test`: `dotnet test ManagedCode.FeatureChecker.Tests/ManagedCode.FeatureChecker.Tests.csproj --configuration Release --verbosity normal`
- `coverage`: `dotnet test ManagedCode.FeatureChecker.Tests/ManagedCode.FeatureChecker.Tests.csproj --configuration Release -- --coverage --coverage-output-format cobertura`
- `format`: `dotnet format ManagedCode.FeatureChecker.sln --verify-no-changes`
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

- Prefer meaningful assertions over count-only tests.
- Cover unknown features, all feature states, and status filtering when feature semantics change.
- Do not weaken assertions to fit an implementation change.

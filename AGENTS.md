# FeatureChecker Agent Guide

Project: ManagedCode.FeatureChecker
Stack: .NET 10 class library, TUnit tests, GitHub Actions, NuGet package publishing

Follows [MCAF](https://mcaf.managed-code.com/).

## Purpose

This file defines how AI agents work in this solution.

- Root `AGENTS.md` holds global workflow, shared commands, cross-cutting rules, and skill routing.
- Local project rules live in `ManagedCode.FeatureChecker/AGENTS.md` and `ManagedCode.FeatureChecker.Tests/AGENTS.md`.
- Local files refine or tighten root rules. They must not silently weaken this file.

## Solution Topology

- Solution root: `.`
- Solution file: `ManagedCode.FeatureChecker.sln`
- Production project: `ManagedCode.FeatureChecker/`
- Test project: `ManagedCode.FeatureChecker.Tests/`
- Architecture map: `docs/Architecture.md`
- Agent skills: `.codex/skills/`

## Rule Precedence

1. Read this solution-root `AGENTS.md` first.
2. Read the nearest local `AGENTS.md` for the files being edited.
3. Apply the stricter rule when root and local files overlap.
4. If a local rule appears weaker than root policy, stop and clarify or document an explicit exception.
5. Document justified exceptions in the nearest local `AGENTS.md`, `docs/ADR/`, or `docs/Features/`.

## Conversations

Record durable user preferences and repeated corrections here instead of relying on chat history. Do not record one-off task scope.

Update this file when the user gives a permanent requirement, repeated correction, workflow change, or strong preference.

## Global Skills

MCAF skills are installed from the canonical MCAF tutorial and use current `mcaf-*` folders only:

- `mcaf-solution-governance` - root and project-local `AGENTS.md`, precedence, topology, and maintainability policy.
- `mcaf-architecture-overview` - `docs/Architecture.md` and navigational system maps.
- `mcaf-solid-maintainability` - SOLID, SRP, file/type/function limits, and exception handling policy.
- `mcaf-testing` - automated testing rules and verification planning.
- `mcaf-ci-cd` - GitHub Actions, NuGet publishing, release flow, and pipeline safety.
- `mcaf-source-control` - branch, commit, review, and release hygiene.
- `mcaf-documentation` - durable docs structure and source-of-truth placement.
- `mcaf-feature-spec` - non-trivial feature behaviour specs under `docs/Features/`.
- `mcaf-adr-writing` - architecture decisions under `docs/ADR/`.
- `mcaf-code-review` - review readiness and PR hygiene.
- `mcaf-security-baseline` - secrets, secure defaults, and security-impact checkpoints.
- `mcaf-devex` - local setup, inner loop, onboarding, and reproducible commands.

.NET skills are installed from [Managed Code Skills](https://skills.managed-code.com/) into `.codex/skills/`:

- `dotnet`, `project-setup`, `modern-csharp`, `microsoft-extensions`
- `format`, `analyzer-config`, `code-analysis`, `quality-ci`
- `complexity`, `crap-score`, `roslynator`, `meziantou-analyzer`, `stylecop-analyzers`, `csharpier`
- `run-tests`, `dotnet-test-frameworks`, `tunit`, `coverlet`, `coverage-analysis`, `reportgenerator`, `test-anti-patterns`
- `nuget-trusted-publishing`

Skill management rules:

- Keep MCAF framework skills current and prefixed with `mcaf-`.
- Fetch .NET-specific skills from `https://skills.managed-code.com/`.
- Add new tool-specific skills only when the repo actually uses that tool in code, tests, CI, or release work.
- After skill updates, recheck the build, test, format, analyze, coverage, and release commands below.

## Commands

- `restore`: `dotnet restore ManagedCode.FeatureChecker.sln`
- `build`: `dotnet build ManagedCode.FeatureChecker.sln --configuration Release --no-restore`
- `test`: `dotnet test --solution ManagedCode.FeatureChecker.sln --configuration Release --verbosity normal`
- `format`: `dotnet format ManagedCode.FeatureChecker.sln --verify-no-changes`
- `analyze`: `dotnet build ManagedCode.FeatureChecker.sln --configuration Release -p:RunAnalyzers=true`
- `coverage`: `dotnet test --solution ManagedCode.FeatureChecker.sln --configuration Release -- --coverage --coverage-output-format cobertura`

.NET specifics:

- Target framework is `net10.0` from `Directory.Build.props`.
- `LangVersion` is explicitly pinned to `14` in `Directory.Build.props`.
- Test framework is TUnit.
- Runner model is `Microsoft.Testing.Platform`, configured in `global.json`.
- Coverage uses the `Microsoft.Testing.Platform` `--coverage` extension syntax. Do not use VSTest `--collect` syntax unless the runner model changes.
- Repo-root `.editorconfig` is the source of truth for formatting, naming, style, and analyzer severity.
- `Directory.Build.props`, `Directory.Packages.props`, and project files own package versions, analyzer switches, package metadata, and runner settings.

## Project AGENTS Policy

- This is a multi-project solution. Keep one root `AGENTS.md` and one local `AGENTS.md` in each project root.
- Each local file must document purpose, entry points, boundaries, local commands, applicable skills, and local risks.
- When adding a new project, add its local `AGENTS.md` in the same change.

## Maintainability Limits

- `file_max_loc`: `400`
- `type_max_loc`: `200`
- `function_max_loc`: `50`
- `max_nesting_depth`: `3`
- `exception_policy`: `Document any justified exception in the nearest ADR, feature doc, or local AGENTS.md with the reason, scope, verification, and removal or refactor plan.`

Local `AGENTS.md` files may tighten these values. They must not loosen them without an explicit root-level exception.

## Task Delivery

- Start non-trivial work from `docs/Architecture.md` and the nearest local `AGENTS.md`.
- Use vertical slices for behaviour changes: keep code, tests, contracts, docs, and supporting artifacts close to the feature boundary.
- For non-trivial work, create `<slug>.brainstorm.md` before `<slug>.plan.md`; skip the brainstorm only for simple or obvious tasks.
- Put test strategy inside the plan before implementation starts.
- Run a baseline test pass after planning non-trivial work so pre-existing failures are visible.
- Implement code and tests together.
- Run verification in layers: build, focused tests, related tests, broader suite, format, analyzers, coverage, and any relevant release checks.
- Update `docs/Architecture.md`, `docs/Features/`, or `docs/ADR/` when architecture, behaviour, or durable rules change.

## Testing

- TDD is the default for new behaviour and bug fixes.
- Bug fixes start with a failing regression test when practical.
- New public behaviour needs positive, negative, and edge-case coverage.
- Prefer testing caller-visible behaviour through public contracts over implementation details.
- Flaky tests are failures. Fix the cause.
- Production code changes should preserve or improve coverage. Public contracts and critical flows require explicit success and failure assertions.
- Do not skip tests to make a branch green.

## Code and Design

- SOLID, SRP, cohesion, and composition over inheritance are default design rules.
- Keep public API changes intentional, documented, and covered by tests.
- Avoid hardcoded values and magic literals in implementation code. Prefer named constants, enums, options, or value objects.
- Remove obsolete code when replacing an implementation. Do not leave shims or fallback paths unless an explicit temporary exception documents the owner, scope, verification, and removal plan.
- Do not add runtime dependencies to the core library without a documented reason.

## Release and Source Control

- Never force-push to `main`.
- Do not commit secrets, keys, or connection strings.
- Release and NuGet changes must be validated with `mcaf-ci-cd` and `nuget-trusted-publishing`.
- Package metadata lives in `Directory.Build.props` and `ManagedCode.FeatureChecker/ManagedCode.FeatureChecker.csproj`.
- GitHub Actions workflows live in `.github/workflows/`.

## Preferences

### Likes

### Dislikes

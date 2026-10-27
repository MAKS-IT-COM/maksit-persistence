# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.1] - 2026-10-27

### Changed

- Bumped package dependencies: linq2db / linq2db.PostgreSQL 6.5.1, MaksIT.Core 1.6.14, MaksIT.Results 2.0.6, Microsoft.Extensions.* 10.0.14 (tests: coverlet.mtp, xunit.v3).
- Synced RepoUtils utils: ContainerRegistry JSON catalog (PascalCase Harbor / InCluster keys); `CleanupArtifacts` replaces `DotNetCleanupArtifacts`; GitHub `githubSecret` is `GitClone`; release `scriptSettings` declares `repoUtilsSecretsShared` / `repoUtilsSecrets`; dropped unused Npm plugins from release settings.
- NuGet publish moved to GitHub Actions Trusted Publishing (OIDC on `v*` tags); removed `DotNetNuGet` from the RepoUtils release pipeline.
- Docs: CONTRIBUTING points at RepoUtils Origin; README HAMode note is **maksit-buoy**.

## [2.0.0] - 2026-08-21

### Removed
- **Schema sync (breaking):** `RegisterSchemaSync`, `ISchemaSyncService`, `ISchemaSyncOptions`, `SchemaColumn`, desired-schema maps, `DatabaseStartupPhases.SchemaSync`, and add-only `ALTER TABLE` sync. Schema evolution is FluentMigrator only.

## [1.0.0] - 2026-08-17

### Added
- Package **MaksIT.Persistence** (repo `maksit-persistence`): product-agnostic linq2db + PostgreSQL Engine spine.
- **Connection factory:** `ILinq2DbConnectionFactory` / `RegisterLinq2Db(connectionString, mappingSchema?)` — scoped PostgreSQL `DataConnection`.
- **Migrations:** `RegisterPostgresMigrations` — wait for Postgres, optional create-DB, retry `MigrateUp`, product hooks for coordination tables and schema verify.
- **Schema sync:** `RegisterSchemaSync` — add-only `ALTER TABLE … ADD COLUMN` from a product desired-schema map.
- **Guards:** `Linq2DbGuard` maps Postgres unique violation (`23505`) to `Result.Conflict`; rethrows `OperationCanceledException`.
- **Mapping:** `NameConverter.ToSnakeCase` and fluent `MapScalarPropertiesToSnakeCase`.

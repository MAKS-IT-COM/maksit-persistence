# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

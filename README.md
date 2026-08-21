# MaksIT.Persistence

![Line Coverage](https://img.shields.io/badge/Line%20Coverage-36.8%25-yellow)
![Branch Coverage](https://img.shields.io/badge/Branch%20Coverage-41.4%25-yellowgreen)
![Method Coverage](https://img.shields.io/badge/Method%20Coverage-48.6%25-yellowgreen)

NuGet facade over [linq2db](https://linq2db.github.io/) + PostgreSQL for ASP.NET Core Engines: connection factory, FluentMigrator startup, and `MaksIT.Results` guards.

## Table of Contents

- [Overview](#overview)
- [Result conventions](#result-conventions)
- [Getting Started](#getting-started)
- [Installation](#installation)
- [Registering services](#registering-services)
- [What stays in the product](#what-stays-in-the-product)
- [Contributing](#contributing)
- [Contact](#contact)
- [License](#license)

## Overview

`MaksIT.Persistence` wraps linq2db PostgreSQL wiring so Engine code depends on `ILinq2DbConnectionFactory` instead of copying `DataOptions` / `ProviderName.PostgreSQL` / startup wait in every product. Domain tables, Dto, FluentMapping **content**, and FluentMigrator migration classes stay in the product Engine.

Linq2Db queries stay in the product: persist/query services still call `GetTable<T>()`. This library hides connection lifetime, Postgres transients, and FluentMigrator bootstrap.

| Need | Register | Inject |
|------|----------|--------|
| Scoped `DataConnection` | `RegisterLinq2Db(connectionString, mappingSchema?)` | `ILinq2DbConnectionFactory` |
| FluentMigrator wait / create-DB / retry MigrateUp | `RegisterPostgresMigrations(connectionString, assembly, options?)` | `IRunMigrationsService` |

Work leases are **not** in this package: use **MaksIT.Dapr** (`RegisterWorkLeases`) for most first-party apps, or **MaksIT.HAMode** for **maksit-nats** only.

## Result conventions

- **Success / failure:** check `result.IsSuccess`. Map to HTTP with `result.ToActionResult()`.
- **Cancellation:** `Linq2DbGuard` rethrows `OperationCanceledException` (do not wrap as a failed `Result`).
- **Unique violation (`23505`):** `Linq2DbGuard` maps to `Conflict`.
- **Missing row:** product persist/query ports return `NotFound` — not this library.

## Getting Started

Ensure that you have the following installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/get-started) (optional; used by RepoUtils Linux test validation)

## Installation

```powershell
dotnet add package MaksIT.Persistence
```

Or in your `.csproj`:

```xml
<PackageReference Include="MaksIT.Persistence" Version="2.0.0" />
```

## Registering services

Call from the product Engine `Add*Engine`, not from host JSON binding. Pass the Engine connection string; optional `MappingSchema` for fluent maps (omit to use attribute mapping).

```csharp
using MaksIT.Persistence.Extensions;

services.RegisterLinq2Db(config.ConnectionString, ProductLinq2DbMapping.Schema);
services.RegisterPostgresMigrations(
  config.ConnectionString,
  typeof(BaselineSchema).Assembly,
  new PostgresMigrationOptions {
    EnsureCoordinationTables = ct => CoordinationTableProvisioner.EnsureAsync(config.ConnectionString, ct),
    VerifySchema = ct => VerifyCoreSchemaAsync(config.ConnectionString, ct),
  });
```

Schema changes go in a new FluentMigrator `[Migration]` (`Create.Table` / `Alter.Table.AddColumn`; Drop/rename needs a separate decision). Host `Program.cs` before `RunAsync`:

```csharp
await app.Services.EnsureLinq2DbMigratedAsync();
```

Product persist/query:

```csharp
using var db = connectionFactory.Create();
var row = db.GetTable<UserDto>().FirstOrDefault(u => u.Id == id);
```

## What stays in the product

- Table names, Dto, FluentMapping entity list, FluentMigrator `[Migration]` classes
- Coordination DDL unique to the product (hook on `PostgresMigrationOptions`)
- `I*PersistenceService` / `I*QueryService` and domain services
- HA work leases (Dapr or HAMode)

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).

## Contact

For questions or support, contact:

- **Email:** [maksym.sadovnychyy@gmail.com](mailto:maksym.sadovnychyy@gmail.com)

## License

This project is licensed under the MIT License. See the [LICENSE.md](LICENSE.md) file for details.

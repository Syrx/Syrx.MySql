# Syrx.MySql

Syrx.MySql adds MySQL support to the Syrx data-access stack while preserving the same explicit-command and `ICommander<TRepository>` programming model used across the wider Syrx ecosystem.

## Overview

- Target framework: .NET 10
- Current package version: 3.0.0
- Primary dependency: MySqlConnector
- Recommended entry point: `Syrx.MySql.Extensions`

## Packages

| Package | Purpose |
|--|--|
| `Syrx.Commanders.Databases.Connectors.MySql` | Low-level MySQL database connector implementation. |
| `Syrx.Commanders.Databases.Connectors.MySql.Extensions` | Dependency injection extensions for registering the connector. |
| `Syrx.MySql` | Aggregates the core MySQL support packages. |
| `Syrx.MySql.Extensions` | Recommended package for most consumers; combines Syrx and MySQL registration helpers. |

## Installation

> [!TIP]
> Most applications should install `Syrx.MySql.Extensions` so MySQL support can be added through a single registration call.

### Recommended package

| Source | Command |
|--|--|
| .NET CLI | `dotnet add package Syrx.MySql.Extensions --version 3.0.0` |
| Package Manager | `Install-Package Syrx.MySql.Extensions -Version 3.0.0` |
| Package Reference | `<PackageReference Include="Syrx.MySql.Extensions" Version="3.0.0" />` |
| Paket CLI | `paket add Syrx.MySql.Extensions --version 3.0.0` |

### Core package only

If you only need the package graph without the higher-level registration helpers, install `Syrx.MySql`.

| Source | Command |
|--|--|
| .NET CLI | `dotnet add package Syrx.MySql --version 3.0.0` |
| Package Manager | `Install-Package Syrx.MySql -Version 3.0.0` |
| Package Reference | `<PackageReference Include="Syrx.MySql" Version="3.0.0" />` |
| Paket CLI | `paket add Syrx.MySql --version 3.0.0` |

## Usage

```csharp
using Syrx.Commanders.Databases.Connectors.MySql.Extensions;

public static IServiceCollection Install(this IServiceCollection services)
{
    return services.UseSyrx(builder =>
        builder.UseMySql(settings => settings
            .AddConnectionString("Default", "Server=localhost;Database=app;User ID=user;Password=password;")
            .AddCommand(commandNamespace =>
            {
                // Register explicit Syrx command definitions for each repository method.
            })));
}
```

## Repository structure

The repository keeps the solution split into focused packages:

- `src/Syrx.Commanders.Databases.Connectors.MySql`: connector implementation
- `src/Syrx.Commanders.Databases.Connectors.MySql.Extensions`: DI registration helpers
- `src/Syrx.MySql`: package aggregation for the core MySQL experience
- `src/Syrx.MySql.Extensions`: package aggregation for the recommended registration experience
- `tests/unit`: unit coverage for connector and extension wiring
- `tests/integration`: integration coverage against ephemeral MySQL instances

## Build and test

```powershell
dotnet build Syrx.MySql.sln --configuration Release
dotnet test Syrx.MySql.sln --configuration Release
```

Integration tests use Testcontainers-based MySQL coverage and require Docker-compatible container execution on the host machine.

For local containerized integration tests only, the fixture uses a test-scoped connection string with `SslMode=None`. This is intentionally non-production and should not be reused for deployed environments.

## Release publishing approvals

The publish workflow deploy job targets the `production` GitHub Actions environment.

- Configure required reviewers on the `production` environment so release publication is explicitly approved before execution.
- Store `NUGET_API_KEY` as an environment-scoped secret on `production` instead of as a repository-wide secret.
- Release publishing should proceed only after required environment approvals are granted.

## Documentation

- Public API XML documentation is tracked with `scripts/Get-DocumentationMetrics.ps1`.
- Project-level package notes live alongside each `src` project in a local `README.md`.
- Security and performance research reports are generated under `.docs/research`.

## Credits

Syrx builds on the command-oriented data access model provided by [Syrx](https://github.com/Syrx/Syrx) and is inspired by [Dapper](https://github.com/DapperLib/Dapper).
MySQL connectivity is provided by [MySqlConnector](https://github.com/mysql-net/MySqlConnector).

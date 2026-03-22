# Syrx.Commanders.Databases.Connectors.MySql.Extensions

This package adds `IServiceCollection` and `SyrxBuilder` extension methods for wiring the MySQL connector into an application.

## Responsibilities

- Register `MySqlDatabaseConnector` as the active `IDatabaseConnector`.
- Register Syrx command readers and database commander services.
- Expose the `UseMySql` entry point for fluent application startup configuration.

## Typical usage

Use this package when you already depend on the lower-level Syrx packages and want a focused registration helper for MySQL.

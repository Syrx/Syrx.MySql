# Syrx.Commanders.Databases.Connectors.MySql

This package contains the concrete `MySqlDatabaseConnector` used by Syrx to create `IDbConnection` instances backed by MySqlConnector.

## Responsibilities

- Resolve MySQL database connections from Syrx command settings.
- Bridge Syrx database commander infrastructure to `MySqlConnectorFactory`.
- Keep connection creation separate from dependency injection registration concerns.

## When to use this package

Install this package directly when you want the connector implementation but prefer to register services manually.

Most applications should instead use `Syrx.MySql.Extensions`.

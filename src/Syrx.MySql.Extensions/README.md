# Syrx.MySql.Extensions

This is the recommended package for applications that want Syrx plus MySQL support with fluent registration helpers.

## Responsibilities

- Aggregate `Syrx.MySql` and `Syrx.Commanders.Databases.Connectors.MySql.Extensions`.
- Give application startup a single package reference for MySQL-enabled Syrx registration.

## Typical usage

Install this package in application projects and configure Syrx by calling `UseMySql` inside your service registration pipeline.

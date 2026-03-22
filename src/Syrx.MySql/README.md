# Syrx.MySql

This package provides the core package graph required to use Syrx with MySQL.

## Responsibilities

- Aggregate the MySQL connector package with the base Syrx database commander package.
- Provide a simpler package reference for consumers who do not need the extensions bundle.

## Typical usage

Reference this package when you prefer to compose registration and container wiring yourself.

If you want the recommended out-of-the-box registration experience, use `Syrx.MySql.Extensions`.

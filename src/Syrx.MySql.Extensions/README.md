# Syrx.MySql.Extensions

Dependency injection and configuration extensions for Syrx MySQL integration.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
  - [Basic Configuration](#basic-configuration)
  - [Advanced Configuration](#advanced-configuration)
- [Configuration Methods](#configuration-methods)
  - [Connection String Management](#connection-string-management)
  - [Command Configuration](#command-configuration)
  - [Service Lifetime](#service-lifetime)
- [Command Builder Methods](#command-builder-methods)
- [MySQL-Specific Examples](#mysql-specific-examples)
  - [Working with JSON Columns](#working-with-json-columns)
  - [Stored Procedure Calls](#stored-procedure-calls)
  - [Bulk Operations](#bulk-operations)
- [Multi-Repository Configuration](#multi-repository-configuration)
- [Connection String Examples](#connection-string-examples)
  - [Local Development](#local-development)
  - [Production with SSL](#production-with-ssl)
  - [Connection Pooling](#connection-pooling)
  - [AWS RDS](#aws-rds)
- [Repository Registration](#repository-registration)
- [Environment-Specific Configuration](#environment-specific-configuration)
- [Performance Considerations](#performance-considerations)
  - [Connection Pool Settings](#connection-pool-settings)
  - [Read/Write Separation](#readwrite-separation)
- [Related Packages](#related-packages)
- [License](#license)
- [Credits](#credits)

## Overview

`Syrx.MySql.Extensions` provides easy-to-use extension methods and configuration builders for integrating Syrx with MySQL databases. This package simplifies the setup process and provides fluent APIs for configuration.

## Features

- **Fluent Configuration**: Easy-to-read configuration syntax
- **Dependency Injection**: Seamless integration with Microsoft.Extensions.DependencyInjection
- **Connection Management**: Simplified connection string management
- **Command Builders**: Type-safe command configuration
- **Service Lifetime Control**: Configurable service lifetimes

## Installation

```bash
dotnet add package Syrx.MySql.Extensions
```

**Package Manager**
```bash
Install-Package Syrx.MySql.Extensions
```

**PackageReference**
```xml
<PackageReference Include="Syrx.MySql.Extensions" Version="2.4.5" />
```

## Quick Start

### Basic Configuration

```csharp
using Syrx.MySql.Extensions;

public void ConfigureServices(IServiceCollection services)
{
    services.UseSyrx(builder => builder
        .UseMySql(mysql => mysql
            .AddConnectionString("Default", "Server=localhost;Database=MyDb;Uid=myuser;Pwd=mypass;")
            .AddCommand(types => types
                .ForType<UserRepository>(methods => methods
                    .ForMethod(nameof(UserRepository.GetAllAsync), command => command
                        .UseConnectionAlias("Default")
                        .UseCommandText("SELECT * FROM Users"))))));
}
```

### Advanced Configuration

```csharp
services.UseSyrx(builder => builder
    .UseMySql(mysql => mysql
        .AddConnectionString("Primary", "Server=localhost;Database=mydb;Uid=admin;Pwd=adminpass;")
        .AddConnectionString("ReadOnly", "Server=readonly;Database=mydb;Uid=reader;Pwd=readpass;")
        .AddCommand(types => types
            .ForType<UserRepository>(methods => methods
                .ForMethod("GetActiveUsers", command => command
                    .UseConnectionAlias("Primary")
                    .UseCommandText("SELECT * FROM Users WHERE IsActive = 1")
                    .SetCommandTimeout(30)
                    .SetCommandType(CommandType.Text))
                .ForMethod("GetUserStatistics", command => command
                    .UseConnectionAlias("ReadOnly")  
                    .UseCommandText("CALL sp_GetUserStats(@userId)")
                    .SetCommandType(CommandType.StoredProcedure)))
            .ForType<ProductRepository>(methods => methods
                .ForMethod("GetProductsWithCategories", command => command
                    .UseConnectionAlias("Primary")
                    .UseCommandText(@"
                        SELECT p.*, c.*
                        FROM Products p
                        JOIN Categories c ON p.CategoryId = c.Id
                        WHERE p.IsActive = 1")
                    .SplitOn("Id"))),
        ServiceLifetime.Scoped));
```

## Configuration Methods

### Connection String Management

```csharp
.UseMySql(mysql => mysql
    .AddConnectionString("alias", "connection-string")
    .AddConnectionString("another-alias", "another-connection-string")
)
```

### Command Configuration

```csharp
.AddCommand(types => types
    .ForType<RepositoryType>(methods => methods
        .ForMethod("MethodName", command => command
            .UseConnectionAlias("alias")
            .UseCommandText("SQL command text")
            .SetCommandTimeout(seconds)
            .SetCommandType(CommandType.Text | CommandType.StoredProcedure)
            .SplitOn("column-name-for-multimap")
            .SetIsolationLevel(IsolationLevel.ReadCommitted))))
```

### Service Lifetime

```csharp
services.UseSyrx(builder => builder
    .UseMySql(/* configuration */),
    ServiceLifetime.Scoped);  // or Singleton, Transient
```

## Command Builder Methods

| Method | Description | Example |
|--------|-------------|---------|
| `UseConnectionAlias(string)` | Specifies which connection string to use | `.UseConnectionAlias("Primary")` |
| `UseCommandText(string)` | Sets the SQL command text | `.UseCommandText("SELECT * FROM Users")` |
| `SetCommandTimeout(int)` | Sets command timeout in seconds | `.SetCommandTimeout(30)` |
| `SetCommandType(CommandType)` | Sets command type | `.SetCommandType(CommandType.StoredProcedure)` |
| `SplitOn(string)` | Sets split columns for multi-map queries | `.SplitOn("Id")` |
| `SetIsolationLevel(IsolationLevel)` | Sets transaction isolation level | `.SetIsolationLevel(IsolationLevel.ReadCommitted)` |

## MySQL-Specific Examples

### Working with JSON Columns

```csharp
.ForMethod("GetUserPreferences", command => command
    .UseConnectionAlias("Primary")
    .UseCommandText(@"
        SELECT Id, Name, 
               JSON_EXTRACT(Preferences, '$.theme') as Theme,
               JSON_EXTRACT(Preferences, '$.language') as Language
        FROM Users 
        WHERE Id = @userId"))
```

### Stored Procedure Calls

```csharp
.ForMethod("GetMonthlyReport", command => command
    .UseConnectionAlias("Reporting")
    .UseCommandText("CALL sp_GetMonthlyReport(@year, @month)")
    .SetCommandType(CommandType.StoredProcedure)
    .SetCommandTimeout(60))
```

### Bulk Operations

```csharp
.ForMethod("BulkInsertUsers", command => command
    .UseConnectionAlias("Primary")
    .UseCommandText(@"
        INSERT INTO Users (Name, Email, CreatedDate) 
        VALUES (@Name, @Email, @CreatedDate)")
    .SetCommandTimeout(120))
```

## Multi-Repository Configuration

```csharp
.AddCommand(types => types
    .ForType<UserRepository>(methods => methods
        .ForMethod("GetUsers", command => command
            .UseConnectionAlias("Primary")
            .UseCommandText("SELECT * FROM Users"))
        .ForMethod("GetUserById", command => command
            .UseConnectionAlias("Primary") 
            .UseCommandText("SELECT * FROM Users WHERE Id = @id")))
    .ForType<ProductRepository>(methods => methods
        .ForMethod("GetProducts", command => command
            .UseConnectionAlias("Catalog")
            .UseCommandText("SELECT * FROM Products"))
        .ForMethod("SearchProducts", command => command
            .UseConnectionAlias("Catalog")
            .UseCommandText("SELECT * FROM Products WHERE Name LIKE CONCAT('%', @search, '%')"))))
```

## Connection String Examples

### Local Development
```csharp
.AddConnectionString("Local", "Server=localhost;Database=myapp;Uid=dev;Pwd=devpass;")
```

### Production with SSL
```csharp
.AddConnectionString("Production", 
    "Server=prod.mysql.com;Database=myapp;Uid=produser;Pwd=prodpass;SslMode=Required;")
```

### Connection Pooling
```csharp
.AddConnectionString("Pooled", 
    "Server=localhost;Database=myapp;Uid=user;Pwd=pass;MinimumPoolSize=5;MaximumPoolSize=100;")
```

### AWS RDS
```csharp
.AddConnectionString("AWS", 
    "Server=myinstance.region.rds.amazonaws.com;Database=myapp;Uid=admin;Pwd=password;SslMode=Required;")
```

## Repository Registration

Don't forget to register your repositories:

```csharp
services.AddScoped<UserRepository>();
services.AddScoped<ProductRepository>();
```

## Environment-Specific Configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    var connectionString = _configuration.GetConnectionString("DefaultConnection");
    
    services.UseSyrx(builder => builder
        .UseMySql(mysql => mysql
            .AddConnectionString("Default", connectionString)
            .AddCommand(/* command configuration */)));
}
```

## Performance Considerations

### Connection Pool Settings
```csharp
.AddConnectionString("Optimized", 
    "Server=localhost;Database=mydb;Uid=user;Pwd=pass;" +
    "MinimumPoolSize=10;MaximumPoolSize=200;" +
    "ConnectionTimeout=30;CommandTimeout=60;")
```

### Read/Write Separation
```csharp
.UseMySql(mysql => mysql
    .AddConnectionString("Master", masterConnectionString)
    .AddConnectionString("Slave", slaveConnectionString)
    .AddCommand(types => types
        .ForType<UserRepository>(methods => methods
            .ForMethod("GetUsers", command => command
                .UseConnectionAlias("Slave"))  // Read from slave
            .ForMethod("CreateUser", command => command
                .UseConnectionAlias("Master"))))) // Write to master
```

## Related Packages

- **[Syrx.MySql](https://www.nuget.org/packages/Syrx.MySql/)**: Core MySQL provider
- **[Syrx](https://www.nuget.org/packages/Syrx/)**: Core Syrx interfaces
- **[Syrx.Commanders.Databases.Extensions](https://www.nuget.org/packages/Syrx.Commanders.Databases.Extensions/)**: Database framework extensions

## License

This project is licensed under the [MIT License](https://github.com/Syrx/Syrx/blob/main/LICENSE).

## Credits

- Built on top of [Dapper](https://github.com/DapperLib/Dapper)
- MySQL support provided by [MySqlConnector](https://github.com/mysql-net/MySqlConnector)
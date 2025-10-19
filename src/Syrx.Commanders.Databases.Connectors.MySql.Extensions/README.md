# Syrx.Commanders.Databases.Connectors.MySql.Extensions

Dependency injection extensions for Syrx MySQL database connectors.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Key Extensions](#key-extensions)
  - [ServiceCollectionExtensions](#servicecollectionextensions)
  - [MySqlConnectorExtensions](#mysqlconnectorextensions)
- [Usage](#usage)
  - [Basic Registration](#basic-registration)
  - [Custom Lifetime](#custom-lifetime)
  - [Advanced Configuration](#advanced-configuration)
- [Service Registration Details](#service-registration-details)
- [Service Lifetimes](#service-lifetimes)
  - [Lifetime Recommendations](#lifetime-recommendations)
- [Registration Process](#registration-process)
- [Integration with Other Extensions](#integration-with-other-extensions)
- [MySQL-Specific Configuration](#mysql-specific-configuration)
  - [Connection Pool Management](#connection-pool-management)
  - [Primary/Replica Configuration](#primaryreplica-configuration)
  - [SSL Configuration](#ssl-configuration)
- [Error Handling](#error-handling)
- [Testing Support](#testing-support)
- [Performance Optimizations](#performance-optimizations)
  - [Connection String Optimization](#connection-string-optimization)
  - [Bulk Operation Configuration](#bulk-operation-configuration)
- [Related Packages](#related-packages)
- [License](#license)
- [Credits](#credits)

## Overview

`Syrx.Commanders.Databases.Connectors.MySql.Extensions` provides dependency injection and service registration extensions specifically for MySQL database connectors in the Syrx framework. This package enables easy registration of MySQL connectors with DI containers.

## Features

- **Service Registration**: Automatic registration of MySQL connector services
- **Lifecycle Management**: Configurable service lifetimes for connectors
- **DI Integration**: Seamless integration with Microsoft.Extensions.DependencyInjection
- **Builder Pattern**: Fluent configuration APIs
- **Extensibility**: Support for custom connector configurations

## Installation

> **Note**: This package is typically installed automatically as a dependency of `Syrx.MySql.Extensions`.

```bash
dotnet add package Syrx.Commanders.Databases.Connectors.MySql.Extensions
```

**Package Manager**
```bash
Install-Package Syrx.Commanders.Databases.Connectors.MySql.Extensions
```

**PackageReference**
```xml
<PackageReference Include="Syrx.Commanders.Databases.Connectors.MySql.Extensions" Version="3.0.0" />
```

## Key Extensions

### ServiceCollectionExtensions

Provides extension methods for `IServiceCollection`:

```csharp
public static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddMySql(
        this IServiceCollection services, 
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return services.TryAddToServiceCollection(
            typeof(IDatabaseConnector),
            typeof(MySqlDatabaseConnector),
            lifetime);
    }
}
```

### MySqlConnectorExtensions

Provides builder pattern extensions:

```csharp
public static class MySqlConnectorExtensions
{
    public static SyrxBuilder UseMySql(
        this SyrxBuilder builder,
        Action<CommanderSettingsBuilder> factory,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        // Extension implementation
    }
}
```

## Usage

### Basic Registration

```csharp
using Syrx.Commanders.Databases.Connectors.MySql.Extensions;

public void ConfigureServices(IServiceCollection services)
{
    services.UseSyrx(builder => builder
        .UseMySql(mysql => mysql
            .AddConnectionString("Default", connectionString)
            .AddCommand(/* command configuration */)));
}
```

### Custom Lifetime

```csharp
services.UseSyrx(builder => builder
    .UseMySql(
        mysql => mysql.AddConnectionString(/* config */),
        ServiceLifetime.Scoped));
```

### Advanced Configuration

```csharp
services.UseSyrx(builder => builder
    .UseMySql(mysql => mysql
        .AddConnectionString("Primary", "Server=localhost;Database=mydb;Uid=admin;Pwd=adminpass;")
        .AddConnectionString("ReadOnly", "Server=readonly;Database=mydb;Uid=reader;Pwd=readpass;")
        .AddCommand(types => types
            .ForType<UserRepository>(methods => methods
                .ForMethod("GetUsers", command => command
                    .UseConnectionAlias("ReadOnly")
                    .UseCommandText("SELECT * FROM Users")))),
        ServiceLifetime.Singleton));
```

## Service Registration Details

The extensions automatically register:

1. **ICommanderSettings**: The configuration settings instance
2. **IDatabaseCommandReader**: For reading command configurations  
3. **IDatabaseConnector**: The MySQL-specific connector
4. **DatabaseCommander<T>**: The generic database commander

## Service Lifetimes

| Lifetime | Use Case | Description |
|----------|----------|-------------|
| `Transient` | Default | New instance per injection |
| `Scoped` | Web Apps | Instance per request/scope |
| `Singleton` | Performance | Single instance for application |

### Lifetime Recommendations

- **Transient**: Default for most scenarios, minimal overhead
- **Scoped**: Web applications where you want request-scoped connections
- **Singleton**: High-performance scenarios with careful connection management

## Registration Process

When calling `.UseMySql()`, the following happens:

1. **Settings Registration**: CommanderSettings configured as transient
2. **Reader Registration**: DatabaseCommandReader registered
3. **Connector Registration**: MySqlDatabaseConnector registered
4. **Commander Registration**: DatabaseCommander<T> registered

## Integration with Other Extensions

Works seamlessly with other Syrx extension packages:

```csharp
services.UseSyrx(builder => builder
    .UseMySql(/* MySQL config */)
    .UseSqlServer(/* SQL Server config */)    // If needed
    .UseNpgsql(/* PostgreSQL config */));     // If needed
```

## MySQL-Specific Configuration

### Connection Pool Management
```csharp
services.UseSyrx(builder => builder
    .UseMySql(mysql => mysql
        .AddConnectionString("Pooled", 
            "Server=localhost;Database=mydb;Uid=user;Pwd=pass;" +
            "MinimumPoolSize=10;MaximumPoolSize=200;ConnectionTimeout=30;")
        .AddCommand(/* commands */)));
```

### Primary/Replica Configuration
```csharp
services.UseSyrx(builder => builder
    .UseMySql(mysql => mysql
        .AddConnectionString("Primary", primaryConnectionString)
        .AddConnectionString("Replica", replicaConnectionString)
        .AddCommand(types => types
            .ForType<UserRepository>(methods => methods
                .ForMethod("GetUsers", command => command
                    .UseConnectionAlias("Replica"))    // Read operations
                .ForMethod("CreateUser", command => command
                    .UseConnectionAlias("Primary"))))); // Write operations
```

### SSL Configuration
```csharp
services.UseSyrx(builder => builder
    .UseMySql(mysql => mysql
        .AddConnectionString("Secure", 
            "Server=prod.mysql.com;Database=mydb;Uid=user;Pwd=pass;" +
            "SslMode=Required;SslCert=client.pem;SslKey=key.pem;SslCa=ca.pem;")
        .AddCommand(/* commands */)));
```

## Error Handling

The extensions provide proper error handling for:
- Invalid configuration scenarios
- Missing dependencies
- Circular dependency issues
- Service registration conflicts
- MySQL-specific connection errors

## Testing Support

The extensions support testing scenarios:

```csharp
// Test service collection
var services = new ServiceCollection();
services.UseSyrx(builder => builder
    .UseMySql(mysql => mysql
        .AddConnectionString("Test", testConnectionString)
        .AddCommand(/* test commands */)));

var provider = services.BuildServiceProvider();
var connector = provider.GetService<IDatabaseConnector>();
```

## Performance Optimizations

### Connection String Optimization
```csharp
services.UseSyrx(builder => builder
    .UseMySql(mysql => mysql
        .AddConnectionString("Optimized", 
            "Server=localhost;Database=mydb;Uid=user;Pwd=pass;" +
            "MinimumPoolSize=5;MaximumPoolSize=100;" +
            "ConnectionTimeout=30;CommandTimeout=60;" +
            "ConnectionLifeTime=300;ConnectionReset=true;")
        .AddCommand(/* commands */)));
```

### Bulk Operation Configuration
```csharp
.ForMethod("BulkInsert", command => command
    .UseConnectionAlias("BulkWrite")
    .UseCommandText("INSERT INTO Users (Name, Email) VALUES (@Name, @Email)")
    .SetCommandTimeout(300))  // Longer timeout for bulk operations
```

## Related Packages

- **[Syrx.MySql.Extensions](https://www.nuget.org/packages/Syrx.MySql.Extensions/)**: High-level MySQL extensions
- **[Syrx.Commanders.Databases.Connectors.MySql](https://www.nuget.org/packages/Syrx.Commanders.Databases.Connectors.MySql/)**: Core MySQL connector
- **[Syrx.Commanders.Databases.Extensions](https://www.nuget.org/packages/Syrx.Commanders.Databases.Extensions/)**: Base database extensions

## License

This project is licensed under the [MIT License](https://github.com/Syrx/Syrx/blob/main/LICENSE).

## Credits

- Built on top of [Microsoft.Extensions.DependencyInjection](https://github.com/dotnet/extensions)
- MySQL support provided by [MySqlConnector](https://github.com/mysql-net/MySqlConnector)
- Follows [Dapper](https://github.com/DapperLib/Dapper) performance patterns

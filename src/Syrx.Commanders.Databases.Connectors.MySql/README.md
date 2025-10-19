# Syrx.Commanders.Databases.Connectors.MySql

MySQL database connector implementation for the Syrx data access framework.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Architecture](#architecture)
- [Key Components](#key-components)
  - [MySqlDatabaseConnector](#mysqldatabaseconnector)
  - [Connection Creation Process](#connection-creation-process)
- [Usage](#usage)
- [Connection String Support](#connection-string-support)
  - [Basic Authentication](#basic-authentication)
  - [With Port Specification](#with-port-specification)
  - [SSL/TLS Connections](#ssltls-connections)
  - [Connection Pooling](#connection-pooling)
  - [Cloud MySQL (AWS RDS)](#cloud-mysql-aws-rds)
- [Configuration Requirements](#configuration-requirements)
- [Error Handling](#error-handling)
- [Performance Considerations](#performance-considerations)
- [MySqlConnector Benefits](#mysqlconnector-benefits)
- [Connection Pool Configuration](#connection-pool-configuration)
- [MySQL-Specific Features](#mysql-specific-features)
  - [Character Set Support](#character-set-support)
  - [Time Zone Handling](#time-zone-handling)
  - [SSL Configuration](#ssl-configuration)
- [Related Packages](#related-packages)
- [License](#license)
- [Credits](#credits)

## Overview

`Syrx.Commanders.Databases.Connectors.MySql` provides the low-level MySQL database connector implementation for Syrx. This package contains the core connector logic that manages MySQL database connections using MySqlConnector.

## Features

- **MySQL Connectivity**: Native MySQL connection management
- **MySqlConnector Integration**: Uses MySqlConnectorFactory for optimal performance
- **Connection Pooling**: Leverages MySQL connection pooling capabilities
- **Thread-Safe Operations**: Safe for concurrent access
- **Configuration-Driven**: Connection management based on Syrx configuration settings

## Installation

> **Note**: This package is typically installed automatically as a dependency of `Syrx.MySql` or `Syrx.MySql.Extensions`.

```bash
dotnet add package Syrx.Commanders.Databases.Connectors.MySql
```

**Package Manager**
```bash
Install-Package Syrx.Commanders.Databases.Connectors.MySql
```

**PackageReference**
```xml
<PackageReference Include="Syrx.Commanders.Databases.Connectors.MySql" Version="3.0.0" />
```

## Architecture

This package implements the `IDatabaseConnector` interface specifically for MySQL:

```csharp
public class MySqlDatabaseConnector : DatabaseConnector
{
    public MySqlDatabaseConnector(ICommanderSettings settings) 
        : base(settings, () => MySqlConnectorFactory.Instance)
    {
    }
}
```

## Key Components

### MySqlDatabaseConnector

The main connector class that:
- Inherits from `DatabaseConnector` base class
- Uses `MySqlConnectorFactory.Instance` for creating MySQL connections
- Manages connection string resolution based on aliases
- Handles connection lifecycle management

### Connection Creation Process

1. **Alias Resolution**: Resolves connection string alias from command settings
2. **Factory Creation**: Uses MySqlConnectorFactory to create the connection instance
3. **Connection String Assignment**: Assigns the resolved connection string
4. **Connection Return**: Returns the configured IDbConnection

## Usage

This package is typically consumed through higher-level Syrx packages:

```csharp
// Usually configured through extensions
services.UseSyrx(builder => builder
    .UseMySql(mysql => mysql
        .AddConnectionString("Primary", connectionString)
        .AddCommand(/* configuration */)));
```

Direct usage (advanced scenarios):

```csharp
var settings = /* your ICommanderSettings */;
var connector = new MySqlDatabaseConnector(settings);
var connection = connector.CreateConnection(commandSetting);
```

## Connection String Support

Supports all standard MySQL connection string formats:

### Basic Authentication
```
Server=localhost;Database=mydatabase;Uid=myuser;Pwd=mypassword;
```

### With Port Specification
```
Server=localhost;Port=3306;Database=mydatabase;Uid=myuser;Pwd=mypassword;
```

### SSL/TLS Connections
```
Server=localhost;Database=mydatabase;Uid=myuser;Pwd=mypassword;SslMode=Required;
```

### Connection Pooling
```
Server=localhost;Database=mydatabase;Uid=myuser;Pwd=mypassword;
MinimumPoolSize=5;MaximumPoolSize=100;ConnectionTimeout=30;
```

### Cloud MySQL (AWS RDS)
```
Server=myinstance.region.rds.amazonaws.com;Database=mydatabase;
Uid=admin;Pwd=password;SslMode=Required;
```

## Configuration Requirements

Requires proper `ICommanderSettings` configuration with:
- Connection string settings with aliases
- Command settings that reference those aliases

Example configuration structure:
```csharp
{
    "Connections": [
        {
            "Alias": "Primary",
            "ConnectionString": "Server=localhost;Database=MyDb;Uid=user;Pwd=pass;"
        }
    ],
    "Namespaces": [
        {
            "Name": "MyApp.Repositories",
            "Types": [
                {
                    "Name": "UserRepository", 
                    "Commands": {
                        "GetUsers": {
                            "ConnectionAlias": "Primary",
                            "CommandText": "SELECT * FROM Users"
                        }
                    }
                }
            ]
        }
    ]
}
```

## Error Handling

The connector handles various error scenarios:
- **Missing Connection Alias**: Throws `NullReferenceException` with descriptive message
- **Invalid Connection String**: MySQL exceptions are propagated with full context
- **Connection Creation Failure**: Detailed error information is preserved

## Performance Considerations

- **MySqlConnector Advantage**: Uses the high-performance MySqlConnector library instead of Oracle's MySQL.Data
- **Connection Pooling**: Relies on MySQL's built-in connection pooling
- **Factory Pattern**: Minimal overhead using singleton MySqlConnectorFactory
- **Resource Management**: Proper disposal pattern implementation
- **Thread Safety**: Safe for concurrent operations

## MySqlConnector Benefits

This connector uses [MySqlConnector](https://github.com/mysql-net/MySqlConnector) which provides:
- **Better Performance**: Significantly faster than Oracle's MySQL.Data
- **Async Support**: True asynchronous operations
- **Memory Efficiency**: Lower memory usage and fewer allocations
- **Standards Compliance**: Better adherence to .NET standards
- **Active Development**: Regular updates and bug fixes

## Connection Pool Configuration

The connector supports comprehensive connection pool settings:

```csharp
"Server=localhost;Database=mydb;Uid=user;Pwd=pass;" +
"MinimumPoolSize=10;" +         // Minimum connections in pool
"MaximumPoolSize=200;" +        // Maximum connections in pool
"ConnectionTimeout=30;" +       // Connection timeout in seconds
"ConnectionLifeTime=0;" +       // Connection lifetime in seconds (0 = infinite)
"ConnectionReset=true;" +       // Reset connections when returned to pool
"LoadBalance=RoundRobin;"       // Load balancing for multiple servers
```

## MySQL-Specific Features

### Character Set Support
```
Server=localhost;Database=mydb;Uid=user;Pwd=pass;CharSet=utf8mb4;
```

### Time Zone Handling
```
Server=localhost;Database=mydb;Uid=user;Pwd=pass;ConvertZeroDateTime=true;
```

### SSL Configuration
```
Server=localhost;Database=mydb;Uid=user;Pwd=pass;
SslMode=Required;SslCert=client-cert.pem;SslKey=client-key.pem;SslCa=ca-cert.pem;
```

## Related Packages

- **[Syrx.MySql](https://www.nuget.org/packages/Syrx.MySql/)**: High-level MySQL provider
- **[Syrx.MySql.Extensions](https://www.nuget.org/packages/Syrx.MySql.Extensions/)**: Configuration extensions
- **[Syrx.Commanders.Databases.Connectors](https://www.nuget.org/packages/Syrx.Commanders.Databases.Connectors/)**: Base connector abstractions

## License

This project is licensed under the [MIT License](https://github.com/Syrx/Syrx/blob/main/LICENSE).

## Credits

- Built on top of [MySqlConnector](https://github.com/mysql-net/MySqlConnector)
- Follows [Dapper](https://github.com/DapperLib/Dapper) performance patterns

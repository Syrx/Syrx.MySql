# Syrx.MySql Documentation

> **MySQL database provider for the Syrx data access framework**

The Syrx.MySql ecosystem provides comprehensive MySQL database support for .NET applications using the Syrx data access framework. Built on top of Dapper and MySqlConnector, it offers high-performance, configuration-driven database operations with strong typing and seamless dependency injection integration.

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Core Concepts](#core-concepts)
- [Package Ecosystem](#package-ecosystem)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [MySQL-Specific Features](#mysql-specific-features)
- [Performance Considerations](#performance-considerations)
- [Advanced Topics](#advanced-topics)
- [Migration Guide](#migration-guide)
- [API Reference](#api-reference)
- [Examples](#examples)

## Architecture Overview

The Syrx.MySql ecosystem follows a layered architecture that separates concerns between application code, command resolution, connection management, and MySQL database execution:

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│  ┌─────────────────┐    ┌─────────────────┐                 │
│  │   Repository    │    │   Repository    │   ...           │
│  │     Class       │    │     Class       │                 │
│  └─────────────────┘    └─────────────────┘                 │
└─────────────────┬───────────────┬───────────────────────────┘
                  │               │
                  ▼               ▼
┌─────────────────────────────────────────────────────────────┐
│                 Commander Layer                             │
│  ┌────────────────────────────────────────────────────────┐ │
│  │           ICommander<TRepository>                      │ │
│  │                                                        │ │
│  │  ┌───────────────────────────────────────────────┐     │ │
│  │  │       DatabaseCommander<TRepository>          |     │ │
│  │  └───────────────────────────────────────────────┘     │ │
│  └────────────────────────────────────────────────────────┘ |
└─────────────────┬───────────────┬───────────────────────────┘
                  │               │
                  ▼               ▼
┌─────────────────────────────────────────────────────────────┐
│              MySQL Connector Layer                          │
│  ┌──────────────────┐    ┌──────────────────┐               │
│  │ MySQL Database   │    │ Connection       │               │
│  │   Connector      │    │   Management     │               │
│  └──────────────────┘    └──────────────────┘               │
└─────────────────┬───────────────┬───────────────────────────┘
                  │               │
                  ▼               ▼
┌─────────────────────────────────────────────────────────────┐
│                MySQL Database Layer                         │
│  ┌──────────────────┐    ┌──────────────────┐               │
│  │  MySqlConnector  │    │  MySQL Server    │               │
│  │     (Driver)     │    │   (Database)     │               │
│  └──────────────────┘    └──────────────────┘               │
└─────────────────────────────────────────────────────────────┘
```

### Key Architectural Principles

1. **MySQL-Optimized**: Built specifically for MySQL with MySqlConnector for optimal performance
2. **Configuration-Driven**: SQL commands are externalized from code
3. **Type Safety**: Strong typing throughout the execution pipeline
4. **Performance First**: Built on Dapper with MySQL-specific optimizations
5. **Connection Pooling**: Efficient MySQL connection management
6. **Thread Safety**: All components are fully thread-safe

## Core Concepts

### Repository Pattern with MySQL
Repositories define business operations that map to MySQL commands:

```csharp
public class UserRepository
{
    private readonly ICommander<UserRepository> _commander;
    
    public UserRepository(ICommander<UserRepository> commander)
    {
        _commander = commander;
    }
    
    // Method name automatically maps to configured MySQL command
    public async Task<User> GetByIdAsync(int id)
    {
        var users = await _commander.QueryAsync<User>(new { id });
        return users.FirstOrDefault();
    }
}
```

### MySQL Command Resolution
Commands are resolved using the pattern: `{Namespace}.{ClassName}.{MethodName}`

For the example above:
- **Namespace**: `MyApp.Repositories`
- **Class**: `UserRepository` 
- **Method**: `GetByIdAsync`
- **Resolved Command**: `MyApp.Repositories.UserRepository.GetByIdAsync`

### MySQL Connection Management
Named MySQL connection strings are resolved by alias:

```json
{
  "Connections": [
    {
      "Alias": "Primary",
      "ConnectionString": "Server=localhost;Database=MyApp;Uid=admin;Pwd=adminpass;MinimumPoolSize=5;MaximumPoolSize=100;"
    },
    {
      "Alias": "ReadOnly",
      "ConnectionString": "Server=readonly-mysql;Database=MyApp;Uid=reader;Pwd=readpass;SslMode=Required;"
    }
  ]
}
```

### Transaction Management with MySQL
- **Query Operations**: Execute without transactions (read-only)
- **Execute Operations**: Automatically wrapped in MySQL transactions with rollback on failure

## Package Ecosystem

The Syrx.MySql ecosystem consists of several interconnected packages:

### Core Packages

| Package | Purpose | Dependencies |
|---------|---------|--------------|
| **[Syrx.MySql](../src/Syrx.MySql/README.md)** | MySQL meta-package aggregator | Core database packages |
| **[Syrx.Commanders.Databases.Connectors.MySql](../src/Syrx.Commanders.Databases.Connectors.MySql/README.md)** | MySQL connection implementation | MySqlConnector, Syrx.Connectors |

### Extension Packages

| Package | Purpose | Use Case |
|---------|---------|-----------|
| **[Syrx.MySql.Extensions](../src/Syrx.MySql.Extensions/README.md)** | Configuration and DI extensions | Service registration |
| **[Syrx.Commanders.Databases.Connectors.MySql.Extensions](../src/Syrx.Commanders.Databases.Connectors.MySql.Extensions/README.md)** | MySQL connector DI extensions | Connector registration |

### Dependencies from Submodules

| Package | Purpose | Source |
|---------|---------|--------|
| **Syrx.Commanders.Databases** | Core command execution engine | Submodule |
| **Syrx.Commanders.Databases.Extensions** | DI container registration | Submodule |
| **Syrx.Commanders.Databases.Settings** | Configuration model definitions | Submodule |

## Getting Started

### 1. Installation

Install the MySQL extensions package (recommended):

```bash
# Recommended: Complete MySQL support with extensions
dotnet add package Syrx.MySql.Extensions

# Alternative: Core package only
dotnet add package Syrx.MySql
```

### 2. Configuration

Create a MySQL-specific configuration:

```json
{
  "Connections": [
    {
      "Alias": "DefaultConnection",
      "ConnectionString": "Server=localhost;Database=MyApp;Uid=myuser;Pwd=mypass;MinimumPoolSize=5;MaximumPoolSize=100;"
    }
  ],
  "Namespaces": [
    {
      "Name": "MyApp.Repositories",
      "Types": [
        {
          "Name": "UserRepository",
          "Commands": {
            "GetByIdAsync": {
              "CommandText": "SELECT * FROM Users WHERE Id = @id",
              "ConnectionAlias": "DefaultConnection"
            },
            "CreateUserAsync": {
              "CommandText": "INSERT INTO Users (Name, Email, CreatedDate) VALUES (@Name, @Email, @CreatedDate); SELECT LAST_INSERT_ID();",
              "ConnectionAlias": "DefaultConnection"
            }
          }
        }
      ]
    }
  ]
}
```

### 3. Service Registration

Register services in your DI container:

```csharp
using Syrx.MySql.Extensions;

public void ConfigureServices(IServiceCollection services)
{
    services.UseSyrx(builder => builder
        .UseMySql(mysql => mysql
            .AddConnectionString("Default", "Server=localhost;Database=MyApp;Uid=user;Pwd=pass;")
            .AddCommand(types => types
                .ForType<UserRepository>(methods => methods
                    .ForMethod("GetByIdAsync", cmd => cmd
                        .UseConnectionAlias("Default")
                        .UseCommandText("SELECT * FROM Users WHERE Id = @id"))))));
    
    // Register your repositories
    services.AddScoped<UserRepository>();
}
```

### 4. Repository Implementation

Create your repository classes:

```csharp
public class UserRepository
{
    private readonly ICommander<UserRepository> _commander;
    
    public UserRepository(ICommander<UserRepository> commander)
    {
        _commander = commander;
    }
    
    public async Task<User> GetByIdAsync(int id)
    {
        var users = await _commander.QueryAsync<User>(new { id });
        return users.FirstOrDefault();
    }
    
    public async Task<int> CreateUserAsync(User user)
    {
        // MySQL-specific: Use LAST_INSERT_ID() to get auto-increment value
        var result = await _commander.QueryAsync<int>(new 
        { 
            user.Name, 
            user.Email, 
            CreatedDate = DateTime.UtcNow 
        });
        return result.FirstOrDefault();
    }
}
```

## Configuration

### MySQL Connection String Format

MySQL connection strings follow the MySqlConnector format:

```
Server=hostname;Port=3306;Database=dbname;Uid=username;Pwd=password;[additional options]
```

### Common MySQL Connection String Options

| Option | Description | Example |
|--------|-------------|---------|
| `Server` | MySQL server hostname | `Server=localhost` |
| `Port` | MySQL server port (default: 3306) | `Port=3306` |
| `Database` | Database name | `Database=MyApp` |  
| `Uid` | Username | `Uid=myuser` |
| `Pwd` | Password | `Pwd=mypassword` |
| `SslMode` | SSL connection mode | `SslMode=Required` |
| `MinimumPoolSize` | Minimum connection pool size | `MinimumPoolSize=5` |
| `MaximumPoolSize` | Maximum connection pool size | `MaximumPoolSize=100` |
| `ConnectionTimeout` | Connection timeout in seconds | `ConnectionTimeout=30` |
| `CharSet` | Character set | `CharSet=utf8mb4` |

### MySQL-Specific Configuration Examples

#### Local Development
```json
{
  "ConnectionString": "Server=localhost;Database=myapp_dev;Uid=dev;Pwd=devpass;"
}
```

#### Production with SSL and Connection Pooling
```json
{
  "ConnectionString": "Server=prod-mysql.example.com;Database=myapp;Uid=produser;Pwd=prodpass;SslMode=Required;MinimumPoolSize=10;MaximumPoolSize=200;ConnectionTimeout=30;"
}
```

#### AWS RDS MySQL
```json
{
  "ConnectionString": "Server=myinstance.region.rds.amazonaws.com;Database=myapp;Uid=admin;Pwd=password;SslMode=Required;ConnectionTimeout=60;"
}
```

#### Azure Database for MySQL
```json
{
  "ConnectionString": "Server=myserver.mysql.database.azure.com;Database=myapp;Uid=admin@myserver;Pwd=password;SslMode=Required;"
}
```

## MySQL-Specific Features

### Auto-Increment Handling

MySQL auto-increment values can be retrieved using `LAST_INSERT_ID()`:

```sql
-- Command configuration
"CreateUser": {
  "CommandText": "INSERT INTO Users (Name, Email) VALUES (@Name, @Email); SELECT LAST_INSERT_ID();",
  "ConnectionAlias": "Primary"
}
```

```csharp
// Repository method
public async Task<int> CreateUserAsync(User user)
{
    var result = await _commander.QueryAsync<int>(user);
    return result.FirstOrDefault(); // Returns the new auto-increment ID
}
```

### MySQL JSON Column Support

MySQL's native JSON data type is fully supported:

```sql
-- Table with JSON column
CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100),
    Preferences JSON
);
```

```sql
-- Command using JSON functions
"GetUserPreferences": {
  "CommandText": "SELECT Id, Name, JSON_EXTRACT(Preferences, '$.theme') as Theme FROM Users WHERE Id = @id",
  "ConnectionAlias": "Primary"
}
```

### MySQL Stored Procedures

Full support for MySQL stored procedures:

```json
{
  "CallUserReport": {
    "CommandText": "sp_GenerateUserReport",
    "CommandType": "StoredProcedure",
    "ConnectionAlias": "Primary",
    "CommandTimeout": 60
  }
}
```

### MySQL-Specific Data Types

Support for MySQL-specific data types through MySqlConnector:

- `DATETIME` and `TIMESTAMP` with time zone handling
- `JSON` data type with extraction functions
- `GEOMETRY` spatial data types
- `ENUM` and `SET` types
- `BIGINT` and `DECIMAL` precision types

## Performance Considerations

### Connection Pooling Best Practices

Configure connection pools for optimal MySQL performance:

```json
{
  "ConnectionString": "Server=localhost;Database=myapp;Uid=user;Pwd=pass;MinimumPoolSize=5;MaximumPoolSize=100;ConnectionTimeout=30;ConnectionLifeTime=0;ConnectionReset=true;"
}
```

### Read/Write Separation

Configure separate connections for read and write operations:

```json
{
  "Connections": [
    {
      "Alias": "Primary",
      "ConnectionString": "Server=mysql-primary;Database=myapp;Uid=admin;Pwd=adminpass;"
    },
    {
      "Alias": "Replica", 
      "ConnectionString": "Server=mysql-replica;Database=myapp;Uid=reader;Pwd=readpass;"
    }
  ]
}
```

### MySQL Query Optimization

- Use parameterized queries to leverage MySQL's prepared statement cache
- Configure appropriate `CommandTimeout` values for long-running queries
- Use `LIMIT` clauses for large result sets
- Leverage MySQL's query cache when appropriate

### MySqlConnector Performance Benefits

- **Async/Await Support**: True asynchronous operations
- **Memory Efficiency**: Reduced memory allocations
- **Connection Pool Management**: Optimized connection pooling
- **Prepared Statements**: Automatic prepared statement caching

## Advanced Topics

### Multi-Mapping with MySQL JOINs

Handle complex MySQL JOINs with object composition:

```csharp
public async Task<IEnumerable<User>> GetUsersWithProfilesAsync()
{
    return await _commander.QueryAsync<User, Profile, User>(
        (user, profile) => 
        {
            user.Profile = profile;
            return user;
        });
}
```

Configuration:
```json
{
  "GetUsersWithProfilesAsync": {
    "CommandText": "SELECT u.*, p.* FROM Users u JOIN Profiles p ON u.Id = p.UserId",
    "SplitOn": "Id",
    "ConnectionAlias": "Primary"
  }
}
```

### MySQL Transaction Isolation Levels

Configure MySQL-specific transaction isolation:

```json
{
  "UpdateUserBalance": {
    "CommandText": "UPDATE Users SET Balance = Balance + @amount WHERE Id = @id",
    "IsolationLevel": "RepeatableRead",
    "ConnectionAlias": "Primary"
  }
}
```

### Bulk Operations with MySQL

Optimize bulk operations for MySQL:

```json
{
  "BulkInsertUsers": {
    "CommandText": "INSERT INTO Users (Name, Email) VALUES (@Name, @Email)",
    "CommandTimeout": 300,
    "ConnectionAlias": "Primary"
  }
}
```

## Migration Guide

### From MySQL.Data to MySqlConnector

This package uses MySqlConnector instead of Oracle's MySQL.Data for better performance:

| MySQL.Data | MySqlConnector | Benefits |
|------------|----------------|-----------|
| Oracle's provider | Community-driven | Better performance |
| Limited async support | Full async/await | True asynchronous operations |
| Higher memory usage | Memory efficient | Reduced allocations |
| Slower connection pooling | Optimized pooling | Better scalability |

### From Entity Framework to Syrx.MySql

```csharp
// Entity Framework
public async Task<User> GetUserAsync(int id)
{
    return await _context.Users.FindAsync(id);
}

// Syrx.MySql
public async Task<User> GetUserAsync(int id)
{
    var users = await _commander.QueryAsync<User>(new { id });
    return users.FirstOrDefault();
}
```

### From Raw MySQL ADO.NET

```csharp
// Raw ADO.NET with MySql.Data
public async Task<User> GetUserAsync(int id)
{
    using var connection = new MySqlConnection(connectionString);
    using var command = new MySqlCommand("SELECT * FROM Users WHERE Id = @id", connection);
    command.Parameters.AddWithValue("@id", id);
    // ... manual mapping
}

// Syrx.MySql
public async Task<User> GetUserAsync(int id)
{
    var users = await _commander.QueryAsync<User>(new { id });
    return users.FirstOrDefault();
}
```

## API Reference

### Core Interfaces

- **[ICommander&lt;TRepository&gt;](api-reference.md#icommander)**: Primary interface for repository operations
- **[MySqlDatabaseConnector](api-reference.md#mysqldatabaseconnector)**: MySQL connection implementation

### Extension Methods

- **[MySqlConnectorExtensions.UseMySql](api-reference.md#usemysql)**: MySQL configuration extension
- **[ServiceCollectionExtensions.AddMySql](api-reference.md#addmysql)**: Service registration extension

### Configuration Models

All configuration models are inherited from the base Syrx framework:
- **CommandSetting**: Individual command configuration
- **ConnectionStringSetting**: MySQL connection string configuration
- **NamespaceSetting**: Namespace-level configuration

## Examples

For complete working examples, see:

- **[Basic CRUD Operations](examples/basic-crud.md)**
- **[MySQL JSON Columns](examples/json-columns.md)**
- **[Stored Procedures](examples/stored-procedures.md)**
- **[Auto-Increment Handling](examples/auto-increment.md)**
- **[Connection Pooling](examples/connection-pooling.md)**
- **[SSL Connections](examples/ssl-connections.md)**
- **[Multi-Mapping Queries](examples/multi-mapping.md)**
- **[Transaction Management](examples/transactions.md)**

## Contributing

See the [Contributing Guide](../CONTRIBUTING.md) for information about:
- Development setup
- Coding standards
- Pull request process
- Testing requirements

## Support

- **Documentation**: [Complete documentation](./)
- **Issues**: [GitHub Issues](https://github.com/Syrx/Syrx.MySql/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Syrx/Syrx.MySql/discussions)

## License

This project is licensed under the [MIT License](../LICENSE).
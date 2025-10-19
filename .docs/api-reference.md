# API Reference - Syrx.MySql

This document provides detailed API reference for all public types and methods in the Syrx.MySql ecosystem.

## Table of Contents

- [MySqlDatabaseConnector](#mysqldatabaseconnector)
- [MySqlConnectorExtensions](#mysqlconnectorextensions)
- [ServiceCollectionExtensions](#servicecollectionextensions)
- [Configuration Models](#configuration-models)
- [Common Usage Patterns](#common-usage-patterns)

## MySqlDatabaseConnector

**Namespace:** `Syrx.Commanders.Databases.Connectors.MySql`  
**Assembly:** `Syrx.Commanders.Databases.Connectors.MySql.dll`

MySQL database connector implementation for the Syrx data access framework.

### Class Declaration

```csharp
public class MySqlDatabaseConnector : DatabaseConnector
```

### Inheritance Hierarchy

```
System.Object
  └── DatabaseConnector
      └── MySqlDatabaseConnector
```

### Constructors

#### MySqlDatabaseConnector(ICommanderSettings)

Initializes a new instance of the MySqlDatabaseConnector class.

```csharp
public MySqlDatabaseConnector(ICommanderSettings settings)
```

**Parameters:**
- `settings` *(ICommanderSettings)*: The commander settings containing connection string aliases and command configurations.

**Exceptions:**
- `ArgumentNullException`: Thrown when settings is null.

**Remarks:**
The constructor passes the MySqlConnectorFactory.Instance to the base DatabaseConnector, which enables the creation of MySQL-specific database connections.

### Example Usage

```csharp
// Typically used through dependency injection
var connector = serviceProvider.GetService<IDatabaseConnector>();
var connection = connector.CreateConnection(commandSetting);
```

## MySqlConnectorExtensions

**Namespace:** `Syrx.Commanders.Databases.Connectors.MySql.Extensions`  
**Assembly:** `Syrx.Commanders.Databases.Connectors.MySql.Extensions.dll`

Extension methods for configuring MySQL database connectors in the Syrx framework.

### Class Declaration

```csharp
public static class MySqlConnectorExtensions
```

### Methods

#### UseMySql(SyrxBuilder, Action&lt;CommanderSettingsBuilder&gt;, ServiceLifetime)

Configures the Syrx builder to use MySQL database connectivity with the specified settings.

```csharp
public static SyrxBuilder UseMySql(
    this SyrxBuilder builder,
    Action<CommanderSettingsBuilder> factory,
    ServiceLifetime lifetime = ServiceLifetime.Singleton)
```

**Parameters:**
- `builder` *(SyrxBuilder)*: The Syrx builder instance to configure.
- `factory` *(Action&lt;CommanderSettingsBuilder&gt;)*: An action that configures the CommanderSettingsBuilder with connection strings, command mappings, and other MySQL-specific settings.
- `lifetime` *(ServiceLifetime)*: The service lifetime for registered services. Defaults to Singleton.

**Returns:**
*(SyrxBuilder)*: The configured SyrxBuilder instance for method chaining.

**Exceptions:**
- `ArgumentNullException`: Thrown when builder or factory is null.

**Service Registrations:**
This method performs the following service registrations:
- Registers `ICommanderSettings` as a singleton with the built configuration
- Registers the database command reader with the specified lifetime
- Registers the MySQL database connector with the specified lifetime  
- Registers the database commander with the specified lifetime

### Example Usage

```csharp
services.UseSyrx(builder => builder
    .UseMySql(mysql => mysql
        .AddConnectionString("Primary", "Server=localhost;Database=MyApp;Uid=user;Pwd=pass;")
        .AddCommand(types => types
            .ForType<UserRepository>(methods => methods
                .ForMethod("GetByIdAsync", cmd => cmd
                    .UseConnectionAlias("Primary")
                    .UseCommandText("SELECT * FROM Users WHERE Id = @id")))),
    ServiceLifetime.Scoped));
```

## ServiceCollectionExtensions

**Namespace:** `Syrx.Commanders.Databases.Connectors.MySql.Extensions`  
**Assembly:** `Syrx.Commanders.Databases.Connectors.MySql.Extensions.dll`

Extension methods for registering MySQL database connector services with dependency injection containers.

### Class Declaration

```csharp
public static class ServiceCollectionExtensions
```

### Methods

#### AddMySql(IServiceCollection, ServiceLifetime)

Registers the MySQL database connector implementation with the service collection.

```csharp
internal static IServiceCollection AddMySql(
    this IServiceCollection services, 
    ServiceLifetime lifetime = ServiceLifetime.Transient)
```

**Parameters:**
- `services` *(IServiceCollection)*: The service collection to register the connector with.
- `lifetime` *(ServiceLifetime)*: The service lifetime for the connector registration. Defaults to Transient.

**Returns:**
*(IServiceCollection)*: The service collection for method chaining.

**Exceptions:**
- `ArgumentNullException`: Thrown when services is null.

**Remarks:**
This method is marked as internal because it's designed to be called by the framework's configuration methods rather than directly by application code. Use the higher-level `MySqlConnectorExtensions.UseMySql` method for typical configuration scenarios.

**Service Lifetime Considerations:**
- **Transient:** New instance per injection (default, safe for all scenarios)
- **Scoped:** One instance per scope/request (suitable for web applications)
- **Singleton:** Single instance for application lifetime (best performance, requires thread-safe usage)

## Configuration Models

The Syrx.MySql packages use the standard Syrx configuration models from the base framework:

### ICommanderSettings

Main configuration interface containing connections and command mappings.

```csharp
public interface ICommanderSettings : ISettings<CommandSetting>
{
    List<ConnectionStringSetting>? Connections { get; init; }
    List<NamespaceSetting> Namespaces { get; init; }
}
```

### ConnectionStringSetting

Represents a named connection string configuration.

```csharp
public sealed record ConnectionStringSetting
{
    public required string Alias { get; init; }
    public required string ConnectionString { get; init; }
}
```

**MySQL Connection String Example:**
```json
{
  "Alias": "Primary",
  "ConnectionString": "Server=localhost;Database=MyApp;Uid=user;Pwd=pass;MinimumPoolSize=5;MaximumPoolSize=100;"
}
```

### CommandSetting

Represents an individual command configuration.

```csharp
public sealed record CommandSetting : ICommandSetting
{
    public required string CommandText { get; init; }
    public string ConnectionAlias { get; init; } = "Default";
    public int? CommandTimeout { get; init; }
    public CommandType? CommandType { get; init; }
    public string? SplitOn { get; init; }
    public IsolationLevel? IsolationLevel { get; init; }
}
```

**MySQL Command Example:**
```json
{
  "CommandText": "SELECT * FROM Users WHERE Id = @id",
  "ConnectionAlias": "Primary",
  "CommandTimeout": 30,
  "CommandType": "Text"
}
```

### NamespaceSetting

Represents namespace-level configuration grouping.

```csharp
public sealed record NamespaceSetting
{
    public required string Name { get; init; }
    public required List<TypeSetting> Types { get; init; }
}
```

### TypeSetting

Represents type-level configuration grouping.

```csharp  
public sealed record TypeSetting
{
    public required string Name { get; init; }
    public required Dictionary<string, CommandSetting> Commands { get; init; }
}
```

## Common Usage Patterns

### Basic Repository Setup

```csharp
// 1. Service registration
services.UseSyrx(builder => builder
    .UseMySql(mysql => mysql
        .AddConnectionString("Default", connectionString)
        .AddCommand(/* commands */)));

// 2. Repository class
public class UserRepository
{
    private readonly ICommander<UserRepository> _commander;
    
    public UserRepository(ICommander<UserRepository> commander)
    {
        _commander = commander;
    }
    
    // Methods automatically resolve to configured commands
    public async Task<User> GetByIdAsync(int id) =>
        (await _commander.QueryAsync<User>(new { id })).FirstOrDefault();
}
```

### Multiple Connection Strings

```csharp
services.UseSyrx(builder => builder
    .UseMySql(mysql => mysql
        .AddConnectionString("Primary", primaryConnectionString)
        .AddConnectionString("Replica", replicaConnectionString)
        .AddCommand(types => types
            .ForType<UserRepository>(methods => methods
                .ForMethod("GetUsers", cmd => cmd
                    .UseConnectionAlias("Replica"))  // Read from replica
                .ForMethod("CreateUser", cmd => cmd  
                    .UseConnectionAlias("Primary"))))); // Write to primary
```

### MySQL-Specific Command Configuration

```csharp
.AddCommand(types => types
    .ForType<UserRepository>(methods => methods
        .ForMethod("CreateUserWithId", cmd => cmd
            .UseConnectionAlias("Primary")
            .UseCommandText("INSERT INTO Users (Name, Email) VALUES (@Name, @Email); SELECT LAST_INSERT_ID();")
            .SetCommandTimeout(30))
        .ForMethod("CallStoredProcedure", cmd => cmd
            .UseConnectionAlias("Primary")
            .UseCommandText("sp_ProcessUser")
            .SetCommandType(CommandType.StoredProcedure)
            .SetCommandTimeout(60))))
```

### Service Lifetime Configuration

```csharp
// Singleton lifetime for best performance (default)
services.UseSyrx(builder => builder
    .UseMySql(/* config */, ServiceLifetime.Singleton));

// Scoped lifetime for web applications  
services.UseSyrx(builder => builder
    .UseMySql(/* config */, ServiceLifetime.Scoped));

// Transient lifetime for maximum isolation
services.UseSyrx(builder => builder
    .UseMySql(/* config */, ServiceLifetime.Transient));
```

### Error Handling Patterns

```csharp
public async Task<User> GetUserAsync(int id)
{
    try
    {
        var users = await _commander.QueryAsync<User>(new { id });
        return users.FirstOrDefault();
    }
    catch (MySqlException ex)
    {
        // MySQL-specific exception handling
        _logger.LogError(ex, "MySQL error occurred: {ErrorCode}", ex.ErrorCode);
        throw;
    }
    catch (TimeoutException ex)
    {
        // Command timeout handling
        _logger.LogError(ex, "MySQL command timeout");
        throw;
    }
}
```

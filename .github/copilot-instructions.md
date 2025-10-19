````instructions
# Copilot Instructions for Syrx.MySql

This document provides guidance for GitHub Copilot and other AI assistants working with the Syrx.MySql codebase.

## Project Overview

Syrx.MySql provides MySQL database support for the Syrx data access framework. Built on top of Dapper and MySqlConnector, it offers high-performance, configuration-driven database operations with strong typing and seamless dependency injection integration.

## Architecture Principles

### Core Design Patterns

1. **MySQL-First Design**: Optimized specifically for MySQL with MySqlConnector for best performance
2. **Configuration-Driven**: SQL commands are externalized from code using JSON/XML configuration
3. **Command Resolution**: Methods are automatically mapped to commands using `{Namespace}.{ClassName}.{MethodName}` pattern
4. **Connection Management**: MySQL connections are managed through named aliases with pooling support
5. **Extension-Based**: Functionality is provided through extension packages for easy configuration

### Project Structure

```
src/
├── Syrx.MySql/                                           # Meta-package aggregator
├── Syrx.MySql.Extensions/                                # Extension meta-package aggregator
├── Syrx.Commanders.Databases.Connectors.MySql/          # Core MySQL connector
└── Syrx.Commanders.Databases.Connectors.MySql.Extensions/ # MySQL DI extensions
```

### Dependency Architecture

```
Syrx.MySql.Extensions (meta-package)
├── Syrx.MySql (meta-package)
│   ├── Syrx.Commanders.Databases.Connectors.MySql
│   └── Syrx.Commanders.Databases (via submodule)
└── Syrx.Commanders.Databases.Connectors.MySql.Extensions
    └── Syrx.Commanders.Databases.Extensions (via submodule)
```

## Coding Standards

### C# Style Guidelines

1. **XML Documentation**: All public interfaces, classes, and methods MUST have comprehensive XML documentation
   ```csharp
   /// <summary>
   /// Configures the Syrx builder to use MySQL database connectivity with the specified settings.
   /// </summary>
   /// <param name="builder">The Syrx builder instance to configure.</param>
   /// <param name="factory">An action that configures the CommanderSettingsBuilder.</param>
   /// <param name="lifetime">The service lifetime for registered services.</param>
   /// <returns>The configured SyrxBuilder instance for method chaining.</returns>
   /// <exception cref="ArgumentNullException">Thrown when builder or factory is null.</exception>
   public static SyrxBuilder UseMySql(this SyrxBuilder builder, Action<CommanderSettingsBuilder> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton)
   ```

2. **MySQL-Specific Considerations**: Always consider MySQL-specific features and limitations
   ```csharp
   // Good: MySQL-aware configuration
   .UseCommandText("INSERT INTO Users (Name, Email) VALUES (@Name, @Email); SELECT LAST_INSERT_ID();")
   
   // Good: MySQL connection string format
   "Server=localhost;Database=MyApp;Uid=user;Pwd=pass;MinimumPoolSize=5;MaximumPoolSize=100;"
   ```

3. **Extension Method Patterns**: Follow consistent extension method patterns
   ```csharp
   // Extension methods should be fluent and chainable
   services.UseSyrx(builder => builder
       .UseMySql(mysql => mysql
           .AddConnectionString("Primary", connectionString)
           .AddCommand(types => types.ForType<Repository>())));
   ```

4. **Nullable Reference Types**: The project uses nullable reference types - be explicit about nullability
   ```csharp
   public List<ConnectionStringSetting>? Connections { get; init; } // Nullable
   public required List<NamespaceSetting> Namespaces { get; init; } // Required
   ```

### MySQL-Specific Coding Patterns

1. **Connection String Management**: Always use named aliases for connection strings
   ```csharp
   // Good: Named connection with MySQL-specific options
   .AddConnectionString("Primary", "Server=localhost;Database=myapp;Uid=user;Pwd=pass;MinimumPoolSize=5;MaximumPoolSize=100;")
   
   // Avoid: Direct connection strings in command configuration
   ```

2. **Auto-Increment Handling**: Use MySQL's LAST_INSERT_ID() for auto-increment values
   ```sql
   INSERT INTO Users (Name, Email) VALUES (@Name, @Email); SELECT LAST_INSERT_ID();
   ```

3. **MySQL JSON Support**: Leverage MySQL's native JSON functions
   ```sql
   SELECT Id, Name, JSON_EXTRACT(Preferences, '$.theme') as Theme FROM Users WHERE Id = @id
   ```

4. **Connection Pooling**: Configure appropriate pool settings
   ```
   MinimumPoolSize=5;MaximumPoolSize=100;ConnectionTimeout=30;ConnectionLifeTime=0;
   ```

### Naming Conventions

- **Classes**: PascalCase (e.g., `MySqlDatabaseConnector`)
- **Extension Classes**: End with `Extensions` (e.g., `MySqlConnectorExtensions`)
- **Internal Methods**: Use descriptive names and mark as `internal` when appropriate
- **MySQL-Specific**: Prefix with `MySql` when MySQL-specific (e.g., `MySqlDatabaseConnector`)

### Error Handling Patterns

1. **MySQL Exception Handling**: Allow MySQL-specific exceptions to bubble up
   ```csharp
   // Don't catch MySqlException - let consumers handle them
   // Do validate parameters before MySQL operations
   Throw<ArgumentNullException>(builder != null, nameof(builder));
   ```

2. **Connection Validation**: Validate connection strings and aliases
   ```csharp
   Throw<InvalidOperationException>(commandSetting != null, "MySQL command not configured");
   ```

## Package Dependencies

### Core Dependencies
- **MySqlConnector**: High-performance MySQL .NET connector
- **Syrx.Commanders.Databases**: Core database framework (via submodule)
- **Microsoft.Extensions.DependencyInjection**: Dependency injection support

### MySQL-Specific Dependencies
```xml
<PackageReference Include="MySqlConnector" Version="..." />
```

### Internal Dependencies
```
Syrx.MySql (meta-package)
├── Syrx.Commanders.Databases.Connectors.MySql
└── Syrx.Commanders.Databases (submodule)

Syrx.MySql.Extensions (meta-package)
├── Syrx.MySql
└── Syrx.Commanders.Databases.Connectors.MySql.Extensions

Syrx.Commanders.Databases.Connectors.MySql.Extensions
├── Syrx.Commanders.Databases.Connectors.MySql
└── Syrx.Commanders.Databases.Extensions (submodule)
```

## Key Implementations

### MySqlDatabaseConnector
- **Purpose**: MySQL-specific database connection creation
- **Implementation**: Inherits from `DatabaseConnector` with `MySqlConnectorFactory.Instance`
- **Key Feature**: Uses high-performance MySqlConnector library

### MySqlConnectorExtensions
- **Purpose**: Fluent configuration API for MySQL
- **Key Method**: `UseMySql()` - configures MySQL support with dependency injection
- **Pattern**: Builder pattern with service lifetime configuration

### ServiceCollectionExtensions
- **Purpose**: Internal service registration for MySQL connector
- **Key Method**: `AddMySql()` - registers MySQL connector with DI container
- **Visibility**: Internal - used by framework, not application code

## Configuration Architecture

### MySQL Connection String Format
```
Server=hostname;Port=3306;Database=dbname;Uid=username;Pwd=password;[options]
```

### Common MySQL Options
- `MinimumPoolSize=5` - Minimum connections in pool
- `MaximumPoolSize=100` - Maximum connections in pool  
- `ConnectionTimeout=30` - Connection timeout in seconds
- `SslMode=Required` - SSL connection mode
- `CharSet=utf8mb4` - Character set
- `ConnectionLifeTime=0` - Connection lifetime (0 = infinite)

### Configuration Sources
1. **Programmatic**: Builder pattern configuration (recommended)
2. **JSON**: Configuration files (inherited from base framework)
3. **XML**: Alternative configuration format (inherited from base framework)

## MySQL-Specific Testing Patterns

### Integration Testing
```csharp
// Use real MySQL database with test configuration
services.UseSyrx(builder => builder
    .UseMySql(mysql => mysql
        .AddConnectionString("Test", "Server=localhost;Database=test_db;Uid=test;Pwd=test;")
        .AddCommand(/* test commands */)));
```

### Connection String Testing
```csharp
// Test various MySQL connection string formats
var testCases = new[]
{
    "Server=localhost;Database=test;Uid=user;Pwd=pass;",
    "Server=localhost;Port=3306;Database=test;Uid=user;Pwd=pass;SslMode=Required;",
    "Server=localhost;Database=test;Uid=user;Pwd=pass;MinimumPoolSize=5;MaximumPoolSize=50;"
};
```

## Performance Considerations

### MySqlConnector Benefits
- **Better Performance**: Significantly faster than Oracle's MySQL.Data
- **Async Support**: True asynchronous operations
- **Memory Efficiency**: Lower memory usage and fewer allocations
- **Standards Compliance**: Better adherence to .NET standards

### Connection Pool Configuration
```csharp
// Optimize for production workloads
"Server=localhost;Database=myapp;Uid=user;Pwd=pass;" +
"MinimumPoolSize=10;" +         // Pre-warm connections
"MaximumPoolSize=200;" +        // Handle peak load
"ConnectionTimeout=30;" +       // Reasonable timeout
"ConnectionLifeTime=300;" +     // Recycle connections
"ConnectionReset=true;"         // Reset state on return
```

### MySQL Query Optimization
- Use parameterized queries for prepared statement caching
- Configure appropriate `CommandTimeout` for long-running queries
- Use `LIMIT` clauses for large result sets
- Leverage MySQL's JSON functions for JSON column queries

## MySQL-Specific Features to Support

### Auto-Increment Handling
```sql
-- Always use LAST_INSERT_ID() for auto-increment values
INSERT INTO Users (Name, Email) VALUES (@Name, @Email); SELECT LAST_INSERT_ID();
```

### JSON Data Type Support
```sql
-- Support MySQL's native JSON functions
SELECT JSON_EXTRACT(data, '$.property') FROM table WHERE id = @id
```

### Stored Procedures
```json
{
  "CommandText": "sp_ProcedureName",
  "CommandType": "StoredProcedure",
  "CommandTimeout": 60
}
```

### SSL/TLS Configuration
```
Server=server;Database=db;Uid=user;Pwd=pass;SslMode=Required;SslCert=cert.pem;SslKey=key.pem;
```

## Documentation Standards

### README.md Files
- Each project MUST have a comprehensive README.md suitable for NuGet
- Include MySQL-specific examples and connection string formats
- Provide installation instructions and usage examples
- Reference version 2.4.3 (current version from Directory.Build.props)

### XML Documentation
- All public APIs require comprehensive XML docs
- Include MySQL-specific remarks and examples
- Document MySQL connection string parameters
- Reference MySqlConnector benefits and features

### .docs Folder
- Comprehensive cross-project documentation
- MySQL-specific architecture guides
- API reference with MySQL examples
- Performance guides and best practices

## Common Patterns to Follow

### Service Registration
```csharp
// Standard MySQL registration pattern
services.UseSyrx(builder => builder
    .UseMySql(mysql => mysql
        .AddConnectionString("Primary", "Server=localhost;Database=myapp;Uid=user;Pwd=pass;")
        .AddCommand(types => types
            .ForType<UserRepository>(methods => methods
                .ForMethod("GetByIdAsync", cmd => cmd
                    .UseConnectionAlias("Primary")
                    .UseCommandText("SELECT * FROM Users WHERE Id = @id"))))));
```

### Repository Implementation
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
    
    // MySQL auto-increment handling
    public async Task<int> CreateUserAsync(User user)
    {
        var result = await _commander.QueryAsync<int>(user);
        return result.FirstOrDefault(); // LAST_INSERT_ID()
    }
}
```

### MySQL Configuration Structure
```json
{
  "Connections": [
    {
      "Alias": "Primary",
      "ConnectionString": "Server=localhost;Database=myapp;Uid=user;Pwd=pass;MinimumPoolSize=5;MaximumPoolSize=100;"
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
              "ConnectionAlias": "Primary"
            },
            "CreateUserAsync": {
              "CommandText": "INSERT INTO Users (Name, Email) VALUES (@Name, @Email); SELECT LAST_INSERT_ID();",
              "ConnectionAlias": "Primary"
            }
          }
        }
      ]
    }
  ]
}
```

## Things to Avoid

1. **Don't use MySQL.Data**: Always use MySqlConnector for better performance
2. **Don't hard-code connection strings**: Use named aliases in configuration
3. **Don't ignore MySQL-specific features**: Leverage JSON, auto-increment, stored procedures
4. **Don't bypass connection pooling**: Configure appropriate pool settings
5. **Don't catch MySQL exceptions in framework code**: Let them bubble up for proper handling
6. **Don't mix MySQL and generic SQL**: Use MySQL-specific syntax when beneficial

## Thread Safety

All MySQL components in the framework are **fully thread-safe**:

- **MySqlDatabaseConnector**: Thread-safe connection creation using MySqlConnectorFactory
- **Configuration Classes**: Immutable records after construction
- **Extension Methods**: Stateless operation, safe for concurrent use
- **Connection Pooling**: MySqlConnector handles thread-safe connection pooling

## Debugging Tips

### MySQL Connection Issues
- Verify MySQL server is running and accessible
- Check connection string format (MySQL.Data vs MySqlConnector syntax differences)
- Verify user permissions and database existence
- Test SSL/TLS configuration if using secure connections

### Performance Issues
- Enable MySQL slow query log to identify bottlenecks
- Monitor connection pool usage and configure appropriately
- Use MySQL's EXPLAIN to analyze query performance
- Consider read/write separation for high-load scenarios

### Common MySQL Errors
- **Authentication failures**: Check Uid/Pwd and user permissions
- **SSL errors**: Verify SslMode and certificate configuration
- **Timeout errors**: Adjust ConnectionTimeout and CommandTimeout
- **Pool exhaustion**: Increase MaximumPoolSize or check connection leaks

## Future Considerations

When extending or modifying the MySQL framework:

1. **MySqlConnector Updates**: Stay current with MySqlConnector library updates
2. **MySQL Version Support**: Ensure compatibility with supported MySQL versions (5.7+, 8.0+)
3. **Cloud Provider Support**: Test with AWS RDS, Azure Database for MySQL, Google Cloud SQL
4. **Performance Monitoring**: Consider adding performance counters and metrics
5. **SSL/TLS Evolution**: Stay current with MySQL SSL/TLS requirements

This framework is designed for high-performance MySQL database access with enterprise-grade features. All changes should maintain these quality standards while leveraging MySQL-specific capabilities for optimal performance.
````
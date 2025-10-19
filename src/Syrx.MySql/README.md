# Syrx.MySql

MySQL database provider for the Syrx data access framework.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
  - [1. Configure Services](#1-configure-services)
  - [2. Create Repository](#2-create-repository)
  - [3. Use in Controllers/Services](#3-use-in-controllersservices)
- [Configuration](#configuration)
  - [Connection Strings](#connection-strings)
  - [Command Configuration](#command-configuration)
- [Multi-map Queries](#multi-map-queries)
- [Transaction Management](#transaction-management)
- [Connection String Requirements](#connection-string-requirements)
- [MySQL-Specific Features](#mysql-specific-features)
  - [Auto-Increment Handling](#auto-increment-handling)
  - [Stored Procedures](#stored-procedures)
  - [JSON Column Support](#json-column-support)
- [Performance Tips](#performance-tips)
- [Connection Pool Configuration](#connection-pool-configuration)
- [Related Packages](#related-packages)
- [License](#license)
- [Credits](#credits)

## Overview

`Syrx.MySql` provides MySQL database support for Syrx applications. This package enables you to use Syrx's powerful data access patterns with MySQL databases, leveraging the performance and flexibility of Dapper underneath with MySqlConnector.

## Features

- **MySQL Integration**: Native support for MySQL databases
- **High Performance**: Built on top of Dapper and MySqlConnector for optimal performance
- **Transaction Support**: Full transaction management with automatic rollback
- **Multi-map Queries**: Complex object composition with up to 16 input parameters
- **Async Operations**: Full async/await support for all operations
- **Connection Pooling**: Efficient connection management

## Installation

> **Recommended**: Install the Extensions package for easier configuration and setup.

```bash
dotnet add package Syrx.MySql.Extensions
```

**Core Package Only**
```bash
dotnet add package Syrx.MySql
```

**Package Manager**
```bash
Install-Package Syrx.MySql.Extensions
Install-Package Syrx.MySql
```

**PackageReference**
```xml
<PackageReference Include="Syrx.MySql.Extensions" Version="2.4.5" />
<PackageReference Include="Syrx.MySql" Version="2.4.5" />
```

## Quick Start

### 1. Configure Services

```csharp
using Syrx.MySql.Extensions;

public void ConfigureServices(IServiceCollection services)
{
    services.UseSyrx(builder => builder
        .UseMySql(mysql => mysql
            .AddConnectionString("DefaultConnection", connectionString)
            .AddCommand(types => types
                .ForType<UserRepository>(methods => methods
                    .ForMethod(nameof(UserRepository.GetByIdAsync), command => command
                        .UseConnectionAlias("DefaultConnection")
                        .UseCommandText("SELECT * FROM Users WHERE Id = @id"))))));
}
```

### 2. Create Repository

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

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _commander.QueryAsync<User>();
    }

    public async Task<User> CreateAsync(User user)
    {
        return await _commander.ExecuteAsync(user) ? user : default;
    }
}
```

### 3. Use in Controllers/Services

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserRepository _userRepository;

    public UsersController(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }
}
```

## Configuration

### Connection Strings

```csharp
.UseMySql(mysql => mysql
    .AddConnectionString("Primary", "Server=localhost;Database=mydb;Uid=myuser;Pwd=mypass;")
    .AddConnectionString("ReadOnly", "Server=readonly;Database=mydb;Uid=reader;Pwd=readpass;")
)
```

### Command Configuration

```csharp
.AddCommand(types => types
    .ForType<UserRepository>(methods => methods
        .ForMethod("GetActiveUsers", command => command
            .UseConnectionAlias("Primary")
            .UseCommandText("SELECT * FROM Users WHERE IsActive = 1")
            .SetCommandTimeout(30))
        .ForMethod("GetUserStats", command => command
            .UseConnectionAlias("ReadOnly")
            .UseCommandText("CALL GetUserStatistics()")
            .SetCommandType(CommandType.StoredProcedure))))
```

## Multi-map Queries

For complex object composition:

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

## Transaction Management

Execute operations are automatically wrapped in transactions:

```csharp
public async Task<User> UpdateUserAsync(User user)
{
    // Automatically handles transaction with rollback on exceptions
    return await _commander.ExecuteAsync(user) ? user : default;
}
```

## Connection String Requirements

MySQL connection strings should include:
- Server/Host
- Database/Initial Catalog
- User ID (Uid) and Password (Pwd)

Example connection strings:

### Basic Authentication
```
Server=localhost;Database=mydatabase;Uid=myuser;Pwd=mypassword;
```

### With Port
```
Server=localhost;Port=3306;Database=mydatabase;Uid=myuser;Pwd=mypassword;
```

### SSL Connection
```
Server=localhost;Database=mydatabase;Uid=myuser;Pwd=mypassword;SslMode=Required;
```

### Cloud MySQL (e.g., AWS RDS)
```
Server=myinstance.region.rds.amazonaws.com;Database=mydatabase;Uid=admin;Pwd=password;SslMode=Required;
```

## MySQL-Specific Features

### Auto-Increment Handling
```csharp
public async Task<int> CreateUserAsync(User user)
{
    var result = await _commander.QueryAsync<int>(user);
    return result.FirstOrDefault(); // Returns LAST_INSERT_ID()
}
```

### Stored Procedures
```csharp
.ForMethod("CallProcedure", command => command
    .UseConnectionAlias("Primary")
    .UseCommandText("CALL sp_GetUserData(@userId)")
    .SetCommandType(CommandType.StoredProcedure))
```

### JSON Column Support
```csharp
public async Task<User> GetUserWithJsonDataAsync(int id)
{
    return await _commander.QueryAsync<User>(new { id });
    // Command text: SELECT Id, Name, JSON_EXTRACT(JsonData, '$.property') AS Property FROM Users WHERE Id = @id
}
```

## Performance Tips

- Use parameterized queries for security and performance
- Consider connection pooling settings in connection string
- Optimize command timeout values based on query complexity
- Use async methods for I/O-bound operations
- Use `MySqlConnector` provider for better performance than `MySql.Data`

## Connection Pool Configuration

```
Server=localhost;Database=mydb;Uid=user;Pwd=pass;
MinimumPoolSize=5;MaximumPoolSize=100;ConnectionTimeout=30;
```

## Related Packages

- **[Syrx.MySql.Extensions](https://www.nuget.org/packages/Syrx.MySql.Extensions/)**: Recommended extensions for easier setup
- **[Syrx](https://www.nuget.org/packages/Syrx/)**: Core Syrx interfaces
- **[Syrx.Commanders.Databases](https://www.nuget.org/packages/Syrx.Commanders.Databases/)**: Database command abstractions

## License

This project is licensed under the [MIT License](https://github.com/Syrx/Syrx/blob/main/LICENSE).

## Credits

- Built on top of [Dapper](https://github.com/DapperLib/Dapper)
- MySQL support provided by [MySqlConnector](https://github.com/mysql-net/MySqlConnector)
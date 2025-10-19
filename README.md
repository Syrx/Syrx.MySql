# Syrx.MySql

This project provides Syrx support for MySql. The overall experience of using [Syrx](https://github.com/Syrx/Syrx) remains the same. The only difference should be during dependency registration. 

## Table of Contents

- [Installation](#installation)
- [Extensions](#extensions)
- [Credits](#credits) 


## Installation 
> [!TIP]
> We recommend installing the Extensions package which includes extension methods for easier configuration. 

|Source|Command|
|--|--|
|.NET CLI|```dotnet add package Syrx.MySql.Extensions```
|Package Manager|```Install-Package Syrx.MySql.Extensions```
|Package Reference|```<PackageReference Include="Syrx.MySql.Extensions" Version="3.0.0" />```|
|Paket CLI|```paket add Syrx.MySql.Extensions --version 3.0.0```|

However, if you don't need the configuration options, you can install the standalone package via [nuget](https://www.nuget.org/packages/Syrx.MySql/).  

|Source|Command|
|--|--|
|.NET CLI|```dotnet add package Syrx.MySql```
|Package Manager|```Install-Package Syrx.MySql```
|Package Reference|```<PackageReference Include="Syrx.MySql" Version="3.0.0" />```|
|Paket CLI|```paket add Syrx.MySql --version 3.0.0```|
## Extensions
The `Syrx.MySql.Extensions` package provides dependency injection support via extension methods. 

```csharp
// add a using statement to the top of the file or in a global usings file.
using Syrx.Commanders.Databases.Connectors.MySql.Extensions;

public static IServiceCollection Install(this IServiceCollection services)
{
    return services
        .UseSyrx(factory => factory         // inject Syrx
        .UseMySql(builder => builder        // using the MySql implementation
            .AddConnectionString(/*...*/)   // add/resolve connection string details 
            .AddCommand(/*...*/)            // add/resolve commands for each type/method
            )
        );
}
```

## Credits
Syrx is inspired by and build on top of [Dapper](https://github.com/DapperLib/Dapper).    
MySql support is provided by [MySqlConnector](https://github.com/mysql-net/MySqlConnector).

using MySqlConnector;
using Syrx.Commanders.Databases.Settings;

namespace Syrx.Commanders.Databases.Connectors.MySql
{
    /// <summary>
    /// MySQL database connector implementation for the Syrx data access framework.
    /// This class provides MySQL-specific database connection functionality using MySqlConnector.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The MySqlDatabaseConnector inherits from <see cref="DatabaseConnector"/> and provides 
    /// MySQL-specific connection creation using the MySqlConnectorFactory. This implementation
    /// leverages the high-performance MySqlConnector library for optimal database connectivity.
    /// </para>
    /// <para>
    /// This connector supports all standard MySQL connection string parameters including:
    /// server, port, database, user credentials, SSL settings, connection pooling, and
    /// MySQL-specific options like character sets and time zone handling.
    /// </para>
    /// <para>
    /// The connector is fully thread-safe and designed for use in multi-threaded applications.
    /// Connection instances are created on-demand and should be properly disposed by the caller.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Connection string configuration
    /// var connectionString = "Server=localhost;Database=mydb;Uid=user;Pwd=pass;";
    /// 
    /// // The connector is typically registered via dependency injection
    /// services.UseSyrx(builder => builder
    ///     .UseMySql(mysql => mysql
    ///         .AddConnectionString("Default", connectionString)));
    /// </code>
    /// </example>
    public class MySqlDatabaseConnector(ICommanderSettings settings) : DatabaseConnector(settings, () => MySqlConnectorFactory.Instance)
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlDatabaseConnector"/> class.
        /// </summary>
        /// <param name="settings">The commander settings containing connection string aliases and command configurations.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> is null.</exception>
        /// <remarks>
        /// The constructor passes the MySqlConnectorFactory.Instance to the base DatabaseConnector,
        /// which enables the creation of MySQL-specific database connections. The settings parameter
        /// provides access to named connection strings and their aliases for connection resolution.
        /// </remarks>
    }
}

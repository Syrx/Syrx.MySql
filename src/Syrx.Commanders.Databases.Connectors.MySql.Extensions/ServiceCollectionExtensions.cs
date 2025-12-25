namespace Syrx.Commanders.Databases.Connectors.MySql.Extensions
{
    /// <summary>
    /// Extension methods for registering MySQL database connector services with dependency injection containers.
    /// </summary>
    /// <remarks>
    /// This class provides internal extension methods used by the Syrx framework to register
    /// MySQL-specific database connector implementations with the service collection. These methods
    /// are typically called indirectly through higher-level configuration methods.
    /// </remarks>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the MySQL database connector implementation with the service collection.
        /// </summary>
        /// <param name="services">The service collection to register the connector with.</param>
        /// <param name="lifetime">
        /// The service lifetime for the connector registration. Defaults to <see cref="ServiceLifetime.Transient"/>.
        /// </param>
        /// <returns>The service collection for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
        /// <remarks>
        /// <para>
        /// This method registers <see cref="MySqlDatabaseConnector"/> as the implementation for
        /// <see cref="IDatabaseConnector"/> using the TryAdd pattern to avoid duplicate registrations.
        /// </para>
        /// <para>
        /// The method is marked as internal because it's designed to be called by the framework's
        /// configuration methods rather than directly by application code. Use the higher-level
        /// <see cref="MySqlConnectorExtensions.UseMySql"/> method for typical configuration scenarios.
        /// </para>
        /// <para>
        /// Service lifetime considerations:
        /// </para>
        /// <list type="bullet">
        /// <item><description><strong>Transient:</strong> New instance per injection (default, safe for all scenarios)</description></item>
        /// <item><description><strong>Scoped:</strong> One instance per scope/request (suitable for web applications)</description></item>
        /// <item><description><strong>Singleton:</strong> Single instance for application lifetime (best performance, requires thread-safe usage)</description></item>
        /// </list>
        /// </remarks>
        internal static IServiceCollection AddMySql(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return services.TryAddToServiceCollection(
                typeof(IDatabaseConnector),
                typeof(MySqlDatabaseConnector),
                lifetime);
        }
    }
}

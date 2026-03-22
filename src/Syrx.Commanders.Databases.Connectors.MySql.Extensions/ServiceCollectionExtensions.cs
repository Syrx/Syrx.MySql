namespace Syrx.Commanders.Databases.Connectors.MySql.Extensions
{
    /// <summary>
    /// Contains service registration helpers for the MySQL connector implementation.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        internal static IServiceCollection AddMySql(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return services.TryAddToServiceCollection(
                typeof(IDatabaseConnector),
                typeof(MySqlDatabaseConnector),
                lifetime);
        }
    }
}

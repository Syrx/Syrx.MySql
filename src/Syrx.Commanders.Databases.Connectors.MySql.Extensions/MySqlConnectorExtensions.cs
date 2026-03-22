namespace Syrx.Commanders.Databases.Connectors.MySql.Extensions
{
    /// <summary>
    /// Adds MySQL-specific registration helpers to the Syrx builder pipeline.
    /// </summary>
    public static class MySqlConnectorExtensions
    {
        /// <summary>
        /// Registers Syrx MySQL settings, readers, connectors, and database commander services.
        /// </summary>
        /// <param name="builder">The Syrx builder being configured.</param>
        /// <param name="factory">Builds the command and connection settings for the MySQL-backed commander.</param>
        /// <param name="lifetime">The service lifetime applied to the registered services.</param>
        /// <returns>The same <see cref="SyrxBuilder"/> instance for fluent chaining.</returns>
        public static SyrxBuilder UseMySql(
            this SyrxBuilder builder,
            Action<CommanderSettingsBuilder> factory,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            var options = CommanderSettingsBuilderExtensions.Build(factory);
            builder.ServiceCollection
                .AddSingleton<ICommanderSettings, CommanderSettings>(a => options)
                .AddReader(lifetime) // add reader
                .AddMySql(lifetime) // add connector
                .AddDatabaseCommander(lifetime);

            return builder;
        }
    }
}

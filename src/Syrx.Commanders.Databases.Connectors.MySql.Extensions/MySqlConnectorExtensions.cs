namespace Syrx.Commanders.Databases.Connectors.MySql.Extensions
{
    /// <summary>
    /// Extension methods for configuring MySQL database connectors in the Syrx framework.
    /// </summary>
    /// <remarks>
    /// This class provides fluent extension methods that simplify the configuration and
    /// registration of MySQL database connectors with dependency injection containers.
    /// The extensions integrate seamlessly with the Syrx builder pattern to provide
    /// a consistent configuration experience.
    /// </remarks>
    public static class MySqlConnectorExtensions
    {
        /// <summary>
        /// Configures the Syrx builder to use MySQL database connectivity with the specified settings.
        /// </summary>
        /// <param name="builder">The Syrx builder instance to configure.</param>
        /// <param name="factory">
        /// An action that configures the <see cref="CommanderSettingsBuilder"/> with connection strings,
        /// command mappings, and other MySQL-specific settings.
        /// </param>
        /// <param name="lifetime">
        /// The service lifetime for registered services. Defaults to <see cref="ServiceLifetime.Singleton"/>.
        /// </param>
        /// <returns>The configured <see cref="SyrxBuilder"/> instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="builder"/> or <paramref name="factory"/> is null.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method performs the following service registrations:
        /// </para>
        /// <list type="bullet">
        /// <item><description>Registers <see cref="ICommanderSettings"/> as a singleton with the built configuration</description></item>
        /// <item><description>Registers the database command reader with the specified lifetime</description></item>
        /// <item><description>Registers the MySQL database connector with the specified lifetime</description></item>
        /// <item><description>Registers the database commander with the specified lifetime</description></item>
        /// </list>
        /// <para>
        /// The factory action allows for fluent configuration of connection strings and command mappings
        /// specific to your application's repository methods and database operations.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// services.UseSyrx(builder => builder
        ///     .UseMySql(mysql => mysql
        ///         .AddConnectionString("Primary", "Server=localhost;Database=MyApp;Uid=user;Pwd=pass;")
        ///         .AddCommand(types => types
        ///             .ForType&lt;UserRepository&gt;(methods => methods
        ///                 .ForMethod("GetByIdAsync", cmd => cmd
        ///                     .UseConnectionAlias("Primary")
        ///                     .UseCommandText("SELECT * FROM Users WHERE Id = @id")))),
        ///     ServiceLifetime.Scoped));
        /// </code>
        /// </example>
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

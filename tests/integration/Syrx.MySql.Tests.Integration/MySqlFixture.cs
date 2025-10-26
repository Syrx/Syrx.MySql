using DotNet.Testcontainers.Builders;

namespace Syrx.MySql.Tests.Integration
{
    public class MySqlFixture : Fixture, IAsyncLifetime
    {
        private readonly MySqlContainer _container;

        /// <summary>
        /// Initializes MySQL test fixture with robust container readiness detection.
        /// Uses combined TCP port check + mysqladmin ping to prevent "container is not running" errors.
        /// </summary>
        public MySqlFixture()
        {
            var _logger = LoggerFactory.Create(b => b
                .AddConsole()
                .AddSystemdConsole()
                .AddSimpleConsole()).CreateLogger<MySqlFixture>();

            _container = new MySqlBuilder()
                .WithImage("docker-syrx-mysql-test:latest")
                .WithDatabase("syrx")
                .WithUsername("syrx_user")
                .WithPassword("YourStrong!Passw0rd")
                .WithPortBinding(3306, true)
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilInternalTcpPortIsAvailable(3306))
                .WithLogger(_logger)
                .Build();
        }

        public async Task DisposeAsync()
        {
            await _container.DisposeAsync();
        }

        public async Task InitializeAsync()
        {
            // Start the container
            await _container.StartAsync();
            
            var connectionString = _container.GetConnectionString();
            // Add Allow User Variables to support MySQL user variables in stored procedures
            connectionString += ";Allow User Variables=true";
            var alias = "Syrx.MySql";

            Install(() => Installer.Install(alias, connectionString));
            Installer.SetupDatabase(base.ResolveCommander<DatabaseBuilder>());

            // set assertion messages for those that change between RDBMS implementations. 
            AssertionMessages.Add<Execute>(nameof(Execute.SupportsTransactionRollback), "DOUBLE value is out of range in 'pow(2147483647,2147483647)'");
            AssertionMessages.Add<Execute>(nameof(Execute.ExceptionsAreReturnedToCaller), "Division by zero error");
            AssertionMessages.Add<Execute>(nameof(Execute.SupportsRollbackOnParameterlessCalls), "Deliberate exception.");

            AssertionMessages.Add<ExecuteAsync>(nameof(ExecuteAsync.SupportsTransactionRollback), "DOUBLE value is out of range in 'pow(2147483647,2147483647)'");
            AssertionMessages.Add<ExecuteAsync>(nameof(ExecuteAsync.ExceptionsAreReturnedToCaller), "Division by zero error");
            AssertionMessages.Add<ExecuteAsync>(nameof(ExecuteAsync.SupportsRollbackOnParameterlessCalls), "Deliberate exception.");

            AssertionMessages.Add<Query>(nameof(Query.ExceptionsAreReturnedToCaller), "Division by zero error");
            AssertionMessages.Add<QueryAsync>(nameof(QueryAsync.ExceptionsAreReturnedToCaller), "Division by zero error");


            await Task.CompletedTask;
        }

    }
}

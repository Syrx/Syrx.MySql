using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MySqlConnector;
using Dapper;

namespace Syrx.MySql.Tests.Integration
{
    public class MySqlFixture : Fixture, IAsyncLifetime
    {
        private readonly IContainer _container;
        private string _connectionString;

        /// <summary>
        /// Initializes MySQL test fixture with robust container readiness detection.
        /// Uses raw container approach to avoid MySqlBuilder complexities in CI.
        /// </summary>
        public MySqlFixture()
        {
            var logger = LoggerFactory.Create(b => b
                .AddConsole()
                .AddSystemdConsole()
                .AddSimpleConsole()).CreateLogger<MySqlFixture>();

            _container = new ContainerBuilder()
                .WithImage("docker-syrx-mysql-test:latest")
                .WithEnvironment("MYSQL_DATABASE", "syrx")
                .WithEnvironment("MYSQL_USER", "syrx_user")
                .WithEnvironment("MYSQL_PASSWORD", "YourStrong!Passw0rd")
                .WithEnvironment("MYSQL_ROOT_PASSWORD", "YourStrong!Passw0rd")
                .WithPortBinding(3306, true)
                .WithLogger(logger)
                .Build();
        }

        public async Task DisposeAsync()
        {
            await _container.DisposeAsync();
        }

        public async Task InitializeAsync()
        {
            // Start the container
            Console.WriteLine("Starting MySQL container...");
            await _container.StartAsync();
            Console.WriteLine($"Container started. ID: {_container.Id}, State: {_container.State}");
            
            // Build connection string manually
            var port = _container.GetMappedPublicPort(3306);
            _connectionString = $"Server=127.0.0.1;Port={port};Database=syrx;Uid=syrx_user;Pwd=YourStrong!Passw0rd;Allow User Variables=true";
            Console.WriteLine($"Connection string: {_connectionString}");
            
            // Wait for MySQL to be fully ready with connection verification
            Console.WriteLine("Starting connection verification...");
            await WaitForMySqlReadyAsync();
            Console.WriteLine("MySQL is ready!");
            
            var alias = "Syrx.MySql";

            Install(() => Installer.Install(alias, _connectionString));
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

        private async Task WaitForMySqlReadyAsync()
        {
            // Start with a longer initial delay for container startup
            await Task.Delay(TimeSpan.FromSeconds(15));
            
            var maxAttempts = 120; // 10 minutes total
            var delayBetweenAttempts = TimeSpan.FromSeconds(5);
            
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    Console.WriteLine($"Connection attempt {attempt}/{maxAttempts}...");
                    using var connection = new MySqlConnection(_connectionString);
                    await connection.OpenAsync();
                    await connection.ExecuteScalarAsync("SELECT 1");
                    Console.WriteLine($"Connection successful on attempt {attempt}");
                    return; // Connection successful
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    Console.WriteLine($"Connection attempt {attempt} failed: {ex.Message}");
                    await Task.Delay(delayBetweenAttempts);
                }
            }
            
            throw new InvalidOperationException("MySQL container did not become ready within the expected time.");
        }

    }
}

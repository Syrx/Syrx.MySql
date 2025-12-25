using MySqlConnector;
using Dapper;

namespace Syrx.MySql.Tests.Integration
{
    public class MySqlFixture : Fixture, IAsyncLifetime
    {
        private string _connectionString;
        private readonly bool _useWorkflowManagedMySQL;

        /// <summary>
        /// Initializes MySQL test fixture.
        /// Uses workflow-managed MySQL service in CI, falls back to local MySQL for development.
        /// </summary>
        public MySqlFixture()
        {
            // Check if we're running in GitHub Actions with a managed MySQL service
            _useWorkflowManagedMySQL = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"));
            
            if (_useWorkflowManagedMySQL)
            {
                // Use the workflow-managed MySQL service
                _connectionString = "Server=127.0.0.1;Port=3306;Database=syrx;Uid=syrx_user;Pwd=YourStrong!Passw0rd;Allow User Variables=true";
                Console.WriteLine("Using workflow-managed MySQL service");
            }
            else
            {
                // For local development, you'll need a local MySQL instance
                _connectionString = "Server=localhost;Port=3306;Database=syrx;Uid=syrx_user;Pwd=YourStrong!Passw0rd;Allow User Variables=true";
                Console.WriteLine("Using local MySQL instance for development");
            }
        }

        public async Task DisposeAsync()
        {
            // Nothing to dispose - workflow manages the MySQL service
            await Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
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
            // Shorter delay since workflow handles MySQL readiness
            if (_useWorkflowManagedMySQL)
            {
                await Task.Delay(TimeSpan.FromSeconds(5)); // Workflow should have MySQL ready
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(15)); // Local dev might need more time
            }
            
            var maxAttempts = _useWorkflowManagedMySQL ? 30 : 120; // Less attempts needed for workflow-managed
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

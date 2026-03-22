´╗┐using DotNet.Testcontainers.Builders;

namespace Syrx.MySql.Tests.Integration
{
    public class MySqlFixture : Fixture, IAsyncLifetime
    {
        private const string MySqlImage = "mysql:8.0";
        private static readonly TimeSpan ContainerStartupTimeout = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan DatabaseReadyTimeout = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan DatabaseReadyPollInterval = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan DatabaseConnectionTimeout = TimeSpan.FromSeconds(2);
        private static readonly bool EnableContainerReuse =
            string.Equals(Environment.GetEnvironmentVariable("SYRX_MYSQL_TESTCONTAINER_REUSE"), "true", StringComparison.OrdinalIgnoreCase);

        private readonly MySqlContainer _container;

        public MySqlFixture()
        {
            var _logger = LoggerFactory.Create(b => b
                .AddConsole()
                .AddSystemdConsole()
                .AddSimpleConsole()).CreateLogger<MySqlFixture>();

            _container = new MySqlBuilder(MySqlImage)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilExternalTcpPortIsAvailable(MySqlBuilder.MySqlPort))
            .WithReuse(EnableContainerReuse)
    .WithLogger(_logger)
    .WithStartupCallback((container, token) =>
    {
        var message = @$"{new string('=', 150)}
Syrx: {nameof(MySqlContainer)} startup callback. Container details:
{new string('=', 150)}
Name ............. : {container.Name}
Id ............... : {container.Id}
State ............ : {container.State}
Health ........... : {container.Health}
CreatedTime ...... : {container.CreatedTime}
StartedTime ...... : {container.StartedTime}
Hostname ......... : {container.Hostname}
Image.Digest ..... : {container.Image.Digest}
Image.FullName ... : {container.Image.FullName}
Image.Registry ... : {container.Image.Registry}
Image.Repository . : {container.Image.Repository}
Image.Tag ........ : {container.Image.Tag}
IpAddress ........ : {container.IpAddress}
MacAddress ....... : {container.MacAddress}
{new string('=', 150)}
";
        container.Logger.LogInformation(message);
        return Task.CompletedTask;
    }).Build();
        }

        public async Task DisposeAsync()
        {
            await _container.DisposeAsync();
        }

        public async Task InitializeAsync()
        {
            using var startupTokenSource = new CancellationTokenSource(ContainerStartupTimeout);
            await _container.StartAsync(startupTokenSource.Token);

            // line up
            // Integration tests run against ephemeral local containers, so TLS is explicitly disabled here.
            // Do not copy this setting into production connection strings.
            var connectionString = $"{_container.GetConnectionString()};Allow User Variables=true;SslMode=None;Connection Timeout={(int)DatabaseConnectionTimeout.TotalSeconds}";
            var alias = "Syrx.Sql";

            await WaitForDatabaseReadyAsync(connectionString, CancellationToken.None);

            // call Install() on the base type. 
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
        }

        private static async Task WaitForDatabaseReadyAsync(string connectionString, CancellationToken cancellationToken)
        {
            var startedAt = DateTime.UtcNow;
            Exception lastError = null;

            while (DateTime.UtcNow - startedAt < DatabaseReadyTimeout)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await using var connection = new MySqlConnector.MySqlConnection(connectionString);
                    await connection.OpenAsync(cancellationToken);
                    await connection.CloseAsync();
                    return;
                }
                catch (Exception ex)
                {
                    lastError = ex;
                }

                await Task.Delay(DatabaseReadyPollInterval, cancellationToken);
            }

            throw new TimeoutException(
                $"MySQL container did not accept client connections within {DatabaseReadyTimeout}.",
                lastError);
        }

    }
}

using DotNet.Testcontainers.Builders;

namespace Syrx.MySql.Tests.Integration
{
    public class MySqlFixture : Fixture, IAsyncLifetime
    {
        private readonly MySqlContainer _container;

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
                    .UntilInternalTcpPortIsAvailable(3306)
                    .UntilCommandIsCompleted("/bin/sh", "-c", "until mysqladmin ping -h localhost --silent; do sleep 1; done"))
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
ConnectionString . : {container.GetConnectionString()}
{new string('=', 150)}
";
        container.Logger.LogInformation(message);
        return Task.CompletedTask;
    }).Build();


            // start
            _container.StartAsync().Wait();
        }

        public async Task DisposeAsync()
        {
            await _container.DisposeAsync();
        }

        public async Task InitializeAsync()
        {
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

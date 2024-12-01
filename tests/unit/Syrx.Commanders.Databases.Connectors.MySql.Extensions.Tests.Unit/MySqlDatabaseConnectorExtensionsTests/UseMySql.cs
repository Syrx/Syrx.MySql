namespace Syrx.Commanders.Databases.Connectors.MySql.Extensions.Tests.Unit.MySqlDatabaseConnectorExtensionsTests
{
    public class UseMySql
    {
        private IServiceCollection _services;

        public UseMySql()
        {
            _services = new ServiceCollection();
        }

        [Fact]
        public void Successful()
        {
            _services.UseSyrx(a => a
                .UseMySql(b => b
                    .AddCommand(c => c
                        .ForType<UseMySql>(d => d
                            .ForMethod(nameof(Successful), e => e.UseCommandText("test-command").UseConnectionAlias("test-aliase"))))));

            var provider = _services.BuildServiceProvider();
            var connector = provider.GetService<IDatabaseConnector>();
            IsType<MySqlDatabaseConnector>(connector);
        }
    }
}

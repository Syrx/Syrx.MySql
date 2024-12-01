namespace Syrx.MySql.Tests.Integration.DatabaseCommanderTests
{
    [Collection(nameof(MySqlFixtureCollection))]
    public class MySqlQueryAsync(MySqlFixture fixture) : QueryAsync(fixture) { }
}

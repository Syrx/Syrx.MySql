namespace Syrx.MySql.Tests.Integration.DatabaseCommanderTests
{
    [Collection(nameof(MySqlFixtureCollection))]
    public class MySqlQuery(MySqlFixture fixture) : Query(fixture) { }
}

namespace Syrx.MySql.Tests.Integration.DatabaseCommanderTests
{
    [Collection(nameof(MySqlFixtureCollection))]
    public class MySqlDispose(MySqlFixture fixture) : Dispose(fixture) { }
}

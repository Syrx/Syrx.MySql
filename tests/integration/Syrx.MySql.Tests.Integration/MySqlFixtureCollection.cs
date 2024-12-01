namespace Syrx.MySql.Tests.Integration
{
    [CollectionDefinition(nameof(MySqlFixtureCollection))]
    public class MySqlFixtureCollection : ICollectionFixture<MySqlFixture> { }
}

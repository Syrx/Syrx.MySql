using System.Transactions;

namespace Syrx.MySql.Tests.Integration.DatabaseCommanderTests
{
    [Collection(nameof(MySqlFixtureCollection))]
    public class MySqlExecuteAsync(MySqlFixture fixture) : ExecuteAsync(fixture) 
    {
        [Theory(Skip = "Not supported by MySQL")]
        [MemberData(nameof(TransactionScopeOptions))] // TransactionScopeOptions is taken from base Exeucte
        public override Task SupportsEnlistingInAmbientTransactions(TransactionScopeOption scopeOption)
        {
            return base.SupportsEnlistingInAmbientTransactions(scopeOption);
        }
    }
}

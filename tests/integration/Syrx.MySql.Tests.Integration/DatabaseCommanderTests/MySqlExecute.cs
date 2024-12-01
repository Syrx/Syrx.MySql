using System.Transactions;

namespace Syrx.MySql.Tests.Integration.DatabaseCommanderTests
{
    [Collection(nameof(MySqlFixtureCollection))]
    public class MySqlExecute(MySqlFixture fixture) : Execute(fixture) 
    {
        
        [Theory(Skip ="Not supported by MySQL")]
        [MemberData(nameof(TransactionScopeOptions))] // TransactionScopeOptions is taken from base Exeucte
        public override void SupportsEnlistingInAmbientTransactions(TransactionScopeOption scopeOption)
        {
            base.SupportsEnlistingInAmbientTransactions(scopeOption);
        }

    }
}

using MySqlConnector;
using Syrx.Commanders.Databases.Settings;

namespace Syrx.Commanders.Databases.Connectors.MySql
{
    /// <summary>
    /// Creates MySQL database connections for Syrx command execution.
    /// </summary>
    /// <param name="settings">The Syrx command and connection settings used to resolve MySQL connections.</param>
    public class MySqlDatabaseConnector(ICommanderSettings settings) : DatabaseConnector(settings, () => MySqlConnectorFactory.Instance)
    {
    }
}

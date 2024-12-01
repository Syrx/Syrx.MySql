﻿namespace Syrx.MySql.Tests.Integration
{
    public class Installer
    {
        public static IServiceProvider Install(string alias, string connectionString)
        {
            return new ServiceCollection()
                .UseSyrx(factory => factory
                    .SetupMySql(connectionString))
                .BuildServiceProvider();
        }

        public static void SetupDatabase(ICommander<DatabaseBuilder> commander)
        {
            var builder = new DatabaseBuilder(commander);
            builder.Build();
        }
    }

    
}

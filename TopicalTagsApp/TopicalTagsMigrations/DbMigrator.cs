using DbUp;
using DbUp.Engine;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace TopicalTagsMigrations
{
    public class DbMigrator
    {
        public void Migrate(string appSettingsPath, string connectionStringName, string migrationsDirectory)
        {
            string connectionString = GetConnectionString(appSettingsPath, connectionStringName);
            DbUp.Engine.DatabaseUpgradeResult migration = MigrateWithEngine(migrationsDirectory, connectionString);
            Console.WriteLine($"Database Migration Successful: {migration.Successful}");
        }

        protected virtual DatabaseUpgradeResult MigrateWithEngine(string migrationsDirectory, string connectionString)
        {
            var engine = DeployChanges.To
                            .SqlDatabase(connectionString)
                            .WithScriptsFromFileSystem(migrationsDirectory)
                            .LogToConsole()
                            .Build();

            var migration = engine.PerformUpgrade();
            return migration;
        }

        protected virtual string GetConnectionString(string appSettingsPath, string connectionStringName)
        {
            var config = JValue.Parse(File.ReadAllText(appSettingsPath));
            string connectionString = config["ConnectionStrings"][connectionStringName].ToString();
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"The name of the connection string {connectionStringName} is not found in {appSettingsPath} > ConnectionStrings section", 
                    nameof(connectionStringName));
            }
            return connectionString;
        }
    }
}

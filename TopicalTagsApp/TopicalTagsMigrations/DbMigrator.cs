using DbUp;
using DbUp.Engine;
using DbUp.Helpers;
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
            DbUp.Engine.DatabaseUpgradeResult executionResult = MigrateWithEngine(migrationsDirectory, connectionString);
            Console.WriteLine($"Database Migration Result: {executionResult.Successful}");

            executionResult = ExecuteAlways(migrationsDirectory, connectionString);
            Console.WriteLine($"Database Always Executed Script Result: {executionResult.Successful}");
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

        protected virtual DatabaseUpgradeResult ExecuteAlways(string migrationsDirectory, string connectionString)
        {
            var engine = DeployChanges.To
                            .SqlDatabase(connectionString)
                            .WithScriptsFromFileSystem(Path.Combine(migrationsDirectory, "/Always/"))
                            .JournalTo(new NullJournal())
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

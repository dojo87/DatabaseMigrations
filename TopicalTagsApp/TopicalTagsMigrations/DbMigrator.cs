using DbUp;
using DbUp.Builder;
using DbUp.Engine;
using DbUp.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace TopicalTagsMigrations
{
    public class DbMigrator
    {
        private const string ConventionPathForScriptsRunAlways = "Always";

        public void Migrate(string appSettingsPath, string connectionStringName, string migrationsDirectory, string configuration = "Debug", string configurationDefault = "Default")
        {
            string connectionString = GetConnectionString(appSettingsPath, connectionStringName);
            DatabaseUpgradeResult executionResult = MigrateWithEngine(migrationsDirectory, connectionString);
            PrintResult("Migration", executionResult);

            executionResult = ExecuteAlways(migrationsDirectory, connectionString, configuration, configurationDefault);
            PrintResult("Data and Configuration Sync", executionResult);
        }

        private void PrintResult(string resultType, DatabaseUpgradeResult executionResult)
        {
            Console.WriteLine($"Database {resultType} Result: {(executionResult.Successful ? "Success" : "Error")}");
            if (!executionResult.Successful)
            {
                Console.WriteLine(executionResult.Error);
            }
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

        protected virtual DatabaseUpgradeResult ExecuteAlways(string migrationsDirectory, string connectionString, string configuration, string configurationDefault)
        {
            string scriptsPath = Path.Combine(migrationsDirectory, ConventionPathForScriptsRunAlways);
            string configurationSpecificScriptsPath = GetConfigurationSpecificScriptsPath(migrationsDirectory, configuration, configurationDefault);
            DatabaseUpgradeResult result = ExecuteAlways(connectionString, scriptsPath, configurationSpecificScriptsPath);
            return result;
        }

        protected virtual DatabaseUpgradeResult ExecuteAlways(string connectionString, string scriptsPath, string configurationSpecificScriptsPath)
        {
            DatabaseUpgradeResult result = new DatabaseUpgradeResult(new SqlScript[0], true, null);
            if (Directory.Exists(scriptsPath))
            {
                UpgradeEngineBuilder engineBuilder = DeployChanges.To
                              .SqlDatabase(connectionString)
                              .WithScriptsFromFileSystem(scriptsPath);

                if (configurationSpecificScriptsPath != null)
                {
                    engineBuilder = engineBuilder.WithScriptsFromFileSystem(configurationSpecificScriptsPath);
                }

                var engine = engineBuilder.JournalTo(new NullJournal())
                                .LogToConsole()
                                .Build();

                result = engine.PerformUpgrade();
            }

            return result;
        }

        private static string GetConfigurationSpecificScriptsPath(string migrationsDirectory, string configuration, string configurationDefault)
        {
            string configurationSpecificScriptsPath = null;

            string[] possibleConfigurations = new string[] { configuration, configurationDefault };

            foreach (string possibleConfiguration in possibleConfigurations)
            {
                if (possibleConfiguration != null)
                {
                    configurationSpecificScriptsPath = Path.Combine(migrationsDirectory, ConventionPathForScriptsRunAlways, possibleConfiguration);
                    if (!Directory.Exists(configurationSpecificScriptsPath))
                    {
                        configurationSpecificScriptsPath = null;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return configurationSpecificScriptsPath;
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

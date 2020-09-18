using DbUp;
using DbUp.Builder;
using DbUp.Engine;
using DbUp.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TopicalTags.DatabaseFirst.Migrations
{
    public class DbMigrator
    {
        public enum AlwaysScriptType
        {
            AlwaysBefore,
            AlwaysAfter
        }

        public void Migrate(string appSettingsPath, 
            string connectionStringName, 
            string migrationsDirectory, 
            string configuration = "Debug", 
            string configurationDefault = "Default",
            string variables=null)
        {
            var variableDictionary = ParseVariables(variables);
            string connectionString = GetConnectionString(appSettingsPath, connectionStringName);

            RecreateIfRequired(variableDictionary, connectionString);

            DatabaseUpgradeResult executionResult = ExecuteAlways(AlwaysScriptType.AlwaysBefore, migrationsDirectory, connectionString, configuration, configurationDefault, variableDictionary);
            PrintResult("Pre Deployment Scripts", executionResult);

            executionResult = MigrateWithEngine(migrationsDirectory, connectionString, variableDictionary);
            PrintResult("Migration", executionResult);

            executionResult = ExecuteAlways(AlwaysScriptType.AlwaysAfter, migrationsDirectory, connectionString, configuration, configurationDefault, variableDictionary);
            PrintResult("Data and Configuration Sync", executionResult);
        }

        private void RecreateIfRequired(Dictionary<string, string> variableDictionary, string connectionString)
        {
            string recreateBool = "false";
            if (variableDictionary.TryGetValue("RecreateDatabase", out recreateBool) && recreateBool.ToLowerInvariant().Equals("true"))
            {
                Console.WriteLine("Dropping database");
                DropDatabase.For.SqlDatabase(connectionString);
            }
        }

        private Dictionary<string, string> ParseVariables(string variablesString)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (variablesString != null)
            {
                result = variablesString.Split("&", StringSplitOptions.RemoveEmptyEntries)
                    .ToDictionary(k => k.Split('=')[0], v => v.Split('=')[1]);
            }
            return result;
        }

        private void PrintResult(string resultType, DatabaseUpgradeResult executionResult)
        {
            Console.WriteLine($"Database {resultType} Result: {(executionResult.Successful ? "Success" : "Error")}");
            if (!executionResult.Successful)
            {
                Console.WriteLine(executionResult.Error);
            }
        }

        protected virtual DatabaseUpgradeResult MigrateWithEngine(string migrationsDirectory, string connectionString, IDictionary<string,string> variables)
        {
            var engine = DeployChanges.To
                            .SqlDatabase(connectionString)
                            .WithScriptsFromFileSystem(migrationsDirectory)
                            .LogToConsole()
                            .WithVariables(variables)
                            .Build();

            var migration = engine.PerformUpgrade();
            return migration;
        }

        protected virtual DatabaseUpgradeResult ExecuteAlways(AlwaysScriptType alwaysScriptType, string migrationsDirectory, string connectionString, string configuration, string configurationDefault, IDictionary<string,string> variables)
        {
            EnsureDatabase.For.SqlDatabase(connectionString);
            
            string scriptsPath = Path.Combine(migrationsDirectory, alwaysScriptType.ToString());
            string configurationSpecificScriptsPath = GetConfigurationSpecificScriptsPath(scriptsPath, configuration, configurationDefault);
            DatabaseUpgradeResult result = ExecuteAlways(connectionString, scriptsPath, configurationSpecificScriptsPath, variables);
            return result;
        }

        protected virtual DatabaseUpgradeResult ExecuteAlways(string connectionString, string scriptsPath, string configurationSpecificScriptsPath, IDictionary<string, string> variables)
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
                                .WithVariables(variables)
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
                    configurationSpecificScriptsPath = Path.Combine(migrationsDirectory, possibleConfiguration);
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

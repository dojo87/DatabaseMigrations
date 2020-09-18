using DbUp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace TopicalTags.DatabaseFirst.Migrations
{
    class Program
    {
        static void Main(string[] args)
        {
            ValidateArguments(args);

            DbMigrator migrator = new DbMigrator();

            migrator.Migrate(appSettingsPath: GetArgument(args, 0), 
                connectionStringName: GetArgument(args, 1), 
                migrationsDirectory: GetArgument(args, 2), 
                configuration: GetArgument(args, 3),
                variables: GetArgument(args, 4),
                configurationDefault: "Default");
        }

        private static string GetArgument(string[] args, int index) => args.Length > index ? args[index] : null;

        private static void ValidateArguments(string[] args)
        {
            if (args.Length < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(args),
                    "usage: TopicalTagsMigrations.exe 'Path/To/appsettings.json' 'ConnectionStringName' 'Migration/scripts/directory' <optional>'configuration' <optional>'variable1=value&variable2=value'");
            }
            if (!File.Exists(args[0]))
            {
                throw new FileNotFoundException($"The app settings file in first argument does not exist: {args[0]}");
            }
            if (!Directory.Exists(args[2]))
            {
                throw new DirectoryNotFoundException($"The scripts directory in second argument does not exist: {args[1]}");
            }
        }
    }
}

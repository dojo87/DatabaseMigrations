using DbUp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace TopicalTagsMigrations
{
    class Program
    {
        static void Main(string[] args)
        {
            ValidateArguments(args);

            DbMigrator migrator = new DbMigrator();

            migrator.Migrate(appSettingsPath: args[0], connectionStringName:args[1], migrationsDirectory:args[2]);
        }

        private static void ValidateArguments(string[] args)
        {
            if (args.Length != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(args),
                    "usage: TopicalTagsMigrations.exe 'Path/To/appsettings.json' 'ConnectionStringName' 'Migration/scripts/directory'");
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

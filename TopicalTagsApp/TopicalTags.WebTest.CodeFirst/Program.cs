using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TopicalTags.CodeFirst;

namespace TopicalTags.WebTest.CodeFirst
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Any(a => a.Equals("--migration")))
            {
                using (DbMigrator migrator = DbMigrator.WithDefaultConfiguration()
                    .UsingConnectionStringName("MigrationConnectionString"))
                {
                    migrator.MigrateDatabase();
                    migrator.RunDataSeed();
                }
            }
            else
            {
                CreateWebHostBuilder(args).Build().Run();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}

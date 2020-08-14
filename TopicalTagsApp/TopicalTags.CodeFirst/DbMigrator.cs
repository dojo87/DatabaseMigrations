using System;
using System.Collections.Generic;
using System.Text;
using TopicalTags.CodeFirst.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace TopicalTags.CodeFirst
{
    public class DbMigrator : IDisposable
    {
        /// <summary>
        /// Getting DbMigrator using the default appsettings.json config file + environment.
        /// </summary>
        /// <returns></returns>
        public static DbMigrator WithDefaultConfiguration() 
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Console.WriteLine($"Using default appsettings.json configuration file along with environmental configs. Environment: {environmentName ?? "Default"}");

            IConfiguration config = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", true, true)
              .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
              .Build();

            return new DbMigrator(config);
        }

        private const string DefaultConnectionStringName = "DefaultDatabase";

        private TopicContext Context { get; set; }

        private IConfiguration Configuration { get; set; }

        public DbMigrator(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void MigrateDatabase()
        {
            Console.WriteLine("Migrating database started");
            EnsureContextInitialized();
            this.Context.Database.Migrate();
        }

        public void RunDataSeed()
        {
            Console.WriteLine("Data Seed");
            bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            Console.WriteLine($"Env:{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
            EnsureContextInitialized();
            Context.OnSeed(isDevelopment);
        }

        private void EnsureContextInitialized()
        {
            if (Context == null)
            {
                Console.WriteLine("Initializing Context");
                UsingConnectionStringName(DefaultConnectionStringName);
            }
        }

        public DbMigrator UsingConnectionStringName(string connectionStringName)
        {
            DbContextOptionsBuilder<TopicContext> options = new DbContextOptionsBuilder<TopicContext>();
            options
                .UseSqlServer(this.Configuration.GetConnectionString(connectionStringName));

            Context = new TopicContext(options.Options);

            return this;
        }

        public void Dispose()
        {
            if (Context != null)
            {
                Context.Dispose();
            }
        }
    }
}

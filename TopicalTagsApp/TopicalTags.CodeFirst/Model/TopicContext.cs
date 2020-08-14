using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TopicalTags.CodeFirst.Model
{
    public partial class TopicContext : DbContext
    {
        public TopicContext() : base()
        {
            
        }

        /// <summary>
        /// This constructor will be used with Service Provider
        /// </summary>
        /// <param name="options"></param>
        public TopicContext(DbContextOptions<TopicContext> options) : base(options)
        {
           
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<TopicTags>(entity =>
            {
                entity.HasKey(e => new { e.TopicId, e.TagId });
            });
        }

        public DbSet<Topic> Topics { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }

    /// <summary>
    /// This class is required for the design time tools to work on the Class Library. 
    /// When in a ASP.NET project, this is automatically determined by the Service Provider, so need to have this there.
    /// Here thought we need to provide a source of our connection string - we will use App Settings.
    /// </summary>
    public class TopicContextFactory : IDesignTimeDbContextFactory<TopicContext>
    {
        public TopicContext CreateDbContext(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder();

            string connectionName = Environment.GetEnvironmentVariable("CONNECTION_NAME") ?? "DefaultDatabase";
            configurationBuilder.AddJsonFile("appsettings.json");
            IConfiguration configuration = configurationBuilder.Build();

            string connectionString = configuration.GetConnectionString(connectionName);
            var optionsBuilder = new DbContextOptionsBuilder<TopicContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new TopicContext(optionsBuilder.Options);
        }
    }
}

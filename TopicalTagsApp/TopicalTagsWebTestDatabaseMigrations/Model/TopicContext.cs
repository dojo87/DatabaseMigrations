using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TopicalTagsWebTestDatabaseMigrations.Model
{
    public partial class TopicContext : DbContext
    {
        public TopicContext()
        {
        }

        public TopicContext(DbContextOptions<TopicContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Configuration> Configuration { get; set; }
        public virtual DbSet<Tag> Tag { get; set; }
        public virtual DbSet<Topic> Topic { get; set; }
        public virtual DbSet<TopicTags> TopicTags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("name=DatabaseConnectionString");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Configuration>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.Key)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Value).IsUnicode(false);
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Topic>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(2000)
                    .IsUnicode(false);

                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasMaxLength(2000)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TopicTags>(entity =>
            {
                entity.HasKey(e => new { e.TopicId, e.TagId });

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.TopicTags)
                    .HasForeignKey(d => d.TagId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TopicTag_ToTag");

                entity.HasOne(d => d.Topic)
                    .WithMany(p => p.TopicTags)
                    .HasForeignKey(d => d.TopicId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TopicTag_ToTopic");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

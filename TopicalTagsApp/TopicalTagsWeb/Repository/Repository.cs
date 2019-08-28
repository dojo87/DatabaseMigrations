using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TopicalTagsWeb
{
    public class Repository : DbContext
    {
        public Repository(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TopicTags>().HasKey(tt => new { tt.TagId, tt.TopicId });
        }

        public DbSet<Topic> Topics { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }

    public class Topic
    {
        public Topic()
        {
            this.TopicTags = new HashSet<TopicTags>();
        }

        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public virtual ICollection<TopicTags> TopicTags { get; set; }
    }

    public class Tag
    {
        public Tag()
        {
            this.TopicTags = new HashSet<TopicTags>();
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<TopicTags> TopicTags { get; set; }
    }

    public class TopicTags
    {
        public int TopicId { get; set; }
        public Topic Topic { get; set; }
        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}

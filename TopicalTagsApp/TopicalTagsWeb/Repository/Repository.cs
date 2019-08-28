using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TopicalTagsWeb
{
    public class Repository : DbContext
    {
        public Repository(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Topic> Topics { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }

    public class Topic
    {
        public Topic()
        {
            this.Tags = new HashSet<TopicTag>();
        }

        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public virtual ICollection<TopicTag> Tags { get; set; }
    }

    public class Tag
    {
        public Tag()
        {
            this.Topics = new HashSet<TopicTag>();
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        
        
        public virtual ICollection<TopicTag> Topics { get; set; }
    }

    public class TopicTag
    {
        public int TopicId { get; set; }
        public Topic Topic { get; set; }
        public int TagId { get; set; }
        public Tag Tag { get; set; }
    }
}

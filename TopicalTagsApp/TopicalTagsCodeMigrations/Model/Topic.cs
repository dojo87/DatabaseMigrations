using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TopicalTagsCodeMigrations.Model
{
    public partial class Topic
    {
        public Topic()
        {
            TopicTags = new HashSet<TopicTags>();
        }

        public Topic(string title, string url) : this()
        {
            Title = title;
            Url = url;
        }

        [Key]
        public int Id { get; set; }

        [MaxLength(2000)]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string Url { get; set; }

        public virtual ICollection<TopicTags> TopicTags { get; set; }

        public Topic AddTags(params Tag[] tags)
        {
            foreach (Tag tag in tags)
            {
                TopicTags.Add(new TopicTags { Topic = this, Tag = tag });
            }
            return this;
        }
    }
}

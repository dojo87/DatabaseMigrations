using System;
using System.Collections.Generic;

namespace TopicalTagsWebTestDatabaseMigrations.Model
{
    public partial class Topic
    {
        public Topic()
        {
            TopicTags = new HashSet<TopicTags>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }

        public virtual ICollection<TopicTags> TopicTags { get; set; }
    }
}

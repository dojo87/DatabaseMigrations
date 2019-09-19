using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TopicalTagsCodeMigrations.Model
{
    public partial class TopicTags
    {
        [Key]
        public int TopicId { get; set; }
        public int TagId { get; set; }

        public virtual Tag Tag { get; set; }
        public virtual Topic Topic { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TopicalTags.CodeFirst.Model
{
    public partial class TopicTags
    {
        public int TopicId { get; set; }
        public int TagId { get; set; }

        public virtual Tag Tag { get; set; }
        public virtual Topic Topic { get; set; }
    }
}

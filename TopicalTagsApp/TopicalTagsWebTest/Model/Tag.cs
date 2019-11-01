﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TopicalTagsWebTest.Model
{
    public partial class Tag
    {
        public Tag()
        {
            TopicTags = new HashSet<TopicTags>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<TopicTags> TopicTags { get; set; }
    }
}

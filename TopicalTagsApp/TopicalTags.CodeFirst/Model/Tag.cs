using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TopicalTags.CodeFirst.Model
{
    public partial class Tag
    {
        public Tag()
        {
            TopicTags = new HashSet<TopicTags>();
        }

        public Tag(string name) : this()
        {
            Name = name;
        }

        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public virtual ICollection<TopicTags> TopicTags { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TopicalTagsCodeMigrations.Model
{
    public partial class TopicContext
    {
        public void OnSeed(bool isDevelopment)
        {
            Dictionary<int, Tag> tags = new Dictionary<int, Tag>();

            tags.Add(1, new Tag("Answers"));
            tags.Add(2, new Tag("Worldview"));
            tags.Add(3, new Tag("Christianity"));
            tags.Add(4, new Tag("Science"));
            tags.Add(5, new Tag("Biology"));
            tags.Add(6, new Tag("Plants"));
            tags.Add(7, new Tag("Astronomy"));
            tags.Add(8, new Tag("Age of the Universe"));
            tags.Add(9, new Tag("Evolution"));
            tags.Add(10, new Tag("Origin of Life"));
         
            UpsertTags(tags.Values.ToList());

            if (!this.Topics.Any())
            {
                this.Topics.AddRange(
                        new Topic("Origin of Life Problems for Naturalists", "https://answersingenesis.org/origin-of-life/origin-of-life-problems-for-naturalists/")
                        .AddTags(tags[1], tags[9], tags[10]),

                        new Topic("Power Plants", "https://answersingenesis.org/biology/plants/power-plants/")
                        .AddTags(tags[1], tags[3], tags[5], tags[6]),

                        new Topic("Evidence for a Young World", "https://answersingenesis.org/astronomy/age-of-the-universe/evidence-for-a-young-world/")
                        .AddTags(tags[1], tags[4], tags[7], tags[8]),

                        new Topic("Are Atheists Right? Is Faith the Absence of Reason/Evidence?", "https://answersingenesis.org/christianity/are-atheists-right/")
                       .AddTags(tags[1], tags[2], tags[3])
                       );

            }

            this.SaveChanges();

        }

        private void UpsertTags(List<Tag> tags)
        {
            var keys = tags.Select(t => t.Name).ToList();
            var existingTagNameId = this.Tags
                .Where(t => keys.Contains(t.Name))
                .ToDictionary(t => t.Name, t => t.Id);

            tags.ForEach(t =>
            {
                // Get existing Ids
                if (existingTagNameId.ContainsKey(t.Name))
                {
                    t.Id = existingTagNameId[t.Name];
                }
            });

            this.AddRange(tags
                .Where(t => !existingTagNameId.ContainsKey(t.Name)));
        }
    }
}

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

            tags.Add(1, new Tag(1, "Answers"));
            tags.Add(2, new Tag(2, "Worldview"));
            tags.Add(3, new Tag(3, "Christianity"));
            tags.Add(4, new Tag(4, "Science"));
            tags.Add(5, new Tag(5, "Biology"));
            tags.Add(6, new Tag(6, "Plants"));
            tags.Add(7, new Tag(7, "Astronomy"));
            tags.Add(8, new Tag(8, "Age of the Universe"));
            tags.Add(9, new Tag(9, "Evolution"));
            tags.Add(10, new Tag(10, "Origin of Life"));

            UpsertTags(tags);

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

        private void UpsertTags(Dictionary<int, Tag> tags)
        {
            var keys = tags.Keys.ToList();

            var upsertedTags = this.Tags
                .Where(t => keys.Contains(t.Id)).Select(t => t.Id)
                .ToList();

            this.AttachRange(tags.Values.Where(t => upsertedTags.Contains(t.Id)));

            this.AddRange(tags.Values
                .Where(t => !upsertedTags.Contains(t.Id))
                .Select(tag => { tag.Id = 0; return tag; }));
        }
    }
}

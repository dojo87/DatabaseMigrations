using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TopicalTags.CodeFirst.Model
{
    public partial class TopicContext
    {
        public void OnSeed(bool isDevelopment)
        {
            Dictionary<int, Tag> tags = Upsert(new Dictionary<int, Tag>
            {
                { 1, new Tag("Answers") },
                { 2, new Tag("Worldview") },
                { 3, new Tag("Christianity") },
                { 4, new Tag("Science") },
                { 5, new Tag("Biology") },
                { 6, new Tag("Plants") },
                { 7, new Tag("Astronomy") },
                { 8, new Tag("Age of the Universe") },
                { 9, new Tag("Evolution") },
                { 10, new Tag( "Origin of Life") },
                { 11, new Tag( "TEST") }
            }, t => t.Id, t => t.Name);

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

            if (!this.Topics.Any(t => t.Title == "DEV") && isDevelopment)
            {
                this.Topics.Add(new Topic("DEV", "localhost")
                        .AddTags(tags[1], tags[9], tags[10]));
            }

            this.SaveChanges();

        }

        private Dictionary<int, T> Upsert<T,TKey>(Dictionary<int, T> entities, Func<T,int> idSelector, Func<T,TKey> comparatorSelector)
            where T: class, new()
        {
            List<TKey> upsertedKeys = entities.Values.Select(comparatorSelector).ToList();
            var existing = this.Set<T>().ToList().Where(e => upsertedKeys.Contains(comparatorSelector(e))).ToList();
            var newEntities = entities.Values.Where(up => !existing.Any(e => comparatorSelector(e).Equals(comparatorSelector(up)))).ToList();
            Dictionary<TKey, int> idByKey = entities.ToDictionary(e => comparatorSelector(e.Value), e => e.Key);

            this.AddRange(newEntities);

            Dictionary<int, T> attachedEntities = new Dictionary<int, T>();

            existing.ForEach(e => attachedEntities[idByKey[comparatorSelector(e)]] = e);
            newEntities.ForEach(e => attachedEntities[idByKey[comparatorSelector(e)]] = e);

            return attachedEntities;
        }
    }
}

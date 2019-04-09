using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Tinkoff.ISA.Infrastructure.MongoDb;

namespace Tinkoff.ISA.Domain
{
    [Collection("Questions")]
    public class Question
    {
        [BsonId]
        public Guid Id { get; set; }

        public string Text { get; set; }
        
        public DateTime LastUpdate { get; set; }

        public ICollection<Answer> Answers { get; set; }

        public List<string> AskedUsersIds { get; set; }
    }
}

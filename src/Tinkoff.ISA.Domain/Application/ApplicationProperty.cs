using System;
using Tinkoff.ISA.Infrastructure.MongoDb;

namespace Tinkoff.ISA.Domain.Application
{
    [Collection("ApplicationProperties")]
    public class ApplicationProperty
    {
        public DateTime LastMongoIndexing { get; set; }

        public DateTime JiraJobLastUpdate { get; set; }

        public DateTime ConfluenceJobLastUpdate { get; set; }
    }
}

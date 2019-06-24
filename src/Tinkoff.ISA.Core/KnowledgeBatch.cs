using System;
using System.Collections.Generic;
using Tinkoff.ISA.Core.Documents;

namespace Tinkoff.ISA.Core
{
    public class KnowledgeBatch<TDocument> : IKnowledgeBatch<TDocument>
        where TDocument : ISearchableDocument
    {
        public KnowledgeBatch(IEnumerable<TDocument> documents)
        {
            Documents = documents;
        }

        public DateTimeOffset UploadedToDate { get; set; }

        public bool IsLastBatch { get; set; }

        public IEnumerable<TDocument> Documents { get; }
    }
}
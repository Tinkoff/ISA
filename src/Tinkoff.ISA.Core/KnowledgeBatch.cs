using System;
using System.Collections.Generic;
using Tinkoff.ISA.Core.Documents;

namespace Tinkoff.ISA.Core
{
    public class KnowledgeBatch<TDocument> : IKnowledgeBatch<TDocument>
        where TDocument : ISearchableDocument
    {
        public KnowledgeBatch(IEnumerable<TDocument> documents, DateTimeOffset uploadedToDate, bool isLastBatch)
        {
            Documents = documents;
            UploadedToDate = uploadedToDate;
            IsLastBatch = isLastBatch;
        }

        public IEnumerable<TDocument> Documents { get; }
        
        public DateTimeOffset UploadedToDate { get; }

        public bool IsLastBatch { get; }
    }
}
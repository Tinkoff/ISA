using System;
using System.Collections.Generic;
using Tinkoff.ISA.Core.Documents;

namespace Tinkoff.ISA.Core
{
    public interface IKnowledgeBatch<out TDocument>
        where TDocument : ISearchableDocument
    {
        DateTimeOffset UploadedToDate { get; set; }

        bool IsLastBatch { get; set; }
        
        IEnumerable<TDocument> Documents { get; }
    }
}
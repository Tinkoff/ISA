using System;
using System.Collections.Generic;
using Tinkoff.ISA.Core.Documents;

namespace Tinkoff.ISA.Core
{
    public interface IKnowledgeBatch<out TDocument>
        where TDocument : ISearchableDocument
    {
        IEnumerable<TDocument> Documents { get; }
        
        DateTimeOffset UploadedToDate { get; }

        bool IsLastBatch { get; }
    }
}
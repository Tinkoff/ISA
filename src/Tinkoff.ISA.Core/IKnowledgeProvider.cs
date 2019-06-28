using System.Threading.Tasks;
using Tinkoff.ISA.Core.Documents;

namespace Tinkoff.ISA.Core
{
    public interface IKnowledgeProvider
    {
        Task<IKnowledgeBatch<ISearchableDocument>> GetKnowledgeBatch(KnowledgeRequest request);
    }
}

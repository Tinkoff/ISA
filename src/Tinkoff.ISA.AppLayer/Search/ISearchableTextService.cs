using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tinkoff.ISA.Domain.Search;

namespace Tinkoff.ISA.AppLayer.Search
{
    public interface ISearchableTextService
    {
        Task<IList<SearchableQuestion>> GetQuestionsAsync(IEnumerable<Guid> questionsIds);

        Task<IList<SearchableQuestion>> GetQuestionsWithAnswersAsync(DateTime startDate);

        Task<IList<SearchableAnswer>> GetAnswersAsync(DateTime startDate);
    }
}

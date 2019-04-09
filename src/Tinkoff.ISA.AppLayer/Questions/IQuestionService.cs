using System.Collections.Generic;
using System.Threading.Tasks;
using Tinkoff.ISA.Domain;

namespace Tinkoff.ISA.AppLayer.Questions
{
    public interface IQuestionService
    {
        Task<Question> UpsertAsync(Question question);

        Task<Question> GetQuestionAsync(string questionId);

        Task AnswerRankUpAsync(string questionId, string answerId);

        Task AnswerRankDownAsync(string questionId, string answerId);

        Task<ICollection<Answer>> GetAnswersOnQuestionExceptAsync(string questinId, string answerId);

        Task AppendAnswerAsync(string questionId, string answerText);

        Task UnsubscribeNotificationForUser(string questionId, string userId);
    }
}

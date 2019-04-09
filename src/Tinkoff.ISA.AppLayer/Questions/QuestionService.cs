using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tinkoff.ISA.DAL.Storage.Common;
using Tinkoff.ISA.DAL.Storage.Dao.Questions;
using Tinkoff.ISA.Domain;

namespace Tinkoff.ISA.AppLayer.Questions
{
    internal class QuestionService : IQuestionService
    {
        private readonly IRepository<Question, Guid> _questionRepository;
        private readonly IQuestionDao _questionDao;

        public QuestionService(
            IRepository<Question, Guid> questionRepository,
            IQuestionDao questionDao)
        {
            _questionRepository = questionRepository;
            _questionDao = questionDao;
        }

        public Task<Question> UpsertAsync(Question question)
        {
            if (question == null) throw new ArgumentException(nameof(question));
            
            return _questionDao.UpsertAsync(q => q.Text == question.Text, question);
        }

        public Task<Question> GetQuestionAsync(string questionId)
        {
            if (string.IsNullOrEmpty(questionId))
                throw new ArgumentException(nameof(questionId));

            return _questionRepository.FirstOrDefault(q => q.Id.Equals(new Guid(questionId)));
        }

        public Task AnswerRankUpAsync(string questionId, string answerId)
        {
            return _questionDao.UpdateRankAsync(questionId, answerId, 1);
        }

        public Task AnswerRankDownAsync(string questionId, string answerId)
        {
            return _questionDao.UpdateRankAsync(questionId, answerId, -1);
        }

        public async Task<ICollection<Answer>> GetAnswersOnQuestionExceptAsync(string questionId, string answerId)
        {
            var questionWithAnswers = await _questionRepository.FirstOrDefault(o => o.Id == new Guid(questionId));
            
            return questionWithAnswers.Answers
                .Where(o => o.Id != new Guid(answerId))
                .OrderByDescending(x => x.Rank).ToArray();
        }

        public Task AppendAnswerAsync(string questionId, string answerText)
        {
            var answer = new Answer
            {
                Id = Guid.NewGuid(),
                Rank = 0,
                Text = answerText,
                LastUpdate = DateTime.UtcNow
            };

            return _questionDao.PushNewItemAsync(kn => kn.Id == new Guid(questionId), kn => kn.Answers, answer);
        }

        public Task UnsubscribeNotificationForUser(string questionId, string userId)
        {
            return _questionDao.RemoveOneItemAsync(kn => kn.Id == new Guid(questionId), kn => kn.AskedUsersIds, userId);
        }
    }
}

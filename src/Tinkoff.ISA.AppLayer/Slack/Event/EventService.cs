using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tinkoff.ISA.AppLayer.Search;
using Tinkoff.ISA.AppLayer.Slack.Buttons;
using Tinkoff.ISA.AppLayer.Slack.Common;
using Tinkoff.ISA.AppLayer.Slack.Event.ButtonParams;
using Tinkoff.ISA.AppLayer.Slack.Event.Request;
using Tinkoff.ISA.DAL.Elasticsearch.Request;
using Tinkoff.ISA.DAL.Elasticsearch.Services;
using Tinkoff.ISA.DAL.Slack;
using Tinkoff.ISA.DAL.Slack.Dtos;
using Tinkoff.ISA.Domain.Search;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace Tinkoff.ISA.AppLayer.Slack.Event
{
    internal class EventService : IEventService
    {
        private readonly ISlackHttpClient _slackClient;
        private readonly IElasticSearchService _elasticSearchService;
        private readonly ISearchableTextService _searchableTextService;
        private readonly ILogger<EventService> _logger;
        private const string SearchInAnswersResultText = "Perhaps you will find the answer here:";
        private const string SearchInConfluenceResultText = "Confluence search results";
        private const string SearchInJiraResultText = "Jira search results";
        private const string CircleSymbol = "\u26ac";
        private const int NecessaryNumberOfQuestions = 5;
        private const int NecessaryNumberOfAnswers = 5;
        private const int NecessaryNumberOfConfluencePages = 5;
        private const int NecessaryNumberOfJiraIssues = 5; 

        public EventService(ISlackHttpClient slackClient, IElasticSearchService elasticSearchService,
            ISearchableTextService searchableTextService, ILogger<EventService> logger)
        {
            _slackClient = slackClient;
            _elasticSearchService = elasticSearchService;
            _searchableTextService = searchableTextService;
            _logger = logger;
        }

        private static AttachmentDto CreateQuestionAttachment(SearchableQuestion question)
        {
            var showAnswersParams = JsonConvert.SerializeObject(new ShowAnswersActionButtonParams
            {
                QuestionId = question.Id
            });

            return new AttachmentDto
            {
                Color = Color.Turquoise,
                Title = $"{Phrases.QuestionInfoTitle}{question.Text}",
                CallbackId = CallbackId.ShowAnswersButtonId,
                Actions = new List<AttachmentActionDto> { new ShowAnswersButtonAttachmentAction(showAnswersParams) }
            };
        }

        private static AttachmentDto CreateConfluenceAttachment(IEnumerable<SearchableConfluence> confluencePages)
        {
            var fields = confluencePages.Select(p => new FieldDto
            {
                Value = $"{CircleSymbol} <{p.Link}|{p.Title}>"
            }).ToList();

            return new AttachmentDto
            {
                Title = SearchInConfluenceResultText,
                Fields = fields,
                Color = Color.LightCobaltBlue
            };
        }

        private static AttachmentDto CreateJiraAttachment(IEnumerable<SearchableJira> jiraIssues)
        {
            var fields = jiraIssues.Select(p => new FieldDto
            {
                Value = $"{CircleSymbol} <{p.Link}|{p.Title}>"
            }).ToList();

            return new AttachmentDto
            {
                Title = SearchInJiraResultText,
                Fields = fields,
                Color = Color.LightPurpleBlue
            };
        }

        private static AttachmentDto CreateAskExpertsAttachment(string requestText)
        {
            return new AttachmentDto
            {
                Color = Color.Sand,
                Text = $"{Phrases.QuestionInfoText}*{requestText}*\n_{Phrases.AskExperts}_",
                CallbackId = CallbackId.StandardButtonsId,
                Actions = new List<AttachmentActionDto> { new AskExpertsButtonAttachmentAction(requestText) }
            };
        }

        private static AttachmentDto CreateAnswersAttachment(IEnumerable<SearchableAnswer> searchableAnswers,
            IEnumerable<SearchableQuestion> searchableQuestions)
        {
            var fields = searchableAnswers
                .Join(searchableQuestions, a => a.QuestionId, q => q.Id,
                    (a, q) => new {QuestionText = q.Text, AnswerText = a.Text})
                .Select(questionWithAnswer =>
                    new FieldDto
                    {
                        Title = $"{CircleSymbol} {questionWithAnswer.QuestionText}",
                        Value = questionWithAnswer.AnswerText
                    })
                .ToList();

            return new AttachmentDto
            {
                Title = SearchInAnswersResultText,
                Fields = fields,
                Color = Color.LightScarlet
            };
        }

        private static void AddToAttachments(ICollection<AttachmentDto> result, AttachmentDto insertingAttachment)
        {
            if (insertingAttachment.Fields.Count != 0)
                result.Add(insertingAttachment);
        }

        public async Task ProcessRequest(EventWrapperRequest request)
        {
            try
            {
                await FindSimilar(request);
            }
            catch (Exception e)
            {
                if (request?.Event?.Channel != null)
                    await _slackClient.SendMessageAsync(request.Event.Channel, Phrases.SlackError);
                _logger.LogError(e, e.Message);
            }
        }

        private async Task FindSimilar(EventWrapperRequest request)   
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Event.Type != "message") return;
            // ignore bot answers
            if (request.Event.BotId != null) return;
            var askedQuestion = request.Event?.Text;
            if (string.IsNullOrWhiteSpace(askedQuestion)) return;

            _logger.LogInformation("User with id {UserId} asked a question {Question}",
                request.Event.UserId, askedQuestion);

            var attachments = new List<AttachmentDto>();

            // questions
            var similarQuestions = await Search<QuestionElasticSearchRequest, SearchableQuestion>(
                askedQuestion, NecessaryNumberOfQuestions,
                _elasticSearchService.SearchAsync<SearchableQuestion>);

            attachments.AddRange(similarQuestions.Select(CreateQuestionAttachment));

            // confluence
            var confluencePages = await Search<ConfluenceElasticSearchRequest, SearchableConfluence>(
                askedQuestion, NecessaryNumberOfConfluencePages,
                _elasticSearchService.SearchWithTitleAsync<SearchableConfluence>);

            AddToAttachments(attachments, CreateConfluenceAttachment(confluencePages));

            // jira
            var jiraIssues = await Search<JiraElasticSearchRequest, SearchableJira>(
                askedQuestion, NecessaryNumberOfJiraIssues,
                _elasticSearchService.SearchWithTitleAsync<SearchableJira>);

            AddToAttachments(attachments, CreateJiraAttachment(jiraIssues));

            // answers
            if (similarQuestions.Count < NecessaryNumberOfQuestions)
            {
                var searchableAnswers = await Search<AnswerElasticSearchRequest, SearchableAnswer>(
                    askedQuestion, NecessaryNumberOfAnswers,
                    _elasticSearchService.SearchAsync<SearchableAnswer>);

                var searchableQuestions = await _searchableTextService
                    .GetQuestionsAsync(searchableAnswers.Select(q => Guid.Parse(q.QuestionId)));

                AddToAttachments(attachments, CreateAnswersAttachment(searchableAnswers, searchableQuestions));
            }

            // ask experts
            attachments.Add(CreateAskExpertsAttachment(askedQuestion));

            await _slackClient.SendMessageAsync(request.Event.Channel, 
                similarQuestions.Count != 0 ? Phrases.SimilarQuestions : null, 
                attachments);
        }

        private static Task<IList<TResponse>> Search<TRequest, TResponse>(string text, int count, Func<TRequest, Task<IList<TResponse>>> searchFunc)
            where TRequest : ElasticSearchRequest
            where TResponse : SearchableText
        {
            var request = Activator.CreateInstance<TRequest>();
            request.Text = text;
            request.Offset = 0;
            request.Count = count;
            return searchFunc(request);
        }
    }
}

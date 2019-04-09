using Microsoft.Extensions.DependencyInjection;
using Tinkoff.ISA.AppLayer.Questions;
using Tinkoff.ISA.AppLayer.Search;
using Tinkoff.ISA.AppLayer.Slack;
using Tinkoff.ISA.AppLayer.Slack.Common;
using Tinkoff.ISA.AppLayer.Slack.Dialogs;
using Tinkoff.ISA.AppLayer.Slack.Dialogs.Submissions;
using Tinkoff.ISA.AppLayer.Slack.Event;
using Tinkoff.ISA.AppLayer.Slack.Executing;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;
using Tinkoff.ISA.AppLayer.Slack.Routing;
using Tinkoff.ISA.AppLayer.Slack.TypeChooser.ActionParams;
using Tinkoff.ISA.AppLayer.Slack.TypeChooser.DialogSubmission;
using Tinkoff.ISA.AppLayer.Slack.Verification;

namespace Tinkoff.ISA.AppLayer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppDependencies(this IServiceCollection services)
        {
            services.AddSingleton<IEventService, EventService>();
            services.AddSingleton<IQuestionService, QuestionService>();
            services.AddSingleton<ISearchableTextService, SearchableTextService>();
            services.AddSingleton<ISlackExecutorService, SlackExecutorService>();
            services.AddSingleton<ISlackParamsSelectService, SlackParamsSelectService>();
            services.AddSingleton<ISubmissionSelectService, SubmissionSelectService>();
            services.AddSingleton<ICallbackIdCustomParamsWrappingService, CallbackIdCustomParamsWrappingService>();

            // scoped because of injecting IMapper
            services.AddScoped<IInteractiveMessageService, InteractiveMessageService>();
            services.AddScoped<IRoutingService, RoutingService>();

            services.AddSingleton<ISlackActionHandler<AskExpertsSlackActionParams>, AskExpertsSlackActionHandler>();
            services.AddSingleton<ISlackActionHandler<HelpedSlackActionParams>, HelpedSlackActionHandler>();
            services.AddSingleton<ISlackActionHandler<NotHelpedSlackActionParams>, NotHelpedSlackActionHandler>();
            services.AddSingleton<ISlackActionHandler<ShowMoreAnswersSlackActionParams>, ShowMoreAnswersSlackActionHandler>();
            services.AddSingleton<ISlackActionHandler<AnswerSlackActionParams>, AnswerSlackActionHandler>();
            services.AddSingleton<ISlackActionHandler<AddAnswerSlackActionParams>, AddAnswerSlackActionHandler>();
            services.AddSingleton<ISlackActionHandler<ShowAnswersSlackActionParams>, ShowAnswersSlackActionHandler>();
            services.AddSingleton<ISlackActionHandler<StopNotificationsSlackActionParams>, StopNotifyUserSlackActionHandler>();

            services.AddSingleton<IDialogSubmissionService<DialogSubmission<AddAnswerSubmission>>, AddAnswerDialogSubmissionService>();
            services.AddSingleton<IQuestionService, QuestionService>();
            services.AddSingleton<ISlackRequestVerifier, SlackRequestVerifier>();

            return services;
        }
    }
}

using System;
using Tinkoff.ISA.AppLayer.Slack.Buttons;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params;
using Tinkoff.ISA.AppLayer.Slack.TypeChooser.ActionParams;
using Xunit;

namespace Tinkoff.ISA.AppLayer.UnitTests.Slack.TypeChooser.ActionParams
{
    public class SlackParamsSelectServiceTests
    {
        private readonly SlackParamsSelectService _service;

        public SlackParamsSelectServiceTests()
        {
            _service = new SlackParamsSelectService();
        }

        [Theory]
        [InlineData(HelpedButtonAttachmentAction.ActionName, typeof(HelpedSlackActionParams))]
        [InlineData(NotHelpedButtonAttachmentAction.ActionName, typeof(NotHelpedSlackActionParams))]
        [InlineData(AskExpertsButtonAttachmentAction.ActionName, typeof(AskExpertsSlackActionParams))]
        [InlineData(ShowMoreAnswersButtonAttachmentAction.ActionName, typeof(ShowMoreAnswersSlackActionParams))]
        [InlineData(AnswerButtonAttachmentAction.ActionName, typeof(AnswerSlackActionParams))]
        [InlineData(AddAnswerButtonAttachmentAction.ActionName, typeof(AddAnswerSlackActionParams))]
        [InlineData(ShowAnswersButtonAttachmentAction.ActionName, typeof(ShowAnswersSlackActionParams))]
        public void Choose_CorrectActionNames_ShouldReturnCorrectType(string actionName, Type expectedReturn)
        {
            //act
            var actualReturn = _service.Choose(actionName);

            //assert 
            Assert.Equal(expectedReturn, actualReturn);
        }

        [Fact]
        public void Choose_InccorectActionName_ThrowArgumentException()
        {
            //act, assert
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => _service.Choose("error"));
        }
    }
}

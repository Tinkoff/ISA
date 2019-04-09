using System;
using Tinkoff.ISA.AppLayer.Slack.Common;
using Tinkoff.ISA.AppLayer.Slack.Dialogs.Submissions;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;
using Tinkoff.ISA.AppLayer.Slack.TypeChooser.DialogSubmission;
using Xunit;

namespace Tinkoff.ISA.AppLayer.UnitTests.Slack.TypeChooser.DialogSubmission
{
     public class SubmissionSelectServiceTests
    {
        private readonly SubmissionSelectService _service;

        public SubmissionSelectServiceTests()
        {
            _service = new SubmissionSelectService();
        }

        [Theory]
        [InlineData(CallbackId.AddAnswerDialogSubmissionId, typeof(DialogSubmission<AddAnswerSubmission>))]
        public void Choose_Correc_ShouldReturnCorrectType(string actionName, Type expectedReturn)
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


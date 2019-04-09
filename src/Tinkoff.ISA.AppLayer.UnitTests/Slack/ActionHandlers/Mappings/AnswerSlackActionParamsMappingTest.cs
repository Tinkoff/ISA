using System.Collections.Generic;
using AutoMapper;
using Newtonsoft.Json;
using Tinkoff.ISA.AppLayer.Slack.Event.ButtonParams;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Mappings;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.ActionHandlers.Params;
using Tinkoff.ISA.AppLayer.Slack.InteractiveMessages.Request;
using Tinkoff.ISA.DAL.Slack.Dtos;
using Xunit;

namespace Tinkoff.ISA.AppLayer.UnitTests.Slack.ActionHandlers.Mappings
{
    public class AnswerSlackActionParamsMappingTest
    {
        private readonly IMapper _mapper;

        public AnswerSlackActionParamsMappingTest()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<AnswerSlackActionParamsMapping>());
            _mapper = new Mapper(config);
        }

        [Fact]
        public void Map__InteractiveMessage_To_AnswerSlackActionParams__AllNecessaryFieldsAreMapped()
        {
            // Arrange
            const string buttonValue = "value";
            var firstAction = new AttachmentActionDto("name", "text")
            {
                Value = JsonConvert.SerializeObject(new AnswerActionButtonParams { QuestionId = buttonValue })
            };

            var originalMessage = new OriginalMessageDto
            {
                Text = "attachmentText",
                TimeStamp = "1212132421",
                Attachments = new List<AttachmentDto>()
            };

            var source = new InteractiveMessage
            {
                Actions = new List<AttachmentActionDto> { firstAction },
                User = new ItemInfo { Id = "id", Name = "userName" },
                OriginalMessage = originalMessage
            };

            // Act
            var destination = _mapper.Map<InteractiveMessage, AnswerSlackActionParams>(source);

            // Assert
            Assert.Equal(source.User.Name, destination.User.Name);
            Assert.Equal(source.User.Id, destination.User.Id);
            Assert.Equal(buttonValue, destination.ButtonParams.QuestionId);
            Assert.Equal(source.OriginalMessage.Text, destination.OriginalMessage.Text);
            Assert.Equal(source.OriginalMessage.TimeStamp, destination.OriginalMessage.TimeStamp);
            Assert.Equal(source.OriginalMessage.Attachments, destination.OriginalMessage.Attachments);
        }
    }
}

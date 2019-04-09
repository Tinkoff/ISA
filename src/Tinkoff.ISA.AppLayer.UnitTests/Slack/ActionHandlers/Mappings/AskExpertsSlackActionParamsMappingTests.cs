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
    public class AskExpertsSlackActionParamsMappingTests
    {
        private readonly IMapper _mapper;

        public AskExpertsSlackActionParamsMappingTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<AskExpertsSlackActionParamsMapping>());
            _mapper = new Mapper(config);
        }

        [Fact]
        public void Map__InteractiveMessage_To_AskExpertsSlackActionParams__AllNecessaryFieldsAreMapped()
        {
            // Arrange
            var firstAction = new AttachmentActionDto("name", "text")
            {
                Value = "value"
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
                Channel = new ItemInfo { Id = "channelId", Name = "channelName"},
                AttachmentId = 1,
                OriginalMessage = originalMessage
            };

            // Act
            var destination = _mapper.Map<InteractiveMessage, AskExpertsSlackActionParams>(source);

            // Assert
            Assert.Equal(source.User.Name, destination.User.Name);
            Assert.Equal(source.User.Id, destination.User.Id);
            Assert.Equal(source.Channel.Name, destination.Channel.Name);
            Assert.Equal(source.Channel.Id, destination.Channel.Id);
            Assert.Equal(source.AttachmentId - 1, destination.AttachmentId);
            Assert.Equal(firstAction.Value, destination.ButtonParams.QuestionText);
            Assert.Equal(source.OriginalMessage.Text, destination.OriginalMessage.Text);
            Assert.Equal(source.OriginalMessage.TimeStamp, destination.OriginalMessage.TimeStamp);
            Assert.Equal(source.OriginalMessage.Attachments, destination.OriginalMessage.Attachments);
        }
    }
}

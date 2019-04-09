using System.Collections.Generic;
using System.Threading.Tasks;
using Tinkoff.ISA.DAL.Slack.Dtos;

namespace Tinkoff.ISA.DAL.Slack
{
    public interface ISlackHttpClient
    {
        Task SendMessageAsync(string channelId, string message, IList<AttachmentDto> attachments = null);

        Task UpdateMessageAsync(string timestampOfMessageToUpdate, string channelId, string message, IList<AttachmentDto> attachments = null);

        Task OpenDialogAsync(DialogRequest dialogRequest);

        Task<ChannelDto> OpenDirectMessageChannelAsync(string userId);
    }
}

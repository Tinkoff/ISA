using System.Collections.Generic;

namespace Tinkoff.ISA.DAL.Slack.Dtos
{
    public class AttachmentDto
    {
        public string CallbackId { get; set; }

        public string Fallback { get; set; }

        public string Color { get; set; }

        public string Pretext { get; set; }

        public string AuthorName { get; set; }

        public string AuthorLink { get; set; }

        public string AuthorIcon { get; set; }

        public string Title { get; set; }

        public string TitleLink { get; set; }

        public string Text { get; set; }

        public IList<FieldDto> Fields { get; set; }

        public string ImageUrl { get; set; }

        public string ThumbUrl { get; set; }

        public IList<string> MrkdwnIn { get; set; }

        public IList<AttachmentActionDto> Actions { get; set; }
    }
}

using Microsoft.AspNetCore.Http;

namespace Tinkoff.ISA.AppLayer.Slack.Verification
{
    public interface ISlackRequestVerifier
    {
        bool Verify(IHeaderDictionary headers, string rawBody);
    }
}

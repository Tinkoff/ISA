using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tinkoff.ISA.AppLayer.Slack.Routing;
using Tinkoff.ISA.AppLayer.Slack.Verification;

namespace Tinkoff.ISA.API.Controllers.Slack
{
    [Route("api/slack/[controller]")]
    [ApiController]
    public class InteractiveMessagesController : ControllerBase
    {
        private readonly IRoutingService _routingService;
        private readonly ISlackRequestVerifier _verifier;

        public InteractiveMessagesController(IRoutingService routingService, ISlackRequestVerifier verifier)
        {
            _routingService = routingService;
            _verifier = verifier;
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> Post()
        {
            string rawBody;
            using (var reader = new StreamReader(Request.Body))
            {
                rawBody = await reader.ReadToEndAsync();
            }

            var decoded = WebUtility.UrlDecode(rawBody);
            var jsonString = decoded.Split("=", 2).ElementAt(1);
            if (!_verifier.Verify(Request.Headers, rawBody))
                return BadRequest();

            await _routingService.Route(jsonString);
            return Ok();
        }
    }
}

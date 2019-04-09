using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Tinkoff.ISA.AppLayer.Slack.Event;
using Tinkoff.ISA.AppLayer.Slack.Event.Request;
using Tinkoff.ISA.AppLayer.Slack.Verification;
using Tinkoff.ISA.Infrastructure.Settings;

namespace Tinkoff.ISA.API.Controllers.Slack
{
    [Route("api/slack/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly ISlackRequestVerifier _verifier;
        private readonly JsonSerializerSettings _defaultSlackSerializerSettings;

        public EventsController(IEventService eventService, ISlackRequestVerifier verifier)
        {
            _eventService = eventService;
            _verifier = verifier;
            _defaultSlackSerializerSettings = SlackSerializerSettings.DefaultSettings;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            string rawBody;
            using (var reader = new StreamReader(Request.Body))
            {
                rawBody = await reader.ReadToEndAsync();
                if (!_verifier.Verify(Request.Headers, rawBody))
                    return BadRequest();
            }

            var eventWrapper = JsonConvert.DeserializeObject<EventWrapperRequest>(rawBody, _defaultSlackSerializerSettings);

            // url verification for slack
            // https://api.slack.com/events-api#request_url_configuration__amp__verification
            if (eventWrapper.Type.Equals("url_verification"))
                return Ok(eventWrapper.Challenge);

            if (ModelState.IsValid)
                // It's necessary to confirm request immediately
                // In order to prevent automatic disabling
                // https://api.slack.com/events-api#responding_to_events
                await Task.Run(() => { _eventService.ProcessRequest(eventWrapper); });

            return Ok();
        }
    }
}

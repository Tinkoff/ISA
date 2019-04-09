using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Tinkoff.ISA.Infrastructure.Settings;

namespace Tinkoff.ISA.AppLayer.Slack.Verification
{
    internal class SlackRequestVerifier : ISlackRequestVerifier
    {
        private readonly string _signingSecret;
            
        public SlackRequestVerifier(IOptions<SlackSettings> settings)
        {
            _signingSecret = settings.Value.SigningSecret;
        }

        public bool Verify(IHeaderDictionary headers, string rawBody)
        {
            if (headers == null) throw new ArgumentNullException(nameof(headers));
            if (rawBody == null) throw new ArgumentNullException(nameof(rawBody));

            var timestamp = headers["X-Slack-Request-Timestamp"];
            var slackSignature = headers["X-Slack-Signature"];
            var sigBaseString = "v0:" + timestamp + ":" + rawBody;

            using (var hash = new HMACSHA256(Encoding.UTF8.GetBytes(_signingSecret)))
            {
                var result = hash.ComputeHash(Encoding.UTF8.GetBytes(sigBaseString));
                var actualSignature = "v0=" + ByteArrayToString(result);
                return actualSignature.Equals(slackSignature);
            }
        }

        private static string ByteArrayToString(byte[] byteArray)
        {
            var hex = new StringBuilder(byteArray.Length * 2);
            foreach (var b in byteArray)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Tinkoff.ISA.AppLayer.Slack.Verification;
using Tinkoff.ISA.Infrastructure.Settings;
using Xunit;

namespace Tinkoff.ISA.AppLayer.UnitTests.Slack.Verification
{
    public class SlackRequestVerifierTests
    {
        private const string SlackTimestampKey = "X-Slack-Request-Timestamp";
        private const string SlackSignatureKey = "X-Slack-Signature";
        private const string SigningSecret = "secret";
        
        private readonly SlackRequestVerifier _verifier;
        private readonly Mock<IHeaderDictionary> _headerDictionaryMock;

        public SlackRequestVerifierTests()
        {
            _headerDictionaryMock = new Mock<IHeaderDictionary>();
            var slackSettingOptionsMock = new Mock<IOptions<SlackSettings>>();
            slackSettingOptionsMock
                .SetupGet(m => m.Value)
                .Returns(() => new SlackSettings
                {
                    SigningSecret = SigningSecret
                });

            _verifier = new SlackRequestVerifier(slackSettingOptionsMock.Object);
        }

        [Fact]
        public void Verify_HeadersAreNull_ArgumentNullException()
        {
            // Act-Assert
            Assert.Throws<ArgumentNullException>(() => _verifier.Verify(null, "body goes here"));
        }

        [Fact]
        public void Verify_RawBodyIsNull_ArgumentNullException()
        {
            // Act-Assert
            Assert.Throws<ArgumentNullException>(() => _verifier.Verify(_headerDictionaryMock.Object, null));
        }

        [Fact]
        public void Verify_ActualSignatureIsEqualToSlackOne_ShouldReturnTrue()
        {
            // Arrange
            const string timestamp = "timestamp";
            const string rawBody = "foo bar";

            var signature = ComputeSignature(timestamp, rawBody);
                
            _headerDictionaryMock.SetupGet(m => m[SlackTimestampKey]).Returns(timestamp);
            _headerDictionaryMock.SetupGet(m => m[SlackSignatureKey]).Returns(signature);
            
            // Act
            var result = _verifier.Verify(_headerDictionaryMock.Object, rawBody);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Verify_ActualSignatureIsNotEqualToSlackOne_ShouldReturnFalse()
        {
            // Arrange
            const string timestamp = "timestamp";
            const string signature = "wrong signature";
            const string rawBody = "foo bar";

            _headerDictionaryMock.SetupGet(m => m[SlackTimestampKey]).Returns(timestamp);
            _headerDictionaryMock.SetupGet(m => m[SlackSignatureKey]).Returns(signature);

            // Act
            var result = _verifier.Verify(_headerDictionaryMock.Object, rawBody);

            // Assert
            Assert.False(result);
        }

        private static string ComputeSignature(string timestamp, string rawBody)
        {
            var sigBaseString = $"v0:{timestamp}:{rawBody}";
            
            using (var hash = new HMACSHA256(Encoding.UTF8.GetBytes(SigningSecret)))
            {
                var result = hash.ComputeHash(Encoding.UTF8.GetBytes(sigBaseString));
                var hexString = ByteArrayToString(result);
                return $"v0={hexString}";
            }
        }
        
        private static string ByteArrayToString(IReadOnlyCollection<byte> byteArray)
        {
            var hex = new StringBuilder(byteArray.Count * 2);
            
            foreach (var b in byteArray)
                hex.AppendFormat("{0:x2}", b);
            
            return hex.ToString();
        }
    }
}

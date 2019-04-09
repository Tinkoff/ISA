using System;
using System.Collections.Generic;
using System.Linq;
using Tinkoff.ISA.AppLayer.Slack.Common;
using Xunit;

namespace Tinkoff.ISA.AppLayer.UnitTests.Slack.Common
{
    public class CallbackIdCustomParamsWrapperTests
    {
        private readonly CallbackIdCustomParamsWrappingService _wrappingService;

        public CallbackIdCustomParamsWrapperTests()
        {
            _wrappingService = new CallbackIdCustomParamsWrappingService();
        }

        [Fact]
        public void Wrap_CallbackIdIsNull_ArgumentNullException()
        {
            // Act-Assert
            Assert.Throws<ArgumentNullException>(() => _wrappingService.Wrap(null, new List<string>()));
        }

        [Fact]
        public void Wrap_CustomParamsCollectionIsNull_ArgumentNullException()
        {
            // Act-Assert
            Assert.Throws<ArgumentNullException>(() => _wrappingService.Wrap("callbackId", null));
        }

        [Fact]
        public void Wrap_ResultCallbackIdExceededMaxLength_ArgumentException()
        {
            // Arrange
            const string callbackId = "callbackId";
            const int maxLength = 200;
            var customParams = new List<string>{new string('a', maxLength - callbackId.Length + 1)};

            // Act-Assert
            Assert.Throws<ArgumentException>(() => _wrappingService.Wrap(callbackId, customParams));
        }

        [Fact]
        public void WrapUnwrap_JustInvoked_ShouldReturnSameParam()
        {
            // Arrange
            const string callbackId = "callbackId";
            const string myParam = "param12345";

            // Act
            var wrappedParams = _wrappingService.Wrap(callbackId, new[] {myParam});
            var result = _wrappingService.Unwrap(wrappedParams);

            // Assert
            Assert.Single(result);
            Assert.Equal(myParam, result.First());
        }
    }
}
